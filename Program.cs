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
// Load .env
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// Database
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<LabDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.EnableSensitiveDataLogging();
});
Console.WriteLine("✅ Database Provider: PostgreSQL");

// JWT Secret
var jwtSecretKey = Environment.GetEnvironmentVariable("JwtSettings__SecretKey")?.Trim();
if (string.IsNullOrEmpty(jwtSecretKey))
{
    throw new Exception("🔥 FATAL ERROR: JWT Secret Key tidak ditemukan di .env!");
}
var key = Encoding.UTF8.GetBytes(jwtSecretKey);

//  Controller & JSON
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Swagger
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
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

// Services & Repositories
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddSignalR(); // SignalR Service
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

// CORS Configuration
var corsOrigins = Environment.GetEnvironmentVariable("CORS__Origins");
Console.WriteLine($"[🔒 CORS CONFIG] Raw Value: '{corsOrigins}'");

builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", p =>
    {
        if (string.IsNullOrEmpty(corsOrigins) || corsOrigins == "*")
        {
            Console.WriteLine("[⚠️ CORS WARNING] Mode Wildcard Aktif");
            p.SetIsOriginAllowed(_ => true) // Allow any origin explicitly for SignalR
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials(); // SignalR butuh ini
        }
        else
        {
            var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(o => o.Trim())
                                     .ToArray();
            Console.WriteLine($"[✅ CORS ACTIVE] Whitelisted: {string.Join(", ", origins)}");

            p.WithOrigins(origins)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials();
        }
    }));

// Authentication & SignalR Token Logic
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
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/logHub"))
            {
                context.Token = accessToken;
                Console.WriteLine($"[SIGNALR AUTH] Token didapat dari URL untuk hub.");
                return Task.CompletedTask;
            }

            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = authHeader.Substring("Bearer ".Length).Trim().Trim('"');
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[🔥 AUTH FAIL] {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();
// Middleware Pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lab Access API v1");
    c.RoutePrefix = "swagger";
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowFrontend");

app.Use(async (context, next) =>
{
    var requestPath = context.Request.Path.Value?.ToLower();

    if (requestPath != null && (requestPath.Contains("/swagger") || requestPath == "/"))
    {
        await next();
        return;
    }

    if (context.Request.Method == "OPTIONS")
    {
        await next();
        return;
    }

    var origin = context.Request.Headers["Origin"].ToString();
    var referer = context.Request.Headers["Referer"].ToString();

    Console.WriteLine($"[👮 SATPAM] {context.Request.Method} {requestPath} | Origin: {origin}");

    var corsOriginsEnv = Environment.GetEnvironmentVariable("CORS__Origins");
    if (!string.IsNullOrEmpty(corsOriginsEnv) && corsOriginsEnv != "*" && !string.IsNullOrEmpty(origin))
    {
        var allowedOrigins = corsOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(o => o.Trim().TrimEnd('/'));

        bool isAllowed = allowedOrigins.Any(o => string.Equals(o, origin, StringComparison.OrdinalIgnoreCase));

        if (!isAllowed)
        {
            Console.WriteLine($"[⛔ DITOLAK] Origin '{origin}' tidak dikenal.");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden Origin");
            return;
        }
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<LogHub>("/logHub"); // Endpoint SignalR

await app.RunAsync();