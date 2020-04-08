using System;

namespace PostDietProgress.Entities.DbEntity
{
    public class Setting
    {
        public string Id { get; set; } = "Setting";
        public long PreviousWeight { get; set; }

        public long PreviousWeekWeight { get; set; }

        public DateTime PreviousMeasurementDate { get; set; }

        public string AccessToken { get; set; }

        public DateTime ExpiresIn { get; set; }

        public string RefreshToken { get; set; }

        public bool ErrorFlag { get; set; }

        public DateTime PreviousErrorDateTime { get; set; }

        public int DietDataTimeToLive { get; set; }

        public string PartitionKey { get; set; } = "Setting";
    }
}
