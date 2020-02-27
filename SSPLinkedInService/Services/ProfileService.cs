using Flurl.Http;
using SSPLinkedInService.Models.Request;
using SSPLinkedInService.Models.Response;
using SSPLinkedInService.Models.Types;
using System;
using System.Collections.Generic;

namespace SSPLinkedInService.Services
{
    public class ProfileService
    {
        private AppServiceConfiguration _configuration;
        private readonly IFlurlClient _flurlClient;

        public ProfileService()
        {
            _configuration = AppServiceConfiguration.Instance;
            _flurlClient = new FlurlClient(_configuration.ServerURL);
            _flurlClient.Settings.Timeout = TimeSpan.FromSeconds(60);
        }

        public void UpdateProfilesExecutionStatusByCompanyID(int accountId, ExecutionStatus executionStatus, int companyId)
        {
            try
            {
                var request = new UpdateProfileExecutionStatusRequest
                {
                    AccountId = accountId,
                    CompanyId = companyId,
                    ExecutionStatus = executionStatus
                };

                var response = _flurlClient.Request("api/v1/profiles/execution-status")
                    .WithOAuthBearerToken(_configuration.Token)
                    .PutJsonAsync(request)
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
