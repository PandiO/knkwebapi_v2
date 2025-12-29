using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    public class UserDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = null!;

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = null!;

        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("coins")]
        public int Coins { get; set; }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("O");
    }

    // Lightweight User DTO for embedding in other payloads (no sensitive info)
    public class UserSummaryDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = null!;
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = null!;
        [JsonPropertyName("coins")]
        public int Coins { get; set; }
    }

    public class UserListDto
    {
        [JsonPropertyName("id")]
        public int? id { get; set; }

        [JsonPropertyName("username")]
        public string username { get; set; } = null!;

        [JsonPropertyName("uuid")]
        public string uuid { get; set; } = null!;

        [JsonPropertyName("email")]
        public string email { get; set; } = null!;

        [JsonPropertyName("coins")]
        public int Coins { get; set; }
    }
}