using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HealthMemo.Entities.Configuration;
using HealthMemo.Entities.GoogleEneity;
using Microsoft.Extensions.Options;

namespace HealthMemo.Domain
{
    public class GoogleFitLogic
    {
        private readonly GoogleConfiguration _googleConfiguration;

        private readonly CosmosDbLogic _cosmosDbLogic;

        private readonly HttpClient _httpClient;

        public GoogleFitLogic(
            IOptions<GoogleConfiguration> googleConfiguration,
            CosmosDbLogic cosmosDbLogic,
            HttpClient httpClient)
        {
            _googleConfiguration = googleConfiguration.Value;
            _cosmosDbLogic = cosmosDbLogic;
            _httpClient = httpClient;
        }

        public async Task<bool> GetGoogleOAuth(string code)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"client_id",_googleConfiguration.ClientId},
                {"client_secret",_googleConfiguration.ClientSecret},
                {"redirect_uri",_googleConfiguration.CallbackInitializeUrl},
                {"code",code},
                {"grant_type","authorization_code"}
            });

            var res = await _httpClient.PostAsync("https://accounts.google.com/o/oauth2/token", content);

            await using var stream = (await res.Content.ReadAsStreamAsync());

            using var reader = (new StreamReader(stream, Encoding.GetEncoding("shift-jis"), true)) as TextReader;
            var jsonString = await reader.ReadToEndAsync();

            var tokenData = JsonSerializer.Deserialize<Token>(jsonString);

            var result = await _cosmosDbLogic.SetGoogleToken(tokenData);

            return result;
        }
    }
}
