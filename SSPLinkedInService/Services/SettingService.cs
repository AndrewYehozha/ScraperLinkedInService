using Flurl.Http;
using SSPLinkedInService.Models.Request;
using SSPLinkedInService.Models.Response;
using SSPLinkedInService.Models.Types;
using System;

namespace SSPLinkedInService.Services
{
    public class SettingService : IDisposable
    {
        private AppServiceConfiguration _configuration;
        private readonly IFlurlClient _flurlClient;

        public SettingService()
        {
            _configuration = AppServiceConfiguration.Instance;
            _flurlClient = new FlurlClient(_configuration.ServerURL);
            _flurlClient.Settings.Timeout = TimeSpan.FromSeconds(60);
        }

        public SettingsResponse GetSettingsByAccountId(int accountId)
        {
            try
            {
                var response = _flurlClient.Request($"api/v1/settings/setting/{ accountId }")
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

        public void UpdateScraperStatus(ScraperStatus status)
        {
            if (_configuration.IsAuthorized)
            {
                var requestModel = new UpdateScraperStatusRequest
                {
                    Status = status
                };

                try
                {
                    var response = _flurlClient.Request("api/v1/settings/setting/scraper-status")
                        .WithOAuthBearerToken(_configuration.Token)
                        .PutJsonAsync(requestModel)
                        .ReceiveJson<BaseResponse>().Result;
                }
                catch
                {
                    _configuration.LogOut();
                }
            }
        }

        public void Dispose()
        {
            _flurlClient.Dispose();
        }
    }
}
