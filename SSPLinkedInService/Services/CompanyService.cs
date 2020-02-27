using Flurl.Http;
using SSPLinkedInService.Models.Response;
using System;

namespace SSPLinkedInService.Services
{
    public class CompanyService : IDisposable
    {
        private AppServiceConfiguration _configuration;
        private readonly IFlurlClient _flurlClient;

        public CompanyService()
        {
            _configuration = AppServiceConfiguration.Instance;
            _flurlClient = new FlurlClient(_configuration.ServerURL);
            _flurlClient.Settings.Timeout = TimeSpan.FromSeconds(60);
        }

        public CompaniesProfilesResponse GetCompaniesForSearch(int accountId, int companyBatchSize)
        {
            try
            {
                var response = _flurlClient.Request($"api/v1/companies/for-search-suitable-profiles?accountId={ accountId }&companyBatchSize={ companyBatchSize }")
                    .WithOAuthBearerToken(_configuration.Token)
                    .GetJsonAsync<CompaniesProfilesResponse>()
                    .Result;

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
