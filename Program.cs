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

// Load Environment & Configuration
Env.Load();
builder.Configuration.AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
}
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Database Connection
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

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
var jwtSecretKey = Environment.GetEnvironmentVariable("JwtSettings__SecretKey")?.Trim();
if (string.IsNullOrEmpty(jwtSecretKey))
{
    throw new Exception("🔥 FATAL ERROR: JWT Secret Key tidak ditemukan di .env! Pasang dulu.");
}
var key = Encoding.UTF8.GetBytes(jwtSecretKey);

// SignalR Configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
});

// Controllers Configuration
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Swagger Configuration
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

// Fluent Validation & AutoMapper
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddAutoMapper(typeof(Program));

// Dependency Injection (Services & Repositories)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IKartuService, KartuService>();
builder.Services.AddScoped<IAksesLogService, AksesLogService>();
builder.Services.AddScoped<IKelasService, KelasService>();
builder.Services.AddScoped<IRuanganService, RuanganService>();
builder.Services.AddScoped<ITapService, TapService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IBroadcastService, BroadcastService>();

builder.Services.AddScoped<IKartuRepository, KartuRepository>();
builder.Services.AddScoped<IAksesLogRepository, AksesLogRepository>();
builder.Services.AddScoped<IKelasRepository, KelasRepository>();
builder.Services.AddScoped<IRuanganRepository, RuanganRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Setup Ping Service
builder.Services.AddHttpClient(); // Wajib untuk melakukan request HTTP
builder.Services.AddHostedService<DailyPingService>(); // Mendaftarkan service background
// ---------------------------------

// CORS Configuration
var corsOrigins = Environment.GetEnvironmentVariable("CORS__Origins");
Console.WriteLine($"[🔒 CORS CONFIG] Raw Value: '{corsOrigins}'");

builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", p =>
    {
        if (string.IsNullOrEmpty(corsOrigins) || corsOrigins == "*")
        {
            p.AllowAnyOrigin()
             .AllowAnyHeader()
             .AllowAnyMethod();
        }
        else
        {
            var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(o => o.Trim())
                                     .ToArray();

            p.WithOrigins(origins)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials();
        }
    }));

// Authentication Configuration
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
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var principal = handler.ValidateToken(token, validationParams, out var validatedToken);
                    context.Principal = principal;
                    context.Success();
                    Console.WriteLine($"[🎉 MANUAL OVERRIDE] Token Valid! User: {principal.Identity?.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[❌ MANUAL FAIL] Token ditolak: {ex.Message}");
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

// HTTP Request Pipeline

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lab Access API v1");
    c.RoutePrefix = "swagger";
});

// Custom Middleware Stack
app.UseMiddleware<SignalRLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<HybridSecurityMiddleware>();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<LogHub>("/hubs/log");

await app.RunAsync();

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
        _logger.LogInformation("⏰ DailyPingService started. Will ping '/' every 24 hours.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

            try
            {
                var client = _httpClientFactory.CreateClient();

                var appUrl = "http://localhost:7860";
                var urls = _configuration["ASPNETCORE_URLS"];
                if (!string.IsNullOrEmpty(urls))
                {
                    appUrl = urls.Split(';')[0]; // Ambil URL pertama
                    appUrl = appUrl.Replace("*", "localhost").Replace("+", "localhost"); // Fix wildcard binding
                }

                _logger.LogInformation($"[🚀 DAILY PING] Sending GET request to {appUrl}/ ...");

                var response = await client.GetAsync($"{appUrl}/", stoppingToken);

                _logger.LogInformation($"[✅ DAILY PING] Response: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[❌ DAILY PING FAILED] Error: {ex.Message}");
            }
        }
    }
}