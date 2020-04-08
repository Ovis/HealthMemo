using System.Text.Json.Serialization;

namespace PostDietProgress.Entities.HealthPlanetEntity
{
    public class Token
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
