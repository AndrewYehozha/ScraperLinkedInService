using Flurl.Http;
using ScraperLinkedInService.Models.Entities;
using ScraperLinkedInService.Models.Request;
using ScraperLinkedInService.Models.Response;
using System;

namespace ScraperLinkedInService.Services
{
    public class CompanyService : IDisposable
    {
        private AppServiceConfiguration _configuration;
        private readonly IFlurlClient _flurlClient;

        public CompanyService()
        {
            _configuration = AppServiceConfiguration.Instance;
            _flurlClient = new FlurlClient(_configuration.ServerURL);
        }

        public CompaniesResponse GetCompaniesForSearch(int companyBatchSize)
        {
            try
            {
                var response = _flurlClient.Request($"api/v1/companies/for-search?companyBatchSize={ companyBatchSize }")
                    .WithOAuthBearerToken(_configuration.Token)
                    .GetJsonAsync<CompaniesResponse>()
                    .Result;

                return response;
            }
            catch
            {
                _configuration.LogOut();
                return default;
            }
        }

        public void UpdateCompany(CompanyViewModel companyVM)
        {
            try
            {
                var request = new CompanyRequest
                {
                    CompanyViewModel = companyVM
                };

                var response = _flurlClient.Request($"api/v1/companies/")
                    .WithOAuthBearerToken(_configuration.Token)
                    .PutJsonAsync(request)
                    .ReceiveJson<CompanyResponse>()
                    .Result;
            }
            catch
            {
                _configuration.LogOut();
            }
        }

        public void Dispose()
        {
            _flurlClient.Dispose();
        }
    }
}
