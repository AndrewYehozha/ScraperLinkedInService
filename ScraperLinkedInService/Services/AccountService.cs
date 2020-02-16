using Flurl.Http;
using ScraperLinkedInService.Models.Request;
using ScraperLinkedInService.Models.Response;
using ScraperLinkedInService.Models.Types;
using System;
using System.Threading.Tasks;

namespace ScraperLinkedInService.Services
{
    public class AccountService
    {
        private readonly string _APIKey;
        private readonly string _serverURL;

        public AccountService()
        {
            _APIKey = AppServiceConfiguration.Instance.APIKey;
            _serverURL = AppServiceConfiguration.Instance.ServerURL;
        }

        public AuthorizationServiceResponse Authorization()
        {
            try
            {
                var requestModel = new AuthorizationServiceRequest
                {
                    Guid = new Guid(_APIKey)
                };

                var response = (_serverURL + "api/v1/accounts/windows-service/signin")
                    .PostUrlEncodedAsync(requestModel)
                    .ReceiveJson<AuthorizationServiceResponse>()
                    .Result;

                return response;
            }
            catch { }

            return null;
        }
    }
}
