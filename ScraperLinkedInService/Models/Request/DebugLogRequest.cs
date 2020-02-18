using ScraperLinkedInService.Models.Entities;
using System.Collections.Generic;

namespace ScraperLinkedInService.Models.Request
{
    public class DebugLogRequest
    {
        public DebugLogViewModel DebugLogViewModel { get; set; }
    }

    public class DebugLogsRequest
    {
        public IEnumerable<DebugLogViewModel> DebugLogsViewModel { get; set; }
    }
}
