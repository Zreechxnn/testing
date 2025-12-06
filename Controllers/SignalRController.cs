// Buat file SignalRController.cs di folder Controllers
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using testing.Hubs;

namespace testing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignalRController : ControllerBase
    {
        private readonly IHubContext<LogHub> _hubContext;
        private readonly ILogger<SignalRController> _logger;

        public SignalRController(IHubContext<LogHub> hubContext, ILogger<SignalRController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPost("broadcast")]
        public async Task<IActionResult> BroadcastMessage([FromBody] BroadcastRequest request)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveBroadcast", new
                {
                    Message = request.Message,
                    Type = request.Type,
                    Timestamp = DateTime.UtcNow,
                    Sender = "System"
                });

                _logger.LogInformation($"Broadcast sent: {request.Type} - {request.Message}");
                return Ok(new { Success = true, Message = "Broadcast sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting message");
                return StatusCode(500, new { Success = false, Message = "Error broadcasting" });
            }
        }

        [HttpPost("test-tap")]
        public async Task<IActionResult> TestTapEvent()
        {
            try
            {
                var testEvent = new
                {
                    Uid = "TEST_123456",
                    Ruangan = "Lab Komputer 1",
                    Status = "CHECKIN",
                    Identitas = "Kelas XII IPA 1",
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await _hubContext.Clients.All.SendAsync("ReceiveTapEvent", testEvent);
                await _hubContext.Clients.Group("dashboard").SendAsync("UpdateDashboard", testEvent);

                return Ok(new { Success = true, Event = testEvent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }
        }

        [HttpGet("connections")]
        public IActionResult GetConnectionInfo()
        {
            // Catatan: Untuk mendapatkan koneksi aktif, perlu implementasi lebih kompleks
            return Ok(new
            {
                HubEndpoint = "/hubs/log",
                SupportedEvents = new[]
                {
                    "ReceiveMessage",
                    "ReceiveTapEvent",
                    "ReceiveCheckIn",
                    "ReceiveCheckOut",
                    "UpdateDashboard",
                    "LogDeleted"
                }
            });
        }
    }

    public class BroadcastRequest
    {
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info"; // info, warning, error, success
    }
}