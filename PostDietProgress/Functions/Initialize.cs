using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostDietProgress.Domain;
using PostDietProgress.Entities;

namespace PostDietProgress.Functions
{
    public class Initialize
    {
        private readonly CosmosDbConfiguration _settings;
        private readonly InitializeCosmosDbLogic _initializeCosmosDbLogic;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="options"></param>
        /// <param name="initializeCosmosDbLogic"></param>
        public Initialize(
            IOptions<CosmosDbConfiguration> options,
            InitializeCosmosDbLogic initializeCosmosDbLogic
            )
        {
            _settings = options.Value;
            _initializeCosmosDbLogic = initializeCosmosDbLogic;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("Initialize")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("�����������J�n");

            //�f�[�^�x�[�X�쐬
            {
                var databaseCreateResult = await _initializeCosmosDbLogic.CreateCosmosDbDatabaseIfNotExistsAsync();

                log.LogInformation(databaseCreateResult
                    ? $"CosmosDB�̃f�[�^�x�[�X���쐬���܂����B �f�[�^�x�[�X��:`{_settings.DatabaseId}`"
                    : $"�f�[�^�x�[�X��: `{_settings.DatabaseId}` �͂��łɑ��݂��܂��B");
            }


            //�ݒ���i�[�R���e�i�쐬
            {

                var settingContainerCreateResult =
                    await _initializeCosmosDbLogic.CreateSettingCosmosDbContainerIfNotExistsAsync();

                log.LogInformation(settingContainerCreateResult
                    ? $"CosmosDB�̃R���e�i���쐬���܂����B �R���e�i��:`{_settings.SettingContainerId}`"
                    : $"�f�[�^�x�[�X��: `{_settings.SettingContainerId}` �͂��łɑ��݂��܂��B");
            }

            //�g�̏��i�[�R���e�i�쐬
            {
                var bodyConditionContainerCreateResult =
                    await _initializeCosmosDbLogic.CreateBodyConditionCosmosDbContainerIfNotExistsAsync();

                log.LogInformation(bodyConditionContainerCreateResult
                    ? $"CosmosDB�̃R���e�i���쐬���܂����B �R���e�i��:`{_settings.DietDataContainerId}`"
                    : $"�f�[�^�x�[�X��: `{_settings.DietDataContainerId}` �͂��łɑ��݂��܂��B");
            }

            log.LogInformation("�����������������܂����B");

            return new OkObjectResult("");
        }

    }
}
