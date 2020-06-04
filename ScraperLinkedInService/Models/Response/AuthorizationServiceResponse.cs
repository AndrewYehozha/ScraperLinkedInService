namespace ScraperLinkedInService.Models.Response
{
    public class AuthorizationServiceResponse : BaseResponse
    {
        public string Token { get; set; }
        public System.DateTime TokenExpires { get; set; }
    }
}
