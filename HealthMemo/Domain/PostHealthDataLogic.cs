using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using HealthMemo.Entities.Configuration;
using HealthMemo.Entities.DbEntity;
using HealthMemo.Entities.PostEntity;
using HealthMemo.Extensions;

namespace HealthMemo.Domain
{
    public class PostHealthDataLogic
    {
        private readonly HttpClient _httpClient;
        private readonly WebHookConfiguration _webHookConfiguration;


        private readonly CosmosDbLogic _cosmosDbLogic;

        public PostHealthDataLogic(HttpClient httpClient,
            IOptions<WebHookConfiguration> webHookConfiguration,
            CosmosDbLogic cosmosDbLogic)
        {
            _httpClient = httpClient;
            _webHookConfiguration = webHookConfiguration.Value;
            _cosmosDbLogic = cosmosDbLogic;
        }

        /// <summary>
        /// 身体データ投稿処理
        /// </summary>
        /// <param name="record"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        public async Task<bool> PostHealthDataAsync(HealthRecord record, Goal goal)
        {
            //BMI
            var bmi = CalcBmi(record);

            double? achievementRate = null;

            //目標達成率
            if (goal != null)
            {
                achievementRate = CalcAchievementRate((double)record.Weight, goal);
            }

            //最新の身体データの取得日時
            var assayDate = record.AssayDate.ConvertJstTimeFromUtc();

            var postString = new StringBuilder();
            postString.AppendLine($"{assayDate:yyyy年MM月dd日(ddd) HH:mm}のダイエット進捗");
            postString.AppendLine($"現在の体重:{record.Weight}kg");
            postString.AppendLine($"BMI:{bmi}");

            if (achievementRate != null)
            {
                postString.AppendLine($"目標達成率:{achievementRate}%");
            }



            var previousHealthData = await _cosmosDbLogic.GetPreviousHealthDataAsync();

            var weekWeightAverage = 0.0;

            if (previousHealthData != null)
            {
                var diffWeight = Math.Round(((double)record.Weight - previousHealthData.PreviousWeight), 2);

                var prevDate = previousHealthData.PreviousMeasurementDate.ConvertJstTimeFromUtc();

                postString.AppendLine($"前日同時間帯測定({prevDate:yyyy年MM月dd日(ddd) HH:mm})から{diffWeight}kgの増減");



                //日曜日なら移動平均計算
                if (((DateTime)assayDate).DayOfWeek == DayOfWeek.Sunday)
                {
                    //前週の体重平均値を取得
                    var prevWeekWeight = previousHealthData.PreviousWeekWeight;

                    //今週の体重平均値を取得
                    weekWeightAverage = await CalcWeightAverageAsync((DateTime)assayDate);

                    if (Math.Abs(prevWeekWeight) > 0.0000000001)
                    {

                        var averageWeight = Math.Round(((double)weekWeightAverage - prevWeekWeight), 2);

                        postString.AppendLine($"前週の平均体重:{prevWeekWeight}kg  今週の平均体重:{weekWeightAverage}kg");
                        postString.AppendLine($"移動平均値:{averageWeight}kg");
                    }
                }
            }

            await PostHealthDataToDiscordAsync(postString.ToString());

            //今回の値をセット
            await _cosmosDbLogic.SetPreviousHealthDataAsync((double)record.Weight, weekWeightAverage, (DateTime)record.AssayDate);


            return false;
        }

        /// <summary>
        /// BMI値算出
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private static double CalcBmi(HealthRecord record)
        {
            var cm = (double)record.Height / 100;

            var bmi = Math.Round(((double)record.Weight / Math.Pow(cm, 2)), 2);

            return bmi;
        }

        /// <summary>
        /// 目標達成率算出
        /// </summary>
        /// <param name="weight"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        private double? CalcAchievementRate(double weight, Goal goal)
        {
            return goal == null
                ? (double?)null
                : Math.Round(((1 - (weight - goal.GoalWeight) / (goal.OriginalWeight - goal.GoalWeight)) * 100), 2);
        }

        /// <summary>
        /// 一週間の体重の平均値を取得
        /// </summary>
        /// <param name="assayDate"></param>
        /// <returns></returns>
        private async Task<double> CalcWeightAverageAsync(DateTime assayDate)
        {
            var sevenDaysAgo = new DateTime(assayDate.AddDays(-7).Year, assayDate.AddDays(-7).Month, assayDate.AddDays(-7).Day, 0, 0, 0);

            var healthList = await _cosmosDbLogic.GetHealthPlanetPostDataPeriodAsync(sevenDaysAgo, assayDate);

            var average = healthList.Select(x => x.Weight).Sum() / healthList.Count();

            var result = average ?? 0.0;

            return result;
        }

        /// <summary>
        /// Discordに投稿
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private async Task PostHealthDataToDiscordAsync(string msg)
        {
            var jsonData = new Discord
            {
                Content = msg
            };

            var json = JsonSerializer.Serialize(jsonData);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_webHookConfiguration.WebHookUrl, content);

            await using var stream = (await response.Content.ReadAsStreamAsync());
            using var reader = (new StreamReader(stream, Encoding.UTF8, true)) as TextReader;

            await reader.ReadToEndAsync();
        }
    }
}
