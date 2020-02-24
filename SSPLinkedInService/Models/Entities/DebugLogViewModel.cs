namespace SSPLinkedInService.Models.Entities
{
    public class DebugLogViewModel
    {
        public int Id { get; set; }
        public string Remarks { get; set; }
        public string Logs { get; set; }
        public System.DateTime? CreatedOn { get; set; }
        public int AccountId { get; set; }
    }
}
