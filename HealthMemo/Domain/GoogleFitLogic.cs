using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HealthMemo.Entities.Configuration;
using HealthMemo.Entities.DbEntity;
using HealthMemo.Entities.GoogleEntity;
using HealthMemo.Extensions;
using Microsoft.Extensions.Options;
using DataSource = HealthMemo.Entities.GoogleEntity.DataSource;
using Token = HealthMemo.Entities.GoogleEneity.Token;

namespace HealthMemo.Domain
{
    public class GoogleFitLogic
    {
        private readonly GoogleConfiguration _googleConfiguration;

        private readonly CosmosDbLogic _cosmosDbLogic;

        private readonly HttpClient _httpClient;

        private const string UserId = "me";
        private const string DataTypeName = "com.google.weight";

        private readonly JsonSerializerOptions _serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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

            if (isRefresh && tokenData.RefreshToken == null)
            {
                //初回認証時でしかリフレッシュトークンをもらえない
                tokenData.RefreshToken = code;

            }

            var result = await _cosmosDbLogic.SetGoogleToken(tokenData);

            return (result, tokenData);
        }

        /// <summary>
        /// GoogleFitに体重を送信する
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetGoogleFit(List<HealthRecord> healthRecordList)
        {
            var isSuccess = true;

            var token = await GetToken();

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var dataSource = new DataSource();

            var dataSourceId = $"{dataSource.Type}:{dataSource.DataType.Name}:{_googleConfiguration.ClientId.Split('-')[0]}:{dataSource.Device.Manufacturer}:{dataSource.Device.Model}:{dataSource.Device.Uid}:{dataSource.DataStreamName}";

            try
            {
                if (!(await CheckDataSourceAsync(token, dataSourceId)))
                {
                    //データソース作成
                    if (!(await CreateDataSourceAsync(token, dataSource)))
                    {
                        return false;
                    }
                }

                foreach (var healthRecord in healthRecordList)
                {
                    var date = (DateTime)healthRecord.AssayDate;

                    var postNanosecond = ParseExtensions.GetUnixEpochNanoseconds(date.ToUniversalTime());

                    var widthDataSet = new Dataset()
                    {
                        DataSourceId = dataSourceId,
                        MaxEndTimeNs = postNanosecond,
                        MinStartTimeNs = postNanosecond,
                        Point = new List<Point>()
                        {
                            new Point()
                            {
                                DataTypeName = DataTypeName,
                                StartTimeNanos = postNanosecond,
                                EndTimeNanos = postNanosecond,
                                Value = new List<Value>()
                                {
                                    new Value()
                                    {
                                        FpVal = (double)healthRecord.Weight
                                    }
                                }
                            }
                        }
                    };

                    if (!(await PostWeightDataAsync(token, widthDataSet)))
                    {
                        isSuccess = false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return isSuccess;
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
                var refresh = await GetGoogleOAuth(tokenData.RefreshToken, true);

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


        /// <summary>
        /// データソース存在確認
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dataSourceId"></param>
        /// <returns></returns>
        private async Task<bool> CheckDataSourceAsync(string token, string dataSourceId)
        {
            try
            {
                var res = await _httpClient.GetAsync($"https://www.googleapis.com/fitness/v1/users/me/dataSources?access_token={token}");

                await using var stream = (await res.Content.ReadAsStreamAsync());

                using var reader = (new StreamReader(stream, Encoding.GetEncoding("shift-jis"), true)) as TextReader;
                var jsonString = await reader.ReadToEndAsync();

                var dataSources = (JsonSerializer.Deserialize<DataSourceList>(jsonString, _serializeOptions)).DataSource;

                //データソース存在確認
                return dataSources.Any(data => data.DataStreamId == dataSourceId);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// データソース作成
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        private async Task<bool> CreateDataSourceAsync(string token, DataSource dataSource)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://www.googleapis.com/fitness/v1/users/me/dataSources");
                request.Headers.Add("ContentType", "application/json");
                request.Headers.Add("Authorization", $"Bearer {token}");
                request.Content = new StringContent(JsonSerializer.Serialize(dataSource, _serializeOptions), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);

                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// GoogleFit体重投稿
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        private async Task<bool> PostWeightDataAsync(string token, Dataset dataSet)
        {
            var patchRequest = new HttpRequestMessage(HttpMethod.Patch, $"https://www.googleapis.com/fitness/v1/users/me/dataSources/{dataSet.DataSourceId}/datasets/{dataSet.MinStartTimeNs}-{dataSet.MaxEndTimeNs}");
            patchRequest.Headers.Add("ContentType", "application/json");
            patchRequest.Headers.Add("Authorization", $"Bearer {token}");
            patchRequest.Content = new StringContent(JsonSerializer.Serialize(dataSet, _serializeOptions), Encoding.UTF8, "application/json");

            var patchResponse = await _httpClient.SendAsync(patchRequest);

            return patchResponse.IsSuccessStatusCode;
        }
    }
}
