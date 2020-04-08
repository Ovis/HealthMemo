using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using PostDietProgress.Entities;

namespace PostDietProgress.Domain
{
    public class InitializeCosmosDbLogic
    {
        private readonly CosmosDbConfiguration _settings;
        private readonly CosmosClient _cosmosDbClient;
        private readonly Database _cosmosDatabase;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cosmosDbClient"></param>
        public InitializeCosmosDbLogic(IOptions<CosmosDbConfiguration> options, CosmosClient cosmosDbClient)
        {
            _settings = options.Value;
            _cosmosDbClient = cosmosDbClient;

            _cosmosDatabase = _cosmosDbClient.GetDatabase(_settings.DatabaseId);
        }

        /// <summary>
        /// CosmosDBのデータベースを作成する(既にある場合は何もしない)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CreateCosmosDbDatabaseIfNotExistsAsync()
        {
            int.TryParse(_settings.DatabaseThroughput, out var databaseThroughput);

            var result = await _cosmosDbClient.CreateDatabaseIfNotExistsAsync(_settings.DatabaseId, databaseThroughput);

            return result.StatusCode == HttpStatusCode.Created;

        }

        /// <summary>
        /// 設定用コンテナ作成
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CreateSettingCosmosDbContainerIfNotExistsAsync()
        {
            var indexPolicy = new IndexingPolicy
            {
                IndexingMode = IndexingMode.Consistent,
                Automatic = true
            };

            int.TryParse(_settings.ContainerThroughput, out var throughput);

            return await CreateCosmosDbContainerIfNotExistsAsync(
                 _settings.SettingContainerId,
                 throughput: throughput,
                 partitionKeyPath: _settings.SettingPartitionKey,
                 indexPolicy: indexPolicy);
        }

        /// <summary>
        /// 身体情報格納コンテナ作成
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CreateBodyConditionCosmosDbContainerIfNotExistsAsync()
        {

            var indexPolicy = new IndexingPolicy
            {
                IndexingMode = IndexingMode.Consistent,
                Automatic = true
            };


            int.TryParse(_settings.ContainerThroughput, out var throughput);

            return await CreateCosmosDbContainerIfNotExistsAsync(
                    _settings.HealthDataContainerId,
                    throughput: throughput,
                    partitionKeyPath: _settings.HealthDataContainerPartitionKey,
                    indexPolicy: indexPolicy,
                    defaultTimeToLive: _settings.HealthDataTimeToLive);
        }

        /// <summary>
        /// CosmosDBのコンテナを作成(既にある場合は何もしない)
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="throughput"></param>
        /// <param name="partitionKeyPath"></param>
        /// <param name="indexPolicy"></param>
        /// <param name="uniqueKeys"></param>
        /// <param name="defaultTimeToLive"></param>
        /// <returns></returns>
        private async Task<bool> CreateCosmosDbContainerIfNotExistsAsync(
            string containerId,
            int throughput = 400,
            string partitionKeyPath = "",
            IndexingPolicy indexPolicy = null,
            Collection<UniqueKey> uniqueKeys = null,
            int defaultTimeToLive = -1
        )
        {
            var properties = new ContainerProperties(containerId, partitionKeyPath)
            {
                //データの有効期限(秒)
                DefaultTimeToLive = defaultTimeToLive,
                //インデックスポリシー
                IndexingPolicy = indexPolicy
            };

            // ユニークキー
            uniqueKeys ??= new Collection<UniqueKey>();
            if (uniqueKeys.Any())
            {
                foreach (var key in uniqueKeys)
                {
                    properties.UniqueKeyPolicy.UniqueKeys.Add(key);
                }
            }

            //コンテナの作成
            var result = await _cosmosDatabase.CreateContainerIfNotExistsAsync(properties, throughput);

            return result.StatusCode == HttpStatusCode.Created;
        }
    }
}
