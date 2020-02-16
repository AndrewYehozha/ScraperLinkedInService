using Flurl.Http;
using ScraperLinkedInService.Models.Request;
using ScraperLinkedInService.Models.Response;
using ScraperLinkedInService.Models.Types;
using System.Threading.Tasks;

namespace ScraperLinkedInService.Services
{
    public class SettingService
    {
        private readonly string _serverURL;

        public SettingService()
        {
            _serverURL = AppServiceConfiguration.Instance.ServerURL;
        }

        public AdvanceSettingsResponse GetAdvanceSettings(string token)
        {
            try
            {
                var response = (_serverURL + "api/v1/advance-settings")
                    .WithHeader("Authorization", "Bearer " + token)
                    .GetJsonAsync<AdvanceSettingsResponse>()
                    .Result;

                return response;
            }
            catch { }

            return null;
        }

        public SettingsResponse GetSettings(string token)
        {
            try
            {
                var response = (_serverURL + "api/v1/settings")
                    .WithHeader("Authorization", "Bearer " + token)
                    .GetJsonAsync<SettingsResponse>()
                    .Result;

                return response;
            }
            catch { }

            return null;
        }

        public async Task UpdateScraperStatus(string token, ScraperStatus status)
        {
            var requestModel = new UpdateScraperStatusRequest
            {
                Status = status
            };

            var response = await (_serverURL + "api/v1/settings/windows-service-scraper/scraper-status")
                .WithHeader("Authorization", "Bearer " + token)
                .PutJsonAsync(requestModel)
                .ReceiveJson<BaseResponse>();
        }
    }
}
