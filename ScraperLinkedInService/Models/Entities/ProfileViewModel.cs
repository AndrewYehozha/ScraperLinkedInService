using ScraperLinkedInService.Models.Types;

namespace ScraperLinkedInService.Models.Entities
{
    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Job { get; set; }
        public string ProfileUrl { get; set; }
        public string AllSkills { get; set; }
        public int CompanyID { get; set; }
        public string OrganizationName { get; set; }
        public System.DateTime DateСreation { get; set; }
        public ExecutionStatus ExecutionStatus { get; set; }
        public ProfileStatus ProfileStatus { get; set; }
    }
}
