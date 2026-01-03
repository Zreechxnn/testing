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

var builder = WebApplication.CreateBuilder(args);

// 1. Load Environment (Wajib di paling atas untuk membaca .env)
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// 2. Setup Logging Dasar
if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// Hapus mapping default claim type agar tidak konflik dengan claim kustom
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// ==========================================
// CONFIGURATION & DATABASE
// ==========================================

// FIXED: Tidak perlu cek manual Environment var. .NET otomatis menggabungkan Env & Json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<LabDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});
Console.WriteLine($"✅ Database Provider: PostgreSQL | Env: {builder.Environment.EnvironmentName}");

// JWT Secret Setup
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"]?.Trim();
if (string.IsNullOrEmpty(jwtSecretKey))
{
    throw new Exception("🔥 FATAL ERROR: JWT Secret Key tidak ditemukan! Cek .env atau appsettings.");
}
var key = Encoding.UTF8.GetBytes(jwtSecretKey);

// ==========================================
// SERVICES REGISTRATION
// ==========================================

// FIXED: SignalR Security - Detailed Errors hanya untuk Dev
builder.Services.AddSignalR(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors = true;
    }
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
});

// Controllers & Json Options
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
        In = ParameterLocation.Header,
        Description = "Masukkan token JWT di sini (tanpa kata 'Bearer')"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Fluent Validation & AutoMapper
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddAutoMapper(typeof(Program));

// Dependency Injection
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IKartuService, KartuService>();
builder.Services.AddScoped<IAksesLogService, AksesLogService>();
builder.Services.AddScoped<IKelasService, KelasService>();
builder.Services.AddScoped<IRuanganService, RuanganService>();
builder.Services.AddScoped<ITapService, TapService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IScanService, ScanService>();
builder.Services.AddScoped<IBroadcastService, BroadcastService>();
builder.Services.AddScoped<IPeriodeService, PeriodeService>();

builder.Services.AddScoped<IPeriodeRepository, PeriodeRepository>();
builder.Services.AddScoped<IKartuRepository, KartuRepository>();
builder.Services.AddScoped<IAksesLogRepository, AksesLogRepository>();
builder.Services.AddScoped<IKelasRepository, KelasRepository>();
builder.Services.AddScoped<IRuanganRepository, RuanganRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Background Services
builder.Services.AddHttpClient();
builder.Services.AddHostedService<DailyPingService>();

// ==========================================
// SECURITY (CORS & AUTH)
// ==========================================

// CORS Configuration
var corsOriginsRaw = builder.Configuration["CORS:Origins"]; // Membaca dari Env: CORS__Origins
builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", p =>
    {
        if (string.IsNullOrEmpty(corsOriginsRaw) || corsOriginsRaw == "*")
        {
            Console.WriteLine("[🔒 CORS] Mode: Allow Any Origin");
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            var origins = corsOriginsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(o => o.Trim()).ToArray();
            Console.WriteLine($"[🔒 CORS] Allowed: {string.Join(", ", origins)}");
            p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    }));

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "LabAccessAPI",
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "LabAccessClient",
        IssuerSigningKey = new SymmetricSecurityKey(key),
        NameClaimType = "name", // Mapping claim standard
        RoleClaimType = "role",
        ClockSkew = TimeSpan.Zero // Token langsung expired saat waktunya habis
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Support Auth lewat Query String (penting untuk SignalR WebSocket)
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                // Tambahkan header khusus jika token expired
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ==========================================
// APP PIPELINE
// ==========================================
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lab Access API v1");
    c.RoutePrefix = "swagger";
});

// Middleware
app.UseMiddleware<SignalRLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<HybridSecurityMiddleware>();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<LogHub>("/hubs/log");

app.MapGet("/", () => Results.Ok($"API Running 🚀 | Env: {app.Environment.EnvironmentName}"));

await app.RunAsync();

// ==========================================
// BACKGROUND SERVICES
// ==========================================
public class DailyPingService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DailyPingService> _logger;
    private readonly IConfiguration _configuration;

    public DailyPingService(IHttpClientFactory httpClientFactory, ILogger<DailyPingService> logger, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("⏰ Anti-Sleep Service Started.");

        var targetUrl = _configuration["AppSettings:ApiUrl"] ?? "http://localhost:7860";

        // Bersihkan URL dari trailing slash
        targetUrl = targetUrl.TrimEnd('/');

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                _logger.LogInformation($"[🚀 PING] Sending Keep-Alive to {targetUrl}...");

                var response = await client.GetAsync($"{targetUrl}/", stoppingToken);

                if (response.IsSuccessStatusCode)
                    _logger.LogInformation($"[✅ PING SUCCESS] Status: {response.StatusCode}");
                else
                    _logger.LogWarning($"[⚠️ PING WARNING] Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[❌ PING FAILED] {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}