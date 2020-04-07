using System;

namespace PostDietProgress.Entities
{
    public class Setting
    {
        public string Id { get; set; }
        public long PreviousWeight { get; set; }

        public long PreviousWeekWeight { get; set; }

        public DateTime PreviousMeasurementDate { get; set; }

        public string RequestToken { get; set; }

        public DateTime ExpiresIn { get; set; }

        public string RefreshToken { get; set; }

        public bool ErrorFlag { get; set; }

        public DateTime PreviousErrorDateTime { get; set; }

        public int DietDataTimeToLive { get; set; }
    }
}
