using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Net.WebSockets;
using System.Text;

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

        // Loop ini menjaga koneksi tetap hidup sampai alat putus koneksi
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                // Proses data dari ESP di sini
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Data dari Alat: {message}");

                // Di sini kamu bisa panggil _hubContext untuk update Frontend
                // await _hubContext.Clients.All.SendAsync("UpdateData", message);

                // Kirim balasan ke ESP jika perlu (ACK)
                var response = Encoding.UTF8.GetBytes("OK");
                await webSocket.SendAsync(
                    new ArraySegment<byte>(response, 0, response.Length),
                    result.MessageType,
                    result.EndOfMessage,
                    CancellationToken.None);
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
}