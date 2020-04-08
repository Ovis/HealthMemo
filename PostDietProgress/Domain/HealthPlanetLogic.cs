using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PostDietProgress.Entities.Configuration;
using PostDietProgress.Entities.HealthPlanetEntity;
using TimeZoneConverter;

namespace PostDietProgress.Domain
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

            var res = await _httpClient.PostAsync("https://www.healthplanet.jp/oauth/token", content);

            await using var stream = (await res.Content.ReadAsStreamAsync());

            using var reader = (new StreamReader(stream, Encoding.GetEncoding("shift-jis"), true)) as TextReader;
            var jsonString = await reader.ReadToEndAsync();

            var tokenData = JsonSerializer.Deserialize<Token>(jsonString);

            var result = await _cosmosDbLogic.SetHealthPlanetToken(tokenData);

            return result;
        }

        /// <summary>
        /// HealthPlanetから身体情報を取得
        /// </summary>
        /// <returns></returns>
        public async Task<(string height, List<HealthData> healthDataList)> GetHealthDataAsync(int period)
        {
            var token = await _cosmosDbLogic.GetSettingDataAsync();

            var jst = TZConvert.GetTimeZoneInfo("Tokyo Standard Time");
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, jst);

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"access_token",token.AccessToken},
                {"date","1"},
                {"from",$"{localTime.AddDays(-period):yyyyMMdd}000000"},
                {"to",$"{localTime:yyyyMMdd}235959"},
                {"tag","6021,6022,6023,6024,6025,6026,6027,6028,6029"}
            });

            var res = await _httpClient.PostAsync("https://www.healthplanet.jp/status/innerscan.json", content);

            await using var stream = (await res.Content.ReadAsStreamAsync());
            using var reader = (new StreamReader(stream, Encoding.UTF8, true)) as TextReader;

            var healthDataJson = await reader.ReadToEndAsync();

            var healthData = JsonSerializer.Deserialize<InnerScan>(healthDataJson);

            return ShapeHealthData(healthData);
        }

        /// <summary>
        /// HealthPlanetから取得したデータの整形
        /// </summary>
        /// <param name="healthData"></param>
        /// <returns></returns>
        private (string height, List<HealthData> healthDataList) ShapeHealthData(InnerScan healthData)
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

            return (healthData.Height, healthPlanetDataList);
        }
    }
}
