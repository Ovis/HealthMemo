using System.Collections.Generic;

namespace HealthMemo.Entities.GoogleEntity
{
    public class DataTypeField
    {
        public string Name { get; set; } = "weight";
        public string Format { get; set; } = "floatPoint";
    }

    public class DataType
    {
        public string Name { get; set; } = "com.google.weight";
        public List<DataTypeField> Field { get; set; } = new List<DataTypeField>() { new DataTypeField() };
    }

    public class Application
    {
        public string PackageName { get; set; }
        public string Version { get; set; } = "1";
        public string Name { get; set; } = "HealthMemo";
        public string DetailsUrl { get; set; }
    }

    public class Device
    {
        public string Uid { get; set; } = "1000001";
        public string Type { get; set; } = "scale";
        public string Version { get; set; } = "1.0";
        public string Model { get; set; } = "RD-906";
        public string Manufacturer { get; set; } = "Tanita";
    }

    public class DataSource
    {
        public string DataStreamId { get; set; }
        public string DataStreamName { get; set; } = "WeightMemo";
        public string Type { get; set; } = "derived";
        public DataType DataType { get; set; } = new DataType();
        public Application Application { get; set; } = new Application();
        public List<object> DataQualityStandard { get; set; }
        public Device Device { get; set; } = new Device();
        public string Name { get; set; }
    }


    public class DataSourceList
    {
        public List<DataSource> DataSource { get; set; }
    }
}
