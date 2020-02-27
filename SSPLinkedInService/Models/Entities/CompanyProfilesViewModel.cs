using System.Collections.Generic;

namespace SSPLinkedInService.Models.Entities
{
    public class CompanyProfilesViewModel : CompanyViewModel
    {
        public IEnumerable<ProfileViewModel> ProfilesViewModel { get; set; }
    }
}
