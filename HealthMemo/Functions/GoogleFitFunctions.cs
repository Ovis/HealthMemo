using System;
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
            var period = 7;
            if (!(string.IsNullOrEmpty(req.Query["period"])) && !int.TryParse(req.Query["period"], out period))
            {
                return new BadRequestErrorMessageResult("期間指定の値が数値以外の値になっています。");
            }

            var now = DateTime.UtcNow;

            var isSuccess = false;

            var healthDataList = await _cosmosDbLogic.GetHealthPlanetPostDataPeriodAsync(now.AddDays(-period), now);

            if (healthDataList != null)
            {
                isSuccess = await _googleFitLogic.SetGoogleFit(healthDataList);
            }

            return isSuccess
                ? (IActionResult)new OkObjectResult("GoogleFitに投稿を行いました。")
                : new BadRequestObjectResult("投稿処理に失敗しました。");
        }
    }
}
