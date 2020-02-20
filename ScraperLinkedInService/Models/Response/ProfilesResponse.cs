using ScraperLinkedInService.Models.Entities;
using System.Collections.Generic;

namespace ScraperLinkedInService.Models.Response
{
    public class ProfilesResponse : BaseResponse
    {
        public IEnumerable<ProfileViewModel> ProfilesViewModel { get; set; }

        public int CountProfilesInProcess { get; set; }

        public int CountNewProfiles { get; set; }
    }
}
