using Flurl.Http;
using ScraperLinkedInService.Models.Request;
using ScraperLinkedInService.Models.Response;
using System;
using System.Net;

namespace ScraperLinkedInService.Services
{
    public class AccountService : IDisposable
    {
        private AppServiceConfiguration _configuration;
        private readonly IFlurlClient _flurlClient;

        public AccountService()
        {
            _configuration = AppServiceConfiguration.Instance;
            _flurlClient = new FlurlClient(_configuration.ServerURL);
            _flurlClient.Settings.Timeout = TimeSpan.FromSeconds(60);
        }

        public AuthorizationServiceResponse Authorization()
        {
            try
            {
                var requestModel = new AuthorizationServiceRequest
                {
                    Guid = new Guid(_configuration.APIKey)
                };

                var response = _flurlClient.Request("api/v1/accounts/signin/windows-service")
                     .PostUrlEncodedAsync(requestModel)
                     .ReceiveJson<AuthorizationServiceResponse>()
                     .Result;

                _configuration.IsAuthorized = !string.IsNullOrEmpty(response.Token) && response.StatusCode == (int)HttpStatusCode.OK;
                _configuration.Token = !string.IsNullOrEmpty(response.Token) ? response.Token : string.Empty;

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
