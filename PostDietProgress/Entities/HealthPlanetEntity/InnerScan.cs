using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PostDietProgress.Entities.HealthPlanetEntity
{
    public class InnerScan
    {
        public class Health
        {
            [JsonPropertyName("date")]
            public string Date { get; set; }

            [JsonPropertyName("keydata")]
            public string Keydata { get; set; }

            [JsonPropertyName("model")]
            public string Model { get; set; }

            [JsonPropertyName("tag")]
            public string Tag { get; set; }
        }

        [JsonPropertyName("birth_date")]
        public string BirthDate { get; set; }

        [JsonPropertyName("data")]
        public List<Health> Data { get; set; }

        [JsonPropertyName("height")]
        public string Height { get; set; }

        [JsonPropertyName("sex")]
        public string Sex { get; set; }
    }
}
