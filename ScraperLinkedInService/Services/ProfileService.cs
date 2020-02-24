using Flurl.Http;
using ScraperLinkedInService.Models.Entities;
using ScraperLinkedInService.Models.Request;
using ScraperLinkedInService.Models.Response;
using ScraperLinkedInService.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;

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
            _flurlClient.Settings.Timeout = TimeSpan.FromSeconds(60);
        }

        public ProfilesResponse GetProfilesForSearch(int profileBatchSize)
        {
            try
            {
                var response = _flurlClient.Request($"api/v1/profiles/for-search?profilesBatchSize={ profileBatchSize }")
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

                var response = _flurlClient.Request("api/v1/profiles/")
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

        public void UpdateProfiles(IEnumerable<ProfileViewModel> profilesVM, IEnumerable<string> rolesSearch, IEnumerable<string> technologiesSearch)
        {
            foreach (var profileVM in profilesVM)
            {
                if (profileVM.ProfileStatus != ProfileStatus.Unsuited)
                {
                    profileVM.ProfileStatus = GetProfileStatus(profileVM, rolesSearch, technologiesSearch);
                }
            }

            try
            {
                var request = new ProfilesRequest
                {
                    ProfilesViewModel = profilesVM
                };

                var response = _flurlClient.Request("api/v1/profiles/")
                    .WithOAuthBearerToken(_configuration.Token)
                    .PutJsonAsync(request)
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

        private ProfileStatus GetProfileStatus(ProfileViewModel profile, IEnumerable<string> rolesSearch, IEnumerable<string> technologiesSearch)
        {
            if (rolesSearch.Any(x => profile.Job.ToUpper().Split(' ').Contains(x.Trim())))
            {
                return ProfileStatus.Chief;
            }

            if (technologiesSearch.Any(y => profile.AllSkills.ToUpper().Contains(y.Trim())))
            {
                return ProfileStatus.Developer;
            }

            return ProfileStatus.Unsuited;
        }
    }
}
