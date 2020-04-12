using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HealthMemo.Entities.Configuration;
using HealthMemo.Entities.DbEntity;
using HealthMemo.Entities.HealthPlanetEntity;
using HealthMemo.Extensions;
using Microsoft.Extensions.Options;
using TimeZoneConverter;

namespace HealthMemo.Domain
{
    public class HealthPlanetLogic
    {
        private readonly HttpClient _httpClient;
        private readonly HealthPlanetConfiguration _healthPlanetConfiguration;

        private readonly CosmosDbLogic _cosmosDbLogic;

        public HealthPlanetLogic(HttpClient httpClient,
            IOptions<HealthPlanetConfiguration> healthPlanetConfiguration,
            CosmosDbLogic cosmosDbLogic)
        {
            _httpClient = httpClient;
            _healthPlanetConfiguration = healthPlanetConfiguration.Value;
            _cosmosDbLogic = cosmosDbLogic;
        }

        /// <summary>
        /// トークン取得処理
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<bool> GetHealthPlanetTokenAsync(string code)
        {
            //TANITA HealthPlanet処理のためリダイレクト
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"client_id",_healthPlanetConfiguration.TanitaClientId},
                {"client_secret",_healthPlanetConfiguration.TanitaClientSecretToken},
                {"redirect_uri","https://www.healthplanet.jp/success.html"},
                {"code",code},
                {"grant_type","authorization_code"}
            });

            var res = await _httpClient.PostAsync("https://www.healthplanet.jp/oauth/refreshToken", content);

            await using var stream = (await res.Content.ReadAsStreamAsync());

            using var reader = (new StreamReader(stream, Encoding.GetEncoding("shift-jis"), true)) as TextReader;
            var jsonString = await reader.ReadToEndAsync();

            var tokenData = JsonSerializer.Deserialize<Token>(jsonString);

            var result = await _cosmosDbLogic.SetHealthPlanetToken(tokenData);

            return result;
        }

        /// <summary>
        /// リフレッシュトークン処理
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public async Task<(bool isSuccess, string accessToken)> GetHealthPlanetRefreshTokenAsync(string refreshToken)
        {
            //TANITA HealthPlanet処理のためリダイレクト
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"client_id",_healthPlanetConfiguration.TanitaClientId},
                {"client_secret",_healthPlanetConfiguration.TanitaClientSecretToken},
                {"redirect_uri","https://www.healthplanet.jp/success.html"},
                {"refresh_token",refreshToken},
                {"grant_type","refresh_token"}
            });

            try
            {
                var res = await _httpClient.PostAsync("https://www.healthplanet.jp/oauth/refreshToken", content);

                await using var stream = (await res.Content.ReadAsStreamAsync());

                using var reader = (new StreamReader(stream, Encoding.GetEncoding("shift-jis"), true)) as TextReader;
                var jsonString = await reader.ReadToEndAsync();

                var tokenData = JsonSerializer.Deserialize<Token>(jsonString);

                var result = await _cosmosDbLogic.SetHealthPlanetToken(tokenData);
                return (true, tokenData.AccessToken);
            }
            catch
            {
                return (false, null);
            }
        }

        /// <summary>
        /// HealthPlanetから身体情報を取得
        /// </summary>
        /// <returns></returns>
        public async Task<(bool isSuccess, string height, List<HealthData> healthDataList)> GetHealthDataAsync(int period)
        {
            var token = await _cosmosDbLogic.GetSettingDataAsync();

            string accessToken;

            if (token == null)
            {
                return (false, null, null);
            }

            var jstTimeZone = TZConvert.GetTimeZoneInfo("Tokyo Standard Time");
            var jstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, jstTimeZone);

            if (token.ExpiresIn < jstTime)
            {
                var refresh = await GetHealthPlanetRefreshTokenAsync(token.RefreshToken);

                if (refresh.isSuccess)
                {
                    accessToken = refresh.accessToken;
                }
                else
                {
                    return (false, null, null);
                }
            }
            else
            {
                accessToken = token.AccessToken;
            }

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"access_token",accessToken},
                {"date","1"},
                {"from",$"{jstTime.AddDays(-period):yyyyMMdd}000000"},
                {"to",$"{jstTime:yyyyMMdd}235959"},
                {"tag","6021,6022,6023,6024,6025,6026,6027,6028,6029"}
            });

            var res = await _httpClient.PostAsync("https://www.healthplanet.jp/status/innerscan.json", content);

            await using var stream = (await res.Content.ReadAsStreamAsync());
            using var reader = (new StreamReader(stream, Encoding.UTF8, true)) as TextReader;

            try
            {
                var healthDataJson = await reader.ReadToEndAsync();

                var healthData = JsonSerializer.Deserialize<InnerScan>(healthDataJson);

                return ShapeHealthData(healthData);
            }
            catch
            {
                return (false, "", null);
            }
        }

        /// <summary>
        /// HealthPlanetから取得したデータの整形
        /// </summary>
        /// <param name="healthData"></param>
        /// <returns></returns>
        private (bool isSuccess, string height, List<HealthData> healthDataList) ShapeHealthData(InnerScan healthData)
        {
            var healthPlanetDataList = new List<HealthData>();

            //取得データから日付を抜き出す
            var healthDateList = healthData.Data.Select(x => x.Date).Distinct().ToList();

            //取得データをリスト変換
            foreach (var date in healthDateList)
            {
                var healthList = healthData.Data
                    .Where(r => date.Equals(r.Date))
                    .ToDictionary(x => x.Tag, x => x.Keydata);

                healthPlanetDataList.Add(new HealthData(date, healthList));
            }

            return (true, healthData.Height, healthPlanetDataList);
        }

        /// <summary>
        /// DB格納用リストを生成
        /// </summary>
        /// <param name="healthDataList"></param>
        /// <returns></returns>
        public List<HealthRecord> ShapeHealthRecord(string height, List<HealthData> healthDataList)
        {
            var healthRecordList = new List<HealthRecord>();

            foreach (var health in healthDataList)
            {
                //測定日時(UTC)
                var assayDate = health.DateTime.TryJstDateTimeStringParseToUtc();

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
                    Height = height.ToDoubleOrNull(),
                    Weight = health.Weight.ToDoubleOrNull(),
                    Type = "HealthData"
                };

                healthRecordList.Add(record);
            }

            return healthRecordList;
        }
    }
}
