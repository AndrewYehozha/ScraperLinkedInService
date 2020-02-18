using Flurl.Http;
using ScraperLinkedInService.Models.Entities;
using ScraperLinkedInService.Models.Request;
using ScraperLinkedInService.Models.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScraperLinkedInService.Services
{
    public class DebugLogService : IDisposable
    {
        private AppServiceConfiguration _configuration;
        private readonly IFlurlClient _flurlClient;

        public DebugLogService()
        {
            _configuration = AppServiceConfiguration.Instance;
            _flurlClient = new FlurlClient(_configuration.ServerURL);
        }

        public async Task SendDebugLogAsync(DebugLogViewModel debugLogVM)
        {
            var requestModel = new DebugLogRequest
            {
                DebugLogViewModel = debugLogVM
            };

            try
            {
                await _flurlClient.Request("api/v1/debug-logs/windows-service-scraper")
                    .WithOAuthBearerToken(_configuration.Token)
                    .PutJsonAsync(requestModel)
                    .ReceiveJson<DebugLogsResponse>();

            }
            catch
            {
                _configuration.LogOut();
            }
        }

        public async Task SendDebugLogsAsync(IEnumerable<DebugLogViewModel> debugLogsVM)
        {
            var requestModel = new DebugLogsRequest
            {
                DebugLogsViewModel = debugLogsVM
            };

            try
            {
                await _flurlClient.Request("api/v1/debug-logs/windows-service-scraper")
                    .WithOAuthBearerToken(_configuration.Token)
                    .PutJsonAsync(requestModel)
                    .ReceiveJson<DebugLogsResponse>();

            }
            catch
            {
                _configuration.LogOut();
            }
        }

        public DebugLogViewModel GenerateViewModel(string logs, string remarks)
        {
            return new DebugLogViewModel { Logs = logs, Remarks = remarks, CreatedOn = DateTime.UtcNow };
        }

        public void Dispose()
        {
            _flurlClient.Dispose();
        }
    }
}
