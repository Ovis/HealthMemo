using System.Text;
using HealthMemo.Domain;
using HealthMemo.Entities.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(HealthMemo.Application.Startup))]
namespace HealthMemo.Application
{
    public class Startup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            //Shift-JISを用いるためのまじない
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            builder.Services.AddOptions<CosmosDbConfiguration>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("CosmosDbOptions").Bind(settings);
                });

            builder.Services.AddOptions<HealthPlanetConfiguration>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("HealthPlanetOptions").Bind(settings);
                });

            builder.Services.AddOptions<WebHookConfiguration>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("WebHookOptions").Bind(settings);
                });

            builder.Services.AddOptions<GoogleConfiguration>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("GoogleOptions").Bind(settings);
                });

            builder.Services.AddScoped<InitializeCosmosDbLogic>();
            builder.Services.AddScoped<HealthPlanetLogic>();
            builder.Services.AddScoped<CosmosDbLogic>();
            builder.Services.AddScoped<PostHealthDataLogic>();
            builder.Services.AddScoped<GoogleFitLogic>();

            builder.Services.AddHttpClient();

            builder.Services.AddSingleton((provider) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();

                var accountEndpoint = configuration.GetValue<string>("CosmosDbOptions:AccountEndpoint");
                var accountKey = configuration.GetValue<string>("CosmosDbOptions:AccountKey");

                var cosmosClientBuilder = new CosmosClientBuilder(accountEndpoint, accountKey);

                return cosmosClientBuilder.WithConnectionModeDirect()
                    .WithApplicationRegion(Regions.JapanEast)
                    .WithBulkExecution(true)
                    .WithConnectionModeDirect()
                    .WithSerializerOptions(
                        new CosmosSerializationOptions
                        {
                            IgnoreNullValues = true,
                            Indented = false,
                            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                        })
                    .Build();
            });
        }
    }
}
