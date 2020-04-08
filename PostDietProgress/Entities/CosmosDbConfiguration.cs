namespace PostDietProgress.Entities
{
    public class CosmosDbConfiguration
    {
        public string DatabaseId { get; set; } = "AzureFunctionsDbId";

        public string DatabaseThroughput { get; set; } = "200";

        public string ContainerThroughput { get; set; } = "200";

        public string SettingPartitionKey { get; set; } = "/partitionKey";

        public string SettingContainerId { get; set; } = "Setting";

        public string DietDataContainerId { get; set; } = "DietData";

        public int DietDataTimeToLive { get; set; }

        public string DietDataContainerPartitionKey { get; set; }
    }
}