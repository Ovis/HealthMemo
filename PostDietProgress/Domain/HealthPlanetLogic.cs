using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PostDietProgress.Entities;

namespace PostDietProgress.Domain
{
    public class HealthPlanetLogic
    {
        private readonly HttpClient _httpClient;
        private readonly HealthPlanetConfiguration _healthPlanetConfiguration;

        private readonly CosmosDbLogic _cosmosDbLogic;

        public HealthPlanetLogic(HttpClient httpClient,
            IOptions<HealthPlanetConfiguration> healthPlanetConfiguration,
            CosmosDbLogic cosmosDbLogic)
        {
            _httpClient = httpClient;
            _healthPlanetConfiguration = healthPlanetConfiguration.Value;
            _cosmosDbLogic = cosmosDbLogic;
        }

        public async Task<bool> GetHealthPlanetToken(string code)
        {
            //TANITA HealthPlanet処理のためリダイレクト
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"client_id",_healthPlanetConfiguration.TanitaClientId},
                {"client_secret",_healthPlanetConfiguration.TanitaClientSecretToken},
                {"redirect_uri","https://www.healthplanet.jp/success.html"},
                {"code",code},
                {"grant_type","authorization_code"}
            });

            var res = await _httpClient.PostAsync("https://www.healthplanet.jp/oauth/token", content);

            await using var stream = (await res.Content.ReadAsStreamAsync());

            using var reader = (new StreamReader(stream, Encoding.GetEncoding("shift-jis"), true)) as TextReader;
            var jsonString = await reader.ReadToEndAsync();

            var tokenData = JsonSerializer.Deserialize<HealthPlanetToken>(jsonString);

            var result = await _cosmosDbLogic.SetHealthPlanetToken(tokenData);

            return result;
        }
    }
}
