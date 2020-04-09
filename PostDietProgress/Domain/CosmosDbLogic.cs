using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using PostDietProgress.Entities.Configuration;
using PostDietProgress.Entities.DbEntity;
using PostDietProgress.Entities.HealthPlanetEntity;
using PostDietProgress.Extensions;

namespace PostDietProgress.Domain
{
    public class CosmosDbLogic
    {
        private readonly CosmosDbConfiguration _cosmosDbConfiguration;
        private readonly CosmosClient _cosmosDbClient;
        private readonly Database _cosmosDatabase;
        private readonly Container _settingContainer;
        private readonly Container _healthContainer;

        public CosmosDbLogic(
            IOptions<CosmosDbConfiguration> cosmosDbConfiguration,
            CosmosClient cosmosDbClient
            )
        {
            _cosmosDbConfiguration = cosmosDbConfiguration.Value;
            _cosmosDbClient = cosmosDbClient;

            _cosmosDatabase = _cosmosDbClient.GetDatabase(_cosmosDbConfiguration.DatabaseId);

            _settingContainer = _cosmosDatabase.GetContainer(_cosmosDbConfiguration.SettingContainerId);
            _healthContainer = _cosmosDatabase.GetContainer(_cosmosDbConfiguration.HealthDataContainerId);
        }

        /// <summary>
        /// HealthPlanetトークンDB登録処理
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> SetHealthPlanetToken(Token token)
        {
            var setting = new HealthPlanetToken()
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresIn = DateTime.Now.AddDays(30)
            };

            try
            {
                await _settingContainer.UpsertItemAsync(setting);
            }
            catch (Exception e)
            {
                //TODO 
                Console.Write(e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 設定情報を取得
        /// </summary>
        /// <returns></returns>
        public async Task<HealthPlanetToken> GetSettingDataAsync()
        {
            return await _settingContainer.ReadItemAsync<HealthPlanetToken>("Token", new PartitionKey("Setting"));
        }

        /// <summary>
        /// HealthPlanetから取得した身体情報をDBに格納
        /// </summary>
        /// <param name="healthData"></param>
        public async Task SetHealthPlanetHealthDataAsync((string height, List<HealthData> healthDataList) healthData)
        {
            var jstCulture = new CultureInfo("ja-JP");

            foreach (var health in healthData.healthDataList)
            {
                //測定日時(UTC)
                var assayDate = DateTime.ParseExact(health.DateTime,
                    "yyyyMMddHHmm",
                    jstCulture,
                    DateTimeStyles.AssumeUniversal);

                var record = new HealthRecord
                {
                    Id = health.DateTime,
                    AssayDate = assayDate,
                    BasalMetabolism = health.BasalMetabolism.ToDoubleOrNull(),
                    BodyAge = health.BodyAge,
                    BodyFatPerf = health.BodyFatPerf.ToDoubleOrNull(),
                    BoneQuantity = health.BoneQuantity.ToDoubleOrNull(),
                    MuscleMass = health.MuscleMass.ToDoubleOrNull(),
                    MuscleScore = health.MuscleScore,
                    VisceralFatLevel = health.VisceralFatLevel.ToLongOrNull(),
                    VisceralFatLevel2 = health.VisceralFatLevel2.ToDoubleOrNull(),
                    Height = healthData.height.ToDoubleOrNull(),
                    Weight = health.Weight.ToDoubleOrNull(),
                    Type = "HealthData"
                };

                try
                {
                    await _healthContainer.UpsertItemAsync(record);
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    Console.WriteLine("Item in database with id: {0} already exists\n", record.Id);
                }
            }
        }

        /// <summary>
        /// 最新の身体データを取得
        /// </summary>
        /// <returns></returns>
        public async Task<HealthRecord> GetHealthPlanetPostDataAsync()
        {
            var healthData = new HealthRecord();

            var queryRequestOptions = new QueryRequestOptions { PartitionKey = new PartitionKey("Setting") };

            var iterator = _settingContainer.GetItemQueryIterator<HealthRecord>("SELECT * FROM c WHERE c.type = 'HealthData' ORDER BY c.id desc OFFSET 0 LIMIT 1", requestOptions: queryRequestOptions);

            while (iterator.HasMoreResults)
            {
                var result = await iterator.ReadNextAsync();

                healthData = result.First();
            };

            return healthData;
        }
    }
}
