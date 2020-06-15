using ScraperLinkedInService.Models.Types;

namespace ScraperLinkedInService.Models.Entities
{
    public class CompanyViewModel
    {
        public int Id { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationURL { get; set; }
        public string Founders { get; set; }
        public string HeadquartersLocation { get; set; }
        public string Website { get; set; }
        public string LinkedInURL { get; set; }
        public string LogoUrl { get; set; }
        public string Specialties { get; set; }
        public ExecutionStatus ExecutionStatus { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string PhoneNumber { get; set; }
        public string AmountEmployees { get; set; }
        public string Industry { get; set; }
        public int AccountId { get; set; }
        public int LastScrapedPage { get; set; }
        public System.DateTime DateCreated { get; set; }
    }
}
