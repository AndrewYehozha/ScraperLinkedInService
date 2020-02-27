using Flurl.Http;
using SSPLinkedInService.Models.Entities;
using SSPLinkedInService.Models.Request;
using SSPLinkedInService.Models.Response;
using System;
using System.Collections.Generic;

namespace SSPLinkedInService.Services
{
    public class SuitableProfileService
    {
        private AppServiceConfiguration _configuration;
        private readonly IFlurlClient _flurlClient;

        public SuitableProfileService()
        {
            _configuration = AppServiceConfiguration.Instance;
            _flurlClient = new FlurlClient(_configuration.ServerURL);
            _flurlClient.Settings.Timeout = TimeSpan.FromSeconds(60);
        }

        public void InsertSuitableProfile(IEnumerable<SuitableProfileViewModel> profilesVMs)
        {
            try
            {
                var request = new SuitableProfilesRequest
                {
                    SuitableProfilesViewModel = profilesVMs
                };

                var response = _flurlClient.Request("api/v1/suitable-profiles/")
                    .WithOAuthBearerToken(_configuration.Token)
                    .PostJsonAsync(request)
                    .ReceiveJson<BaseResponse>()
                    .Result;
            }
            catch
            {
                _configuration.LogOut();
            }
        }
    }
}
