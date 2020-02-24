using ScraperLinkedInService.Models.Entities;

namespace ScraperLinkedInService.Models.Request
{
    public class CompanyRequest
    {
        public CompanyViewModel CompanyViewModel { get; set; }
    }

    public class CompanyLastPageRequest
    {
        public int CompanyId { get; set; }

        public int LastScrapedPage { get; set; }
    }
}
