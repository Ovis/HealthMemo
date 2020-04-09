using System.Threading.Tasks;
using System.Web.Http;
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
        /// <param name="cosmosDbLogic"></param>
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
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var period = 30;
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

            //身体データをDBに格納
            await _cosmosDbLogic.SetHealthPlanetHealthDataAsync(healthData);

            return new OkObjectResult("");


        }
    }
}
