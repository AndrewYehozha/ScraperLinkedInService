using Flurl.Http;
using SSPLinkedInService.Models.Entities;
using SSPLinkedInService.Models.Request;
using SSPLinkedInService.Models.Response;
using System;

namespace SSPLinkedInService.Services
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
                        .ReceiveJson<DebugLogResponse>()
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
