using Flurl.Http;
using ScraperLinkedInService.Models.Entities;
using ScraperLinkedInService.Models.Request;
using ScraperLinkedInService.Models.Response;
using System;
using System.Collections.Generic;

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
            _flurlClient.Settings.Timeout = TimeSpan.FromSeconds(60);
        }

        public void SendDebugLog(string logs, string remarks)
        {
            if (_configuration.IsAuthorized)
            {
                var requestModel = new DebugLogRequest
                {
                    DebugLogViewModel = GenerateViewModel(logs, remarks)
                };

                try
                {
                    var response = _flurlClient.Request("api/v1/debug-logs/log")
                        .WithOAuthBearerToken(_configuration.Token)
                        .PostJsonAsync(requestModel)
                        .ReceiveJson<DebugLogsResponse>()
                        .Result;
                }
                catch
                {
                    _configuration.LogOut();
                }
            }
        }

        public void SendDebugLogs(IEnumerable<DebugLogViewModel> debugLogsVM)
        {
            if (_configuration.IsAuthorized)
            {
                var requestModel = new DebugLogsRequest
                {
                    DebugLogsViewModel = debugLogsVM
                };

                try
                {
                    var response = _flurlClient.Request("api/v1/debug-logs/")
                        .WithOAuthBearerToken(_configuration.Token)
                        .PostJsonAsync(requestModel)
                        .ReceiveJson<DebugLogsResponse>()
                        .Result;
                }
                catch
                {
                    _configuration.LogOut();
                }
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
