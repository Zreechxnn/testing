using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace testing.Controllers // Pastikan namespace sesuai project kamu
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
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"[HARDWARE] Raw: {message}");

                        try
                        {
                            // Ubah ke Object
                            var sensorData = JsonSerializer.Deserialize<SensorPayload>(message, jsonOptions);

                            if (sensorData != null)
                            {
                                // LOGIC VALIDASI SEMENTARA
                                string reply = "DENIED";

                                // Contoh Logic Dummy (Ganti dengan Service Database nanti)
                                if (sensorData.Uid != null)
                                {
                                    reply = "OPEN_DOOR";
                                    Console.WriteLine($"Akses Diterima: {sensorData.Uid}");
                                }

                                // Balas ke ESP
                                var responseBytes = Encoding.UTF8.GetBytes(reply);
                                await webSocket.SendAsync(
                                    new ArraySegment<byte>(responseBytes),
                                    WebSocketMessageType.Text,
                                    true,
                                    CancellationToken.None);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[HARDWARE] JSON Error: {e.Message}");
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HARDWARE] Err: {ex.Message}");
            }
        }
    }

    public class SensorPayload
    {
        public string Uid { get; set; }
        public string Device { get; set; }
        public string Timestamp { get; set; }
    }
}