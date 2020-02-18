using Flurl.Http;
using ScraperLinkedInService.Models.Request;
using ScraperLinkedInService.Models.Response;
using ScraperLinkedInService.Models.Types;
using System;

namespace ScraperLinkedInService.Services
{
    public class SettingService : IDisposable
    {
        private AppServiceConfiguration _configuration;
        private readonly IFlurlClient _flurlClient;

        public SettingService()
        {
            _configuration = AppServiceConfiguration.Instance;
            _flurlClient = new FlurlClient(_configuration.ServerURL);
        }

        public AdvanceSettingsResponse GetAdvanceSettings()
        {
            try
            {
                var response = _flurlClient.Request("api/v1/advance-settings")
                    .WithOAuthBearerToken(_configuration.Token)
                    .GetJsonAsync<AdvanceSettingsResponse>()
                    .Result;

                return response;
            }
            catch
            {
                _configuration.LogOut();
                return default;
            }
        }

        public SettingsResponse GetSettings()
        {
            try
            {
                var response = _flurlClient.Request("api/v1/settings")
                    .WithOAuthBearerToken(_configuration.Token)
                    .GetJsonAsync<SettingsResponse>()
                    .Result;

                return response;
            }
            catch
            {
                _configuration.LogOut();
                return default;
            }
        }

        public BaseResponse UpdateScraperStatus(ScraperStatus status)
        {
            var requestModel = new UpdateScraperStatusRequest
            {
                Status = status
            };

            try
            {
                var response = _flurlClient.Request("api/v1/settings/windows-service-scraper/scraper-status")
                    .WithOAuthBearerToken(_configuration.Token)
                    .PutJsonAsync(requestModel)
                    .ReceiveJson<BaseResponse>().Result;

                return response;
            }
            catch
            {
                _configuration.LogOut();
                return default;
            }
        }

        public void Dispose()
        {
            _flurlClient.Dispose();
        }
    }
}
