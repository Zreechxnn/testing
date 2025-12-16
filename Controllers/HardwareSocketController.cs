using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace NamaProjectKamu.Controllers // Sesuaikan namespace
{
    [ApiController]
    public class HardwareSocketController : ControllerBase
    {
        [Route("/ws/hardware")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await HandleHardwareConnection(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task HandleHardwareConnection(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"[Hardware] Raw Data: {message}");

                        try
                        {
                            // 1. Deserialize JSON dari ESP
                            var sensorData = JsonSerializer.Deserialize<SensorPayload>(message, jsonOptions);

                            if (sensorData != null)
                            {
                                // LOGIC UTAMA DISINI
                                // Contoh: Validasi UID ke Database
                                string replyText = "DENIED";

                                if (sensorData.Uid == "E2 45 88 12") // Contoh Hardcode
                                {
                                    replyText = "OPEN_DOOR";
                                    Console.WriteLine($"Akses Diterima untuk: {sensorData.Uid}");
                                }

                                // 2. Kirim Balasan ke ESP
                                var responseBytes = Encoding.UTF8.GetBytes(replyText);
                                await webSocket.SendAsync(
                                    new ArraySegment<byte>(responseBytes),
                                    WebSocketMessageType.Text,
                                    true,
                                    CancellationToken.None);
                            }
                        }
                        catch (JsonException)
                        {
                            Console.WriteLine("[Hardware] Data bukan JSON valid.");
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(
                            result.CloseStatus.Value,
                            result.CloseStatusDescription,
                            CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Hardware] Error: {ex.Message}");
            }
        }
    }

    // Model Data (Letakkan di file terpisah jika mau)
    public class SensorPayload
    {
        public string Uid { get; set; }
        public string Device { get; set; }
    }
}