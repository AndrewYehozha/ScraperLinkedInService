using System.Collections.Generic;

namespace SSPLinkedInService.Models.Response
{
    public class AccountsIdsResponse : BaseResponse
    {
        public IEnumerable<int> Ids { get; set; }
    }
}
