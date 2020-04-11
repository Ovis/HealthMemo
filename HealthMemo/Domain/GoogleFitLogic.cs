using HealthMemo.Entities.Configuration;
using Microsoft.Extensions.Options;

namespace HealthMemo.Domain
{
    public class GoogleFitLogic
    {
        private readonly GoogleConfiguration _googleConfiguration;

        public GoogleFitLogic(IOptions<GoogleConfiguration> googleConfiguration)
        {
            _googleConfiguration = googleConfiguration.Value;
        }


    }
}
