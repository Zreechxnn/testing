using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace testing.Middleware
{
    public class HybridSecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HybridSecurityMiddleware> _logger;

        public HybridSecurityMiddleware(RequestDelegate next, ILogger<HybridSecurityMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestPath = context.Request.Path.Value?.ToLower();

            // 1. Bypass untuk Swagger, Root, dan Hubs (SignalR)
            if (requestPath != null && (requestPath.Contains("/swagger") || requestPath == "/" || requestPath.Contains("/hubs/")))
            {
                await _next(context);
                return;
            }

            var origin = context.Request.Headers["Origin"].ToString();
            var referer = context.Request.Headers["Referer"].ToString();

            // Ambil whitelist dari Environment
            var corsOriginsEnv = Environment.GetEnvironmentVariable("CORS__Origins");
            var allowedOrigins = corsOriginsEnv?
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.Trim().TrimEnd('/'))
                                .ToList();

            _logger.LogInformation($"[👮 SATPAM CEK] Path: {requestPath} | Origin: '{origin}' | Referer: '{referer}'");

            // 2. LOGIKA: Jika tidak ada Origin/Referer (IoT/Mobile/Postman), IZINKAN lewat.
            if (string.IsNullOrEmpty(origin) && string.IsNullOrEmpty(referer))
            {
                _logger.LogInformation("[ℹ️ NON-BROWSER] Request tanpa identitas (IoT/App). Diizinkan lewat.");
            }
            // 3. Jika ada Origin (Browser), CEK WHITELIST.
            else if (!string.IsNullOrEmpty(origin) && allowedOrigins != null)
            {
                bool isAllowed = allowedOrigins.Any(o => string.Equals(o, origin, StringComparison.OrdinalIgnoreCase));
                if (!isAllowed)
                {
                    _logger.LogWarning($"[⛔ DITOLAK] Origin '{origin}' tidak ada di daftar whitelist!");
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync($"Akses Ditolak: Origin '{origin}' dilarang masuk.");
                    return; // Stop pipeline di sini
                }
                _logger.LogInformation("[✅ BROWSER VALID] Origin terdaftar. Silakan masuk.");
            }

            await _next(context);
        }
    }
}