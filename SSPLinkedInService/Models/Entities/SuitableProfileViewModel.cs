using System;

namespace SSPLinkedInService.Models.Entities
{
    public class SuitableProfileViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Job { get; set; }
        public string PersonLinkedIn { get; set; }
        public string Company { get; set; }
        public string Website { get; set; }
        public string CompanyLogoUrl { get; set; }
        public string CrunchUrl { get; set; }
        public string Email { get; set; }
        public string EmailStatus { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string AmountEmployees { get; set; }
        public string Industry { get; set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }
        public string TechStack { get; set; }
        public DateTime? DateTimeCreation { get; set; }
        public int ProfileID { get; set; }
        public int AccountID { get; set; }
        public int CompanyID { get; set; }
    }
}
