using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostDietProgress.Domain;
using PostDietProgress.Entities;

[assembly: FunctionsStartup(typeof(PostDietProgress.Application.Startup))]
namespace PostDietProgress.Application
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

            builder.Services.AddScoped<InitializeCosmosDbLogic>();

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
