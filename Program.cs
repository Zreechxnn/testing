using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using testing.Hubs;
using testing.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using testing.Middleware;
using testing.Services;
using testing.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using DotNetEnv;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Logging;

IdentityModelEventSource.ShowPII = true;
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

Env.Load();
builder.Configuration.AddEnvironmentVariables();

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<LabDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.EnableSensitiveDataLogging();
});
Console.WriteLine("✅ Database Provider: PostgreSQL");

var jwtSecretKey = Environment.GetEnvironmentVariable("JwtSettings__SecretKey")?.Trim();
if (string.IsNullOrEmpty(jwtSecretKey))
{
    throw new Exception("🔥 FATAL ERROR: JWT Secret Key tidak ditemukan di .env!");
}
var key = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lab Access API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            new string[] { }
        }
    });
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddSignalR();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IKartuService, KartuService>();
builder.Services.AddScoped<IAksesLogService, AksesLogService>();
builder.Services.AddScoped<IKelasService, KelasService>();
builder.Services.AddScoped<IRuanganService, RuanganService>();
builder.Services.AddScoped<ITapService, TapService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IKartuRepository, KartuRepository>();
builder.Services.AddScoped<IAksesLogRepository, AksesLogRepository>();
builder.Services.AddScoped<IKelasRepository, KelasRepository>();
builder.Services.AddScoped<IRuanganRepository, RuanganRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var corsOrigins = Environment.GetEnvironmentVariable("CORS__Origins");
builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", p =>
    {
        if (string.IsNullOrEmpty(corsOrigins) || corsOrigins == "*")
        {
            Console.WriteLine("[⚠️ CORS WARNING] Mode Wildcard Aktif");
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()).ToArray();
            Console.WriteLine($"[✅ CORS ACTIVE] Whitelisted: {string.Join(", ", origins)}");
            p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    }));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    var validationParams = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "LabAccessAPI",
        ValidAudience = "LabAccessClient",
        IssuerSigningKey = new SymmetricSecurityKey(key),
        NameClaimType = "name",
        RoleClaimType = "role",
        ClockSkew = TimeSpan.Zero
    };

    options.TokenValidationParameters = validationParams;

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Headers["Authorization"].ToString();
            var path = context.HttpContext.Request.Path;

            // A. Cek Token dari Header (Fetch API)
            if (!string.IsNullOrEmpty(accessToken) && accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var rawToken = accessToken.Substring("Bearer ".Length).Trim();
                // FIX: Hapus tanda kutip ganda jika ada
                context.Token = rawToken.Replace("\"", "");
            }

            // B. Cek Token dari URL (SignalR WebSocket)
            else if (string.IsNullOrEmpty(context.Token))
            {
                var tokenFromQuery = context.Request.Query["access_token"].ToString();

                // Pastikan ada token & path menuju hub SignalR
                if (!string.IsNullOrEmpty(tokenFromQuery) &&
                    path.StartsWithSegments("/logHub", StringComparison.OrdinalIgnoreCase))
                {
                    // FIX: Hapus tanda kutip ganda jika ada
                    context.Token = tokenFromQuery.Replace("\"", "");
                }
            }

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[🔥 SYSTEM FAIL] Auth Failed: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lab Access API v1");
    c.RoutePrefix = "swagger";
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Use(async (context, next) =>
{
    var requestPath = context.Request.Path.Value?.ToLower();

    // Bypass untuk Swagger, Root, DAN SignalR
    if (requestPath != null && (
        requestPath.Contains("/swagger") ||
        requestPath == "/" ||
        requestPath.StartsWith("/loghub")
    ))
    {
        await next();
        return;
    }

    var origin = context.Request.Headers["Origin"].ToString();
    var referer = context.Request.Headers["Referer"].ToString();
    var allowedOrigins = corsOrigins?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(o => o.Trim().TrimEnd('/')).ToList();

    Console.WriteLine($"[👮 SATPAM CEK] Path: {requestPath} | Origin: '{origin}'");

    if (string.IsNullOrEmpty(origin) && string.IsNullOrEmpty(referer))
    {
        Console.WriteLine("[ℹ️ NON-BROWSER] Request tanpa identitas. Diizinkan.");
    }

    else if (!string.IsNullOrEmpty(origin) && allowedOrigins != null)
    {
        if (!allowedOrigins.Any(o => string.Equals(o, origin, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine($"[⛔ DITOLAK] Origin '{origin}' tidak terdaftar!");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Akses Ditolak.");
            return;
        }
        Console.WriteLine("[✅ BROWSER VALID] Silakan masuk.");
    }

    await next();
});

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<LogHub>("/logHub");

await app.RunAsync();