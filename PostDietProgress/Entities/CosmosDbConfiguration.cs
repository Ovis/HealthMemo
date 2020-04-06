namespace PostDietProgress.Entities
{
    public class CosmosDbConfiguration
    {
        public string AccountEndpoint { get; set; }

        public string AccountKey { get; set; }

        public string DatabaseId { get; set; } = "AzureFunctionsDbId";

        public string DatabaseThroughput { get; set; } = "200";

        public string ContainerThroughput { get; set; } = "200";

        public string PartitionKey { get; set; } = "/PartitionKey";

        public string SettingContainerId { get; set; } = "Setting";

        public string DietDataContainerId { get; set; } = "DietData";

        public int DietDataTimeToLive { get; set; }
    }
}