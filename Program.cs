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

// Load Environment
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// Logging Setup
if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
}
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Database Connection
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
    throw new Exception("🔥 FATAL ERROR: JWT Secret Key tidak ditemukan!");
}
var key = Encoding.UTF8.GetBytes(jwtSecretKey);

// SignalR Configuration
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

// Controllers
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
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// Fluent Validation & AutoMapper
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddAutoMapper(typeof(Program));

// Services Injection
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IKartuService, KartuService>();
builder.Services.AddScoped<IAksesLogService, AksesLogService>();
builder.Services.AddScoped<IKelasService, KelasService>();
builder.Services.AddScoped<IRuanganService, RuanganService>();
builder.Services.AddScoped<ITapService, TapService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPeriodeService, PeriodeService>();
builder.Services.AddScoped<IScanService, ScanService>();
builder.Services.AddScoped<IBroadcastService, BroadcastService>();

// Repositories Injection
builder.Services.AddScoped<IPeriodeRepository, PeriodeRepository>();
builder.Services.AddScoped<IKartuRepository, KartuRepository>();
builder.Services.AddScoped<IAksesLogRepository, AksesLogRepository>();
builder.Services.AddScoped<IKelasRepository, KelasRepository>();
builder.Services.AddScoped<IRuanganRepository, RuanganRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Setup Ping Service & HttpClient
builder.Services.AddHttpClient();
builder.Services.AddHostedService<DailyPingService>(); 

// CORS Configuration
var corsOriginsRaw = builder.Configuration["CORS:Origins"]; 
builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", p =>
    {
        if (string.IsNullOrEmpty(corsOriginsRaw) || corsOriginsRaw == "*")
        {
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            var origins = corsOriginsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()).ToArray();
            p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    }));

// Authentication Configuration (MANUAL OVERRIDE - SESUAI REQUEST)
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
            // 1. Manual Check Header
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
            // 2. Manual Check Query String (Support SignalR)
            else
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
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
// DAILY PING SERVICE (SIMPLE PING_URL ONLY)
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
        _logger.LogInformation("⏰ Anti-Sleep Service Menunggu Server Booting...");

        // Delay 15 detik agar server siap dulu (Mencegah error Connection Refused)
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        // HANYA BACA PING_URL (Tanpa Fallback)
        var targetUrl = _configuration["PING_URL"];

        // Safety: Jika PING_URL kosong, batalkan service agar tidak crash
        if (string.IsNullOrEmpty(targetUrl))
        {
            _logger.LogWarning("⚠️ PING_URL tidak ditemukan di Environment! Anti-Sleep Service NON-AKTIF.");
            return;
        }

        targetUrl = targetUrl.TrimEnd('/');
        _logger.LogInformation($"⏰ Anti-Sleep Service Dimulai. Target: {targetUrl}");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(20);

                _logger.LogInformation($"[🚀 PING] Mengirim sinyal ke {targetUrl}...");
                var response = await client.GetAsync($"{targetUrl}/", stoppingToken);

                if (response.IsSuccessStatusCode)
                    _logger.LogInformation($"[✅ PING SUKSES] {response.StatusCode}");
                else
                    _logger.LogWarning($"[⚠️ PING WARNING] {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[❌ PING ERROR] Gagal menghubungi {targetUrl}: {ex.Message}");
            }

            // Ping setiap 2 menit
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}
