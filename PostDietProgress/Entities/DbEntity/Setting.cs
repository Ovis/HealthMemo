using System;

namespace PostDietProgress.Entities.DbEntity
{
    public class Setting
    {
        public string Id { get; set; } = "Setting";
        public long PreviousWeight { get; set; }

        public long PreviousWeekWeight { get; set; }

        public DateTime PreviousMeasurementDate { get; set; }

        public bool ErrorFlag { get; set; }

        public DateTime PreviousErrorDateTime { get; set; }

        public int HealthDataTimeToLive { get; set; }

        public string PartitionKey { get; set; } = "Setting";
    }
}
