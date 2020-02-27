using SSPLinkedInService.Models.Entities;
using System.Collections.Generic;

namespace SSPLinkedInService.Models.Response
{
    public class CompaniesProfilesResponse : BaseResponse
    {
        public IEnumerable<CompanyProfilesViewModel> CompanyProfilesViewModel { get; set; }
    }
}
