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
    public class PostHealthDataFunctions
    {
        private readonly CosmosDbLogic _cosmosDbLogic;
        private readonly PostHealthDataLogic _postHealthDataLogic;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="cosmosDbLogic"></param>
        /// <param name="postHealthDataLogic"></param>
        public PostHealthDataFunctions(
            CosmosDbLogic cosmosDbLogic,
            PostHealthDataLogic postHealthDataLogic
        )
        {
            _cosmosDbLogic = cosmosDbLogic;
            _postHealthDataLogic = postHealthDataLogic;
        }

        [FunctionName("PostHealthData")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var healthRecord = await _cosmosDbLogic.GetHealthPlanetPostDataAsync();

            if (healthRecord == null)
            {
                return new BadRequestErrorMessageResult("最新の身体データ取得に失敗しました。");
            }

            var goal = await _cosmosDbLogic.GetGoalAsync();

            await _postHealthDataLogic.PostHealthDataAsync(healthRecord, goal);

            return new OkObjectResult("");
        }
    }
}
