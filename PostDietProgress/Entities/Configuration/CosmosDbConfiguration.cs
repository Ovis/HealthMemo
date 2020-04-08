namespace PostDietProgress.Entities.Configuration
{
    public class CosmosDbConfiguration
    {
        public string DatabaseId { get; set; } = "AzureFunctionsDbId";

        public string DatabaseThroughput { get; set; } = "200";

        public string ContainerThroughput { get; set; } = "200";

        public string SettingPartitionKey { get; set; } = "/partitionKey";

        public string SettingContainerId { get; set; } = "Setting";

        public string HealthDataContainerId { get; set; } = "HealthData";

        public int HealthDataTimeToLive { get; set; }

        public string HealthDataContainerPartitionKey { get; set; }
    }
}