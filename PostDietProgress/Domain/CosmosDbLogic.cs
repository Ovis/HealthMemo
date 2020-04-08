using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using PostDietProgress.Entities;
using PostDietProgress.Entities.DbEntity;

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
            _healthContainer = _cosmosDatabase.GetContainer(_cosmosDbConfiguration.DietDataContainerId);
        }

        /// <summary>
        /// HealthPlanetトークンDB登録処理
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> SetHealthPlanetToken(HealthPlanetToken token)
        {
            var setting = new Setting()
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
        public async Task<Setting> GetSettingDataAsync()
        {
            return await _settingContainer.ReadItemAsync<Setting>("Setting", new PartitionKey("Setting"));
        }

        /// <summary>
        /// HealthPlanetから取得した身体情報をDBに格納
        /// </summary>
        /// <param name="healthList"></param>
        public async Task SetHealthPlanetHealthDataAsync(List<HealthPlanetHealthData> healthList)
        {
            foreach (var record in healthList.Select(health => new HealthData
            {
                Id = health.DateTime,
                BasalMetabolism = health.BasalMetabolism,
                BodyAge = health.BodyAge,
                BodyFatPerf = health.BodyFatPerf,
                BoneQuantity = health.BoneQuantity,
                MuscleMass = health.MuscleMass,
                MuscleScore = health.MuscleScore,
                VisceralFatLevel = health.VisceralFatLevel,
                VisceralFatLevel2 = health.VisceralFatLevel2,
                Weight = health.Weight,
                Type = "HealthData"
            }))
            {
                await _healthContainer.UpsertItemAsync(record);
            }
        }
    }
}
