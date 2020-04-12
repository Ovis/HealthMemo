using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using HealthMemo.Domain;
using HealthMemo.Entities.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthMemo.Functions
{
    public class InitializeFunctions
    {
        private readonly CosmosDbConfiguration _cosmosDbConfiguration;
        private readonly HealthPlanetConfiguration _healthPlanetConfiguration;
        private readonly GoogleConfiguration _googleConfiguration;

        private readonly InitializeCosmosDbLogic _initializeCosmosDbLogic;
        private readonly HealthPlanetLogic _healthPlanetLogic;
        private readonly GoogleFitLogic _googleFitLogic;



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="cosmosDbConfiguration"></param>
        /// <param name="healthPlanetConfiguration"></param>
        /// <param name="googleConfiguration"></param>
        /// <param name="initializeCosmosDbLogic"></param>
        /// <param name="healthPlanetLogic"></param>
        /// <param name="googleFitLogic"></param>
        public InitializeFunctions(
            IOptions<CosmosDbConfiguration> cosmosDbConfiguration,
            IOptions<HealthPlanetConfiguration> healthPlanetConfiguration,
            IOptions<GoogleConfiguration> googleConfiguration,
            InitializeCosmosDbLogic initializeCosmosDbLogic,
            HealthPlanetLogic healthPlanetLogic,
            GoogleFitLogic googleFitLogic
            )
        {
            _cosmosDbConfiguration = cosmosDbConfiguration.Value;
            _healthPlanetConfiguration = healthPlanetConfiguration.Value;
            _googleConfiguration = googleConfiguration.Value;

            _initializeCosmosDbLogic = initializeCosmosDbLogic;
            _healthPlanetLogic = healthPlanetLogic;
            _googleFitLogic = googleFitLogic;
        }

        /// <summary>
        /// 初期処理
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("Initialize")]
        public async Task<IActionResult> InitializeProc(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("初期処理を開始");

            //データベース作成
            {
                var databaseCreateResult = await _initializeCosmosDbLogic.CreateCosmosDbDatabaseIfNotExistsAsync();

                log.LogInformation(databaseCreateResult
                    ? $"CosmosDBのデータベースを作成しました。 データベース名:`{_cosmosDbConfiguration.DatabaseId}`"
                    : $"データベース名: `{_cosmosDbConfiguration.DatabaseId}` はすでに存在します。");
            }


            //設定情報格納コンテナ作成
            {

                var settingContainerCreateResult =
                    await _initializeCosmosDbLogic.CreateSettingCosmosDbContainerIfNotExistsAsync();

                log.LogInformation(settingContainerCreateResult
                    ? $"CosmosDBのコンテナを作成しました。 コンテナ名:`{_cosmosDbConfiguration.SettingContainerId}`"
                    : $"データベース名: `{_cosmosDbConfiguration.SettingContainerId}` はすでに存在します。");
            }

            //身体情報格納コンテナ作成
            {
                var bodyConditionContainerCreateResult =
                    await _initializeCosmosDbLogic.CreateBodyConditionCosmosDbContainerIfNotExistsAsync();

                log.LogInformation(bodyConditionContainerCreateResult
                    ? $"CosmosDBのコンテナを作成しました。 コンテナ名:`{_cosmosDbConfiguration.HealthDataContainerId}`"
                    : $"データベース名: `{_cosmosDbConfiguration.HealthDataContainerId}` はすでに存在します。");
            }

            //TANITA HealthPlanet処理のためリダイレクト
            var authUrl = new StringBuilder();
            authUrl.Append("https://www.healthplanet.jp/oauth/auth?");
            authUrl.Append($"client_id={_healthPlanetConfiguration.TanitaClientId}");
            authUrl.Append($"&redirect_uri={_healthPlanetConfiguration.CallbackInitializeTanitaUrl}");
            authUrl.Append("&scope=innerscan");
            authUrl.Append("&response_type=code");

            return new RedirectResult(authUrl.ToString());
        }


        /// <summary>
        /// 初期処理
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("InitializeTanita")]
        public async Task<IActionResult> InitializeTanita(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var code = req.Query["code"];

            if (string.IsNullOrEmpty(code))
            {
                return new BadRequestErrorMessageResult("HealthPlanetからコードの取得に失敗しました。");
            }

            var result = await _healthPlanetLogic.GetHealthPlanetTokenAsync(code);


            log.LogInformation("初期処理が完了しました。");

            return new OkObjectResult("初期処理が完了しました。");
        }

        /// <summary>
        /// 元体重・目標体重設定
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("SetGoal")]
        public async Task<IActionResult> SetGoal(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var goalWeightQuery = req.Query["GoalWeight"];
            var originalWeightQuery = req.Query["OriginalWeight"];

            if (string.IsNullOrEmpty(goalWeightQuery))
            {
                return new BadRequestErrorMessageResult("目標体重が未入力です。");
            }

            if (string.IsNullOrEmpty(originalWeightQuery))
            {
                return new BadRequestErrorMessageResult("元体重が未入力です。");
            }

            if (!double.TryParse(goalWeightQuery, out var goalWeight))
            {
                return new BadRequestErrorMessageResult("目標体重の値が数値ではありません。");
            }

            if (!double.TryParse(originalWeightQuery, out var originalWeight))
            {
                return new BadRequestErrorMessageResult("元体重の値が数値ではありません。");
            }

            return await _initializeCosmosDbLogic.SetGoalAsync(goalWeight, originalWeight)
                ? (IActionResult)new OkObjectResult("元体重・目標体重の設定が完了しました。")
                : new BadRequestErrorMessageResult("目標・元体重の設定に失敗しました。");
        }

        /// <summary>
        /// GoogleFit連携処理
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("InitializeGoogleAuth")]
        public IActionResult InitializeGoogleAuth(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var query = HttpUtility.ParseQueryString("");

            query.Add("client_id", _googleConfiguration.ClientId);
            query.Add("redirect_uri", _googleConfiguration.CallbackInitializeUrl);
            query.Add("response_type", "code");
            query.Add("scope", $"https://www.googleapis.com/auth/fitness.body.read https://www.googleapis.com/auth/fitness.body.write");
            query.Add("access_type", "offline");

            var authUrl = new UriBuilder("https://accounts.google.com/o/oauth2/auth")
            {
                Query = query.ToString()
            };

            return new RedirectResult(authUrl.ToString());
        }

        /// <summary>
        /// GoogleFit連携処理
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GoogleAuthRedirect")]
        public async Task<IActionResult> GoogleAuthRedirect(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var code = req.Query["code"];

            await _googleFitLogic.GetGoogleOAuth(code);

            return new OkObjectResult("Googleとの連携処理を完了しました。");
        }

    }
}
