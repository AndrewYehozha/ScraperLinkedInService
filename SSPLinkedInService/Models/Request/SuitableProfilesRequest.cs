using SSPLinkedInService.Models.Entities;
using System.Collections.Generic;

namespace SSPLinkedInService.Models.Request
{
    public class SuitableProfilesRequest
    {
        public IEnumerable<SuitableProfileViewModel> SuitableProfilesViewModel { get; set; }
    }
}
