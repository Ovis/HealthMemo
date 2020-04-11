using System;

namespace HealthMemo.Entities.DbEntity
{
    public class HealthPlanetToken
    {
        public string Id { get; set; } = "Token";
        public string AccessToken { get; set; }

        public DateTime ExpiresIn { get; set; }

        public string RefreshToken { get; set; }

        public string PartitionKey { get; set; } = "Setting";
    }
}
