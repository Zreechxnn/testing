using System.Text.Json.Serialization;

namespace testing.Models
{
    public class TapRequest
    {
        [JsonPropertyName("uid")]
        public required string Uid { get; set; }

        [JsonPropertyName("id_ruangan")]
        public int IdRuangan { get; set; }

        [JsonPropertyName("timestamp")]
        public required string Timestamp { get; set; }
    }
}