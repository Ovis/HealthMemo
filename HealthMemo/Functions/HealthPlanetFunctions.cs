using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using HealthMemo.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace HealthMemo.Functions
{

    public class HealthPlanetFunctions
    {
        private readonly CosmosDbLogic _cosmosDbLogic;
        private readonly GoogleFitLogic _googleFitLogic;
        private readonly HealthPlanetLogic _healthPlanetLogic;
        private readonly PostHealthDataLogic _postHealthDataLogic;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="cosmosDbLogic"></param>
        /// <param name="googleFitLogic"></param>
        /// <param name="healthPlanetLogic"></param>
        /// <param name="postHealthDataLogic"></param>
        public HealthPlanetFunctions(
            CosmosDbLogic cosmosDbLogic,
            GoogleFitLogic googleFitLogic,
            HealthPlanetLogic healthPlanetLogic,
            PostHealthDataLogic postHealthDataLogic
        )
        {
            _cosmosDbLogic = cosmosDbLogic;
            _googleFitLogic = googleFitLogic;
            _healthPlanetLogic = healthPlanetLogic;
            _postHealthDataLogic = postHealthDataLogic;
        }

        [FunctionName("GetHealthPlanet")]
        public async Task<IActionResult> GetHealthPlanet(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var period = 7;
            if (!(string.IsNullOrEmpty(req.Query["period"])) && !int.TryParse(req.Query["period"], out period))
            {
                return new BadRequestErrorMessageResult("期間指定の値が数値以外の値になっています。");
            }

            if (0 > period || period > 120)
            {
                return new BadRequestErrorMessageResult("期間指定の値は0から120までの値を指定してください。");
            }

            //HealthPlanetからデータを取得
            var healthData = await _healthPlanetLogic.GetHealthDataAsync(period);

            if (!healthData.isSuccess)
            {
                return new BadRequestErrorMessageResult("HealthPlanetからのデータ取得に失敗しました。");
            }

            var healthRecordList = _healthPlanetLogic.ShapeHealthRecord(healthData.height, healthData.healthDataList);

            //身体データをDBに格納
            await _cosmosDbLogic.SetHealthPlanetHealthDataAsync(healthRecordList);

            return new OkObjectResult("");


        }

        /// <summary>
        /// HealthPlanet取得から投稿処理まで
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GetToPost")]
        public async Task<IActionResult> GetToPost(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var now = DateTime.UtcNow;

            var period = 7;
            if (!(string.IsNullOrEmpty(req.Query["period"])) && !int.TryParse(req.Query["period"], out period))
            {
                return new BadRequestErrorMessageResult("期間指定の値が数値以外の値になっています。");
            }

            if (0 > period || period > 120)
            {
                return new BadRequestErrorMessageResult("期間指定の値は0から120までの値を指定してください。");
            }

            //HealthPlanetからデータを取得
            var (isSuccess, height, healthDataList) = await _healthPlanetLogic.GetHealthDataAsync(period);

            if (!isSuccess)
            {
                return new BadRequestErrorMessageResult("HealthPlanetからのデータ取得に失敗しました。");
            }

            if (healthDataList.Count == 0)
            {
                return new BadRequestErrorMessageResult("期間内にHealthPlanetから有効なレコードが取得されませんでした。");
            }

            //DB格納用のリストを生成
            var healthRecordList = _healthPlanetLogic.ShapeHealthRecord(height, healthDataList);

            //身体データをDBに格納
            await _cosmosDbLogic.SetHealthPlanetHealthDataAsync(healthRecordList);

            var goal = await _cosmosDbLogic.GetGoalAsync();

            //投稿処理
            await _postHealthDataLogic.PostHealthDataAsync(
                healthRecordList.OrderByDescending(o => o.AssayDate).First(),
                goal);

            //GoogleFitへ投稿
            await _googleFitLogic.SetGoogleFit(healthRecordList);

            return new OkObjectResult("取得及び投稿完了");


        }

        /// <summary>
        /// リフレッシュトークンによるアクセストークン再取得処理
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("RefreshAccessToken")]
        public async Task<IActionResult> RefreshAccessToken(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var healthData = await _healthPlanetLogic.GetHealthPlanetRefreshTokenAsync();

            return new OkObjectResult("トークンの再取得が完了しました。");
        }
    }
}
