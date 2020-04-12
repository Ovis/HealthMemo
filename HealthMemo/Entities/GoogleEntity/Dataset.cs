using System.Collections.Generic;
using Google.Apis.Fitness.v1.Data;

namespace HealthMemo.Entities.GoogleEntity
{
    public class Dataset
    {
        public string DataSourceId { get; set; }
        public long MaxEndTimeNs { get; set; }
        public long MinStartTimeNs { get; set; }
        public string NextPageToken { get; set; }
        public List<Point> Point { get; set; }
        public string ETag { get; set; }
    }

    public class Value
    {
        public double? FpVal { get; set; }
        public int? IntVal { get; set; }
        public List<ValueMapValEntry> MapVal { get; set; }
        public string StringVal { get; set; }
        public object ETag { get; set; }
    }

    public class Point
    {
        public long ComputationTimeMillis { get; set; }
        public string DataTypeName { get; set; }
        public long EndTimeNanos { get; set; }
        public long? ModifiedTimeMillis { get; set; }
        public string OriginDataSourceId { get; set; }
        public long? RawTimestampNanos { get; set; }
        public long StartTimeNanos { get; set; }
        public List<Value> Value { get; set; }
        public string ETag { get; set; }
    }
}
