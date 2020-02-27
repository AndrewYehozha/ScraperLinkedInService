using ScraperLinkedInService.Models.Entities;
using System.Collections.Generic;

namespace ScraperLinkedInService.Models.Response
{
    public class DebugLogResponse : BaseResponse
    {
        public DebugLogViewModel DebugLogViewModel { get; set; }
    }

    public class DebugLogsResponse : BaseResponse
    {
        public IEnumerable<DebugLogViewModel> DebugLogsViewModel { get; set; }
    }
}
