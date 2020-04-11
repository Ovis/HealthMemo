using System;
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

        /// <summary>
        ///  GoogleOAuthトークン取得処理
        /// </summary>
        /// <param name="code"></param>
        /// <param name="isRefresh"></param>
        /// <returns></returns>
        public async Task<(bool isSuccess, Token token)> GetGoogleOAuth(string code, bool isRefresh = false)
        {
            var dic = new Dictionary<string, string>();

            if (isRefresh)
            {
                dic.Add("client_id", _googleConfiguration.ClientId);
                dic.Add("client_secret", _googleConfiguration.ClientSecret);
                dic.Add("refresh_token", code);
                dic.Add("grant_type", "refresh_token");
            }
            else
            {
                dic.Add("client_id", _googleConfiguration.ClientId);
                dic.Add("client_secret", _googleConfiguration.ClientSecret);
                dic.Add("redirect_uri", _googleConfiguration.CallbackInitializeUrl);
                dic.Add("code", code);
                dic.Add("grant_type", "authorization_code");
            }

            var content = new FormUrlEncodedContent(dic);

            var res = await _httpClient.PostAsync("https://accounts.google.com/o/oauth2/token", content);

            await using var stream = (await res.Content.ReadAsStreamAsync());

            using var reader = (new StreamReader(stream, Encoding.GetEncoding("shift-jis"), true)) as TextReader;
            var jsonString = await reader.ReadToEndAsync();

            var tokenData = JsonSerializer.Deserialize<Token>(jsonString);

            var result = await _cosmosDbLogic.SetGoogleToken(tokenData);

            return (result, tokenData);
        }


        public async Task<bool> SetGoogleFit()
        {
            var token = await GetToken();

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Googleのアクセストークンを取得(失効時は再取得)
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetToken()
        {
            var tokenData = await _cosmosDbLogic.GetGoogleTokenAsync();
            var token = "";

            if (tokenData == null)
            {
                return token;
            }

            if (tokenData.ExpiresIn < DateTime.Now)
            {
                var refresh = await GetGoogleOAuth(tokenData.RefreshToken);

                if (refresh.isSuccess)
                {
                    token = refresh.token.AccessToken;
                }
            }
            else
            {
                token = tokenData.AccessToken;
            }

            return token;
        }
    }
}
