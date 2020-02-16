using ScraperLinkedInService.Models.Types;

namespace ScraperLinkedInService.Models.Entities
{
    public class AdvanceSettingsViewModel
    {
        public int Id { get; set; }
        public System.TimeSpan TimeStart { get; set; }
        public IntervalType IntervalType { get; set; }
        public int IntervalValue { get; set; }
        public int CompanyBatchSize { get; set; }
        public int ProfileBatchSize { get; set; }
        public int AccountId { get; set; }
    }
}
