using System;

namespace HealthMemo.Entities.DbEntity
{
    public class Previous
    {
        public string Id { get; set; } = nameof(Previous);

        public string PartitionKey { get; set; } = "Setting";


        public double PreviousWeight { get; set; }

        public double PreviousWeekWeight { get; set; }

        public DateTime PreviousMeasurementDate { get; set; }
    }
}
