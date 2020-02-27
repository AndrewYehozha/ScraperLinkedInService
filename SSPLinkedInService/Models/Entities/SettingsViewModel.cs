using SSPLinkedInService.Models.Types;

namespace SSPLinkedInService.Models.Entities
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
        public bool IsSearchChiefs { get; set; }
        public bool IsSearchDevelopers { get; set; }
        public int? AccountId { get; set; }
    }
}
