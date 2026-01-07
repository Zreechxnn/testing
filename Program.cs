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
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ========== CONFIGURATION ==========
var configuration = builder.Configuration;

// ========== DATABASE ==========
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<LabDbContext>(options =>
{
    options.UseNpgsql(connectionString);

    // Only enable sensitive logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ========== JWT AUTHENTICATION ==========
var jwtSettings = configuration.GetSection("JwtSettings");
var jwtSecretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT Secret Key not configured.");

var key = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "LabAccessAPI",
        ValidAudience = jwtSettings["Audience"] ?? "LabAccessClient",
        IssuerSigningKey = new SymmetricSecurityKey(key),
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };

    // SignalR support
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// ========== CORS ==========
var corsOrigins = configuration["CORS:Origins"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(o => o.Trim())
    .ToArray();

if (builder.Environment.IsDevelopment() && (corsOrigins == null || corsOrigins.Length == 0))
{
    corsOrigins = new[] { "http://localhost:3000", "http://localhost:5173" };
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (corsOrigins != null && corsOrigins.Length > 0)
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // Production: strict CORS
            policy.SetIsOriginAllowed(origin => false);
        }
    });
});

// ========== SERVICES ==========
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

// ========== SWAGGER ==========
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Lab Access API",
        Version = "v1",
        Description = "API untuk sistem akses laboratorium"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ========== DEPENDENCY INJECTION ==========
// AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// SignalR
builder.Services.AddSignalR(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors = true;
    }
    options.MaximumReceiveMessageSize = 1024 * 1024;
});

// Application Services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IKelasService, KelasService>();
builder.Services.AddScoped<IPeriodeService, PeriodeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IKartuService, KartuService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRuanganService, RuanganService>();
builder.Services.AddScoped<IAksesLogService, AksesLogService>();
builder.Services.AddScoped<IScanService, ScanService>();
builder.Services.AddScoped<IBroadcastService, BroadcastService>();

// Repositories
builder.Services.AddScoped<IKelasRepository, KelasRepository>();
builder.Services.AddScoped<IPeriodeRepository, PeriodeRepository>();
builder.Services.AddScoped<IKartuRepository, KartuRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRuanganRepository, RuanganRepository>();
builder.Services.AddScoped<IAksesLogRepository, AksesLogRepository>();

// Background Services
if (!string.IsNullOrEmpty(configuration["HealthCheck:TargetUrl"]))
{
    builder.Services.AddHostedService<HealthCheckBackgroundService>();
}

// ========== BUILD APP ==========
var app = builder.Build();

// ========== MIDDLEWARE PIPELINE ==========
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lab Access API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<LogHub>("/hubs/log");

app.MapGet("/", () => Results.Json(new
{
    Status = "OK",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow
}));

app.MapGet("/health", () => Results.Ok("Healthy"));

await app.RunAsync();

// ==========================================
// DAILY PING SERVICE (VERSI FIX)
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
        var targetUrl = _configuration["PING_URL"]?.TrimEnd('/');

        _logger.LogInformation($"⏰ Anti-Sleep Service Dimulai. Target: {targetUrl}");

        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(20);

                // GANTI ke LogDebug: Tidak akan muncul di console produksi
                _logger.LogDebug($"[🚀 PING] Mengirim sinyal ke {targetUrl}...");

                var response = await client.GetAsync($"{targetUrl}/", stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    // GANTI ke LogDebug: "No news is good news". 
                    // Log hanya dicatat jika kamu set level logging ke Debug.
                    _logger.LogDebug($"[✅ PING SUKSES] {response.StatusCode}");
                }
                else
                {
                    // TETAP Warning: Penting untuk tahu jika server menolak ping
                    _logger.LogWarning($"[⚠️ PING WARNING] {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // TETAP Error: Penting untuk tahu jika server mati total
                _logger.LogError($"[❌ PING ERROR] Gagal menghubungi {targetUrl}: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
        }
    }
}