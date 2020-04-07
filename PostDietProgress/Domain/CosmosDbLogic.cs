﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using PostDietProgress.Entities;

namespace PostDietProgress.Domain
{
    public class CosmosDbLogic
    {
        private readonly CosmosDbConfiguration _cosmosDbConfiguration;
        private readonly CosmosClient _cosmosDbClient;
        private readonly Database _cosmosDatabase;
        private readonly Container _settingContainer;

        public CosmosDbLogic(IOptions<CosmosDbConfiguration> cosmosDbConfiguration, CosmosClient cosmosDbClient)
        {
            _cosmosDbConfiguration = cosmosDbConfiguration.Value;
            _cosmosDbClient = cosmosDbClient;

            _cosmosDatabase = _cosmosDbClient.GetDatabase(_cosmosDbConfiguration.DatabaseId);

            _settingContainer = _cosmosDbClient.GetContainer(_cosmosDbConfiguration.DatabaseId, _cosmosDbConfiguration.SettingContainerId);
        }

        public async Task<bool> SetHealthPlanetToken(HealthPlanetToken token)
        {
            var setting = new Setting()
            {
                Id = "setting",
                RequestToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresIn = DateTime.Now.AddDays(30)
            };

            try
            {
                await _settingContainer.UpsertItemAsync(setting);
            }
            catch (Exception e)
            {
                //TODO 
                Console.Write(e.Message);
                return false;
            }
            return true;
        }
    }
}