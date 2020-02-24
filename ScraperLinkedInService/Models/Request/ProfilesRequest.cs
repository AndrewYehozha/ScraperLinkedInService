using ScraperLinkedInService.Models.Entities;
using System.Collections.Generic;

namespace ScraperLinkedInService.Models.Request
{
    public class ProfilesRequest
    {
        public IEnumerable<ProfileViewModel> ProfilesViewModel { get; set; }
    }
}
