using ScraperLinkedInService.Models.Types;

namespace ScraperLinkedInService.Models.Entities
{
    public class SettingsViewModel
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
        public string Login { get; set; }
        public string TechnologiesSearch { get; set; }
        public string RolesSearch { get; set; }
        public ScraperStatus ScraperStatus { get; set; }
        public int? AccountId { get; set; }
    }
}
