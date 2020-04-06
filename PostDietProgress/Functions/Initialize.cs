using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostDietProgress.Domain;
using PostDietProgress.Entities;

namespace PostDietProgress.Functions
{
    public class Initialize
    {
        private readonly CosmosDbConfiguration _settings;
        private readonly InitializeCosmosDbLogic _initializeCosmosDbLogic;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="options"></param>
        /// <param name="initializeCosmosDbLogic"></param>
        public Initialize(
            IOptions<CosmosDbConfiguration> options,
            InitializeCosmosDbLogic initializeCosmosDbLogic
            )
        {
            _settings = options.Value;
            _initializeCosmosDbLogic = initializeCosmosDbLogic;
        }

        /// <summary>
        /// 初期処理
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("Initialize")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("初期処理を開始");

            //データベース作成
            {
                var databaseCreateResult = await _initializeCosmosDbLogic.CreateCosmosDbDatabaseIfNotExistsAsync();

                log.LogInformation(databaseCreateResult
                    ? $"CosmosDBのデータベースを作成しました。 データベース名:`{_settings.DatabaseId}`"
                    : $"データベース名: `{_settings.DatabaseId}` はすでに存在します。");
            }


            //設定情報格納コンテナ作成
            {

                var settingContainerCreateResult =
                    await _initializeCosmosDbLogic.CreateSettingCosmosDbContainerIfNotExistsAsync();

                log.LogInformation(settingContainerCreateResult
                    ? $"CosmosDBのコンテナを作成しました。 コンテナ名:`{_settings.SettingContainerId}`"
                    : $"データベース名: `{_settings.SettingContainerId}` はすでに存在します。");
            }

            //身体情報格納コンテナ作成
            {
                var bodyConditionContainerCreateResult =
                    await _initializeCosmosDbLogic.CreateBodyConditionCosmosDbContainerIfNotExistsAsync();

                log.LogInformation(bodyConditionContainerCreateResult
                    ? $"CosmosDBのコンテナを作成しました。 コンテナ名:`{_settings.DietDataContainerId}`"
                    : $"データベース名: `{_settings.DietDataContainerId}` はすでに存在します。");
            }

            log.LogInformation("初期処理が完了しました。");

            return new OkObjectResult("");
        }

    }
}
