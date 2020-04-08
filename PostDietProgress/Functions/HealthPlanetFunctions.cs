using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PostDietProgress.Domain;

namespace PostDietProgress.Functions
{

    public class HealthPlanetFunctions
    {
        private readonly CosmosDbLogic _cosmosDbLogic;
        private readonly HealthPlanetLogic _healthPlanetLogic;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="healthPlanetLogic"></param>
        public HealthPlanetFunctions(
            CosmosDbLogic cosmosDbLogic,
            HealthPlanetLogic healthPlanetLogic
        )
        {
            _cosmosDbLogic = cosmosDbLogic;
            _healthPlanetLogic = healthPlanetLogic;
        }

        [FunctionName("GetHealthPlanet")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //HealthPlanetからデータを取得
            var healthData = await _healthPlanetLogic.GetHealthDataAsync();

            //取得データをもとに身体データをリスト化
            var healthList = _healthPlanetLogic.ShapeHealthData(healthData);

            await _cosmosDbLogic.SetHealthPlanetHealthDataAsync(healthList);

            return new OkObjectResult("");


        }
    }
}
