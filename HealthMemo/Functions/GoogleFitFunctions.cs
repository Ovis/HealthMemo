using System;
using System.Threading.Tasks;
using HealthMemo.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace HealthMemo.Functions
{

    public class GoogleFitFunctions
    {
        private readonly CosmosDbLogic _cosmosDbLogic;
        private readonly GoogleFitLogic _googleFitLogic;

        public GoogleFitFunctions(GoogleFitLogic googleFitLogic, CosmosDbLogic cosmosDbLogic)
        {
            _cosmosDbLogic = cosmosDbLogic;
            _googleFitLogic = googleFitLogic;
        }

        [FunctionName("PostGoogleFit")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var now = DateTime.Now;
            var healthDataList = await _cosmosDbLogic.GetHealthPlanetPostDataPeriodAsync(now.AddDays(-7), now);
            await _googleFitLogic.SetGoogleFit(healthDataList);

            return new OkObjectResult("GoogleFitに投稿を行いました。");
        }
    }
}
