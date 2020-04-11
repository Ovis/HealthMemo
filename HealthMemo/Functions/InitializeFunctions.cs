using System.Text;
using System.Threading.Tasks;
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
        private readonly InitializeCosmosDbLogic _initializeCosmosDbLogic;
        private readonly HealthPlanetLogic _healthPlanetLogic;



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="cosmosDbConfiguration"></param>
        /// <param name="healthPlanetConfiguration"></param>
        /// <param name="initializeCosmosDbLogic"></param>
        /// <param name="healthPlanetLogic"></param>
        public InitializeFunctions(
            IOptions<CosmosDbConfiguration> cosmosDbConfiguration,
            IOptions<HealthPlanetConfiguration> healthPlanetConfiguration,
            InitializeCosmosDbLogic initializeCosmosDbLogic,
            HealthPlanetLogic healthPlanetLogic
            )
        {
            _cosmosDbConfiguration = cosmosDbConfiguration.Value;
            _healthPlanetConfiguration = healthPlanetConfiguration.Value;
            _initializeCosmosDbLogic = initializeCosmosDbLogic;
            _healthPlanetLogic = healthPlanetLogic;
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
                return new BadRequestResult();
            }

            var result = await _healthPlanetLogic.GetHealthPlanetTokenAsync(code);


            log.LogInformation("初期処理が完了しました。");

            return new OkObjectResult("");
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
            if (!double.TryParse(req.Query["GoalWeight"], out var goalWeight))
            {
                return new BadRequestErrorMessageResult("目標体重の値が数値ではありません。");
            }

            if (!double.TryParse(req.Query["OriginalWeight"], out var originalWeight))
            {
                return new BadRequestErrorMessageResult("元体重の値が数値ではありません。");
            }

            return await _initializeCosmosDbLogic.SetGoalAsync(goalWeight, originalWeight)
                ? (IActionResult)new OkResult()
                : new BadRequestErrorMessageResult("目標・元体重の設定に失敗しました。");
        }



    }
}
