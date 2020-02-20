using Flurl.Http;
using ScraperLinkedInService.Models.Entities;
using ScraperLinkedInService.Models.Request;
using ScraperLinkedInService.Models.Response;
using System;
using System.Collections.Generic;

namespace ScraperLinkedInService.Services
{
    class ProfileService : IDisposable
    {
        private AppServiceConfiguration _configuration;
        private readonly IFlurlClient _flurlClient;

        public ProfileService()
        {
            _configuration = AppServiceConfiguration.Instance;
            _flurlClient = new FlurlClient(_configuration.ServerURL);
        }

        public ProfilesResponse GetProfilesForSearch(int profileBatchSize)
        {
            try
            {
                var response = _flurlClient.Request($"api/v1/profiles/for-search?profileBatchSize={ profileBatchSize }")
                    .WithOAuthBearerToken(_configuration.Token)
                    .GetJsonAsync<ProfilesResponse>()
                    .Result;

                return response;
            }
            catch
            {
                _configuration.LogOut();
                return default;
            }
        }

        public int GetCountProfilesInProcess()
        {
            try
            {
                var response = _flurlClient.Request("api/v1/profiles/in-process/count")
                    .WithOAuthBearerToken(_configuration.Token)
                    .GetJsonAsync<ProfilesResponse>()
                    .Result;

                return response.CountProfilesInProcess;
            }
            catch
            {
                _configuration.LogOut();
                return default;
            }
        }

        public int GetCountNewProfiles()
        {
            try
            {
                var response = _flurlClient.Request("api/v1/profiles/new/count")
                    .WithOAuthBearerToken(_configuration.Token)
                    .GetJsonAsync<ProfilesResponse>()
                    .Result;

                return response.CountNewProfiles;
            }
            catch
            {
                _configuration.LogOut();
                return default;
            }
        }

        public void InsertProfiles(IEnumerable<ProfileViewModel> profilesVMs)
        {
            try
            {
                var request = new ProfilesRequest
                {
                    ProfilesViewModel = profilesVMs
                };

                var response = _flurlClient.Request("api/v1/profiles/windows-service-scraper")
                    .WithOAuthBearerToken(_configuration.Token)
                    .PostJsonAsync(request)
                    .ReceiveJson<ProfilesResponse>()
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
