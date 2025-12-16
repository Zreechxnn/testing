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

            // 1. Bypass untuk Swagger, Root, Hubs (SignalR), DAN WEBSOCKET HARDWARE (/ws/)
            if (requestPath != null && (
                requestPath.Contains("/swagger") ||
                requestPath == "/" ||
                requestPath.Contains("/hubs/") ||
                requestPath.Contains("/ws/") // <-- INI YANG BARU
               ))
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

            _logger.LogInformation($"[SATPAM] Path: {requestPath} | Origin: '{origin}'");

            // 2. LOGIKA: Jika tidak ada Origin/Referer (IoT/App), IZINKAN lewat.
            if (string.IsNullOrEmpty(origin) && string.IsNullOrEmpty(referer))
            {
                _logger.LogInformation("[NON-BROWSER] Request tanpa identitas. Diizinkan.");
            }
            // 3. Jika ada Origin (Browser), CEK WHITELIST.
            else if (!string.IsNullOrEmpty(origin) && allowedOrigins != null)
            {
                bool isAllowed = allowedOrigins.Any(o => string.Equals(o, origin, StringComparison.OrdinalIgnoreCase));
                if (!isAllowed)
                {
                    _logger.LogWarning($"[DITOLAK] Origin '{origin}' tidak terdaftar!");
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync($"Akses Ditolak.");
                    return;
                }
                _logger.LogInformation("[BROWSER VALID] Origin terdaftar.");
            }

            await _next(context);
        }
    }
}