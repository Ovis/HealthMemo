using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HealthMemo.Entities.Configuration;
using HealthMemo.Entities.DbEntity;
using HealthMemo.Entities.HealthPlanetEntity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;

namespace HealthMemo.Domain
{
    public class CosmosDbLogic
    {
        private readonly Container _settingContainer;
        private readonly Container _healthContainer;

        public CosmosDbLogic(
            IOptions<CosmosDbConfiguration> cosmosDbConfiguration,
            CosmosClient cosmosDbClient
            )
        {
            var cosmosDbConfig = cosmosDbConfiguration.Value;

            var cosmosDatabase = cosmosDbClient.GetDatabase(cosmosDbConfig.DatabaseId);

            _settingContainer = cosmosDatabase.GetContainer(cosmosDbConfig.SettingContainerId);
            _healthContainer = cosmosDatabase.GetContainer(cosmosDbConfig.HealthDataContainerId);
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
            try
            {
                return await _settingContainer.ReadItemAsync<HealthPlanetToken>("HealthPlanet", new PartitionKey("Token"));
            }
            catch
            {
                Console.WriteLine("トークン取得に失敗");
                return null;
            }
        }

        /// <summary>
        /// HealthPlanetから取得した身体情報をDBに格納
        /// </summary>
        /// <param name="healthRecordList"></param>
        /// <returns></returns>
        public async Task SetHealthPlanetHealthDataAsync(List<HealthRecord> healthRecordList)
        {
            foreach (var record in healthRecordList)
            {
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
            HealthRecord healthData = null;

            var queryRequestOptions = new QueryRequestOptions { PartitionKey = new PartitionKey("HealthData") };

            var iterator = _healthContainer.GetItemQueryIterator<HealthRecord>("SELECT * FROM c WHERE c.type = 'HealthData' ORDER BY c.id desc OFFSET 0 LIMIT 1", requestOptions: queryRequestOptions);

            while (iterator.HasMoreResults)
            {
                var result = await iterator.ReadNextAsync();

                if (result.Count != 0)
                {
                    healthData = result.First();
                }
            };

            return healthData;
        }

        /// <summary>
        /// 指定された期間の身体データを取得
        /// </summary>
        /// <returns></returns>
        public async Task<List<HealthRecord>> GetHealthPlanetPostDataPeriodAsync(DateTime start, DateTime end)
        {
            var healthDataList = new List<HealthRecord>();

            var queryRequestOptions = new QueryRequestOptions { PartitionKey = new PartitionKey("HealthData") };

            try
            {
                var iterator = _healthContainer.GetItemLinqQueryable<HealthRecord>(requestOptions: queryRequestOptions)
                    .Where(o => o.AssayDate > start)
                    .Where(w => w.AssayDate <= end)
                    .Where(n => n.Weight != null)
                    .ToFeedIterator();

                while (iterator.HasMoreResults)
                {
                    var result = await iterator.ReadNextAsync();

                    healthDataList.AddRange(result);
                }
            }
            catch
            {
                return null;
            }

            return healthDataList;
        }

        /// <summary>
        /// 元体重、目標体重を取得
        /// </summary>
        /// <returns></returns>
        public async Task<Goal> GetGoalAsync()
        {
            try
            {
                return await _settingContainer.ReadItemAsync<Goal>("Goal", new PartitionKey("Setting"));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 前回身体データに今回の身体データをセット
        /// </summary>
        /// <returns></returns>
        public async Task SetPreviousHealthDataAsync(double previousWeight, double previousWeekWeight, DateTime previousMeasurementDate)
        {
            var record = new Previous
            {
                Id = "Previous",
                PreviousWeight = previousWeight,
                PreviousMeasurementDate = previousMeasurementDate,
                PreviousWeekWeight = previousWeekWeight,
                PartitionKey = "Setting"
            };

            try
            {
                await _settingContainer.UpsertItemAsync(record);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                Console.WriteLine("Item in database with id: {0} already exists\n", record.Id);
            }
        }

        /// <summary>
        /// 前回の身体データを取得
        /// </summary>
        /// <returns></returns>
        public async Task<Previous> GetPreviousHealthDataAsync()
        {
            try
            {
                return await _settingContainer.ReadItemAsync<Previous>("Previous", new PartitionKey("Setting"));
            }
            catch
            {
                return null;
            }
        }






        /// <summary>
        /// GoogleトークンDB登録処理
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> SetGoogleToken(Entities.GoogleEntity.Token token)
        {
            var setting = new GoogleToken()
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresIn = DateTime.Now.AddHours(1)
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
        /// Googleトークンを取得
        /// </summary>
        /// <returns></returns>
        public async Task<GoogleToken> GetGoogleTokenAsync()
        {
            try
            {
                return await _settingContainer.ReadItemAsync<GoogleToken>("Google", new PartitionKey("Token"));
            }
            catch
            {
                Console.WriteLine("トークン取得に失敗");
                return null;
            }
        }
    }
}
