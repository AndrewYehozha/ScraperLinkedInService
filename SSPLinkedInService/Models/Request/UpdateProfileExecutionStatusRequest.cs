using SSPLinkedInService.Models.Types;

namespace SSPLinkedInService.Models.Request
{
    public class UpdateProfileExecutionStatusRequest
    {
        public ExecutionStatus ExecutionStatus { get; set; }
        public int CompanyId { get; set; }
        public int AccountId { get; set; }
    }
}
