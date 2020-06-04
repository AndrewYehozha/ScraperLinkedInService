using System.Configuration;

namespace ScraperLinkedInService
{
    public class AppServiceConfiguration
    {
        private AppServiceConfiguration() { }
        private static AppServiceConfiguration _instance { get; set; }

        public string APIKey { get; set; }
        public string ServerURL { get; set; }
        public string Token { get; set; }
        public System.DateTime TokenExpires { get; set; }
        public int AccountId { get; set; }
        public bool IsAuthorized { get; set; }

        public static AppServiceConfiguration Instance
        {
            get
            {
                if(_instance == null) {
                    _instance = new AppServiceConfiguration
                    {
                        APIKey = ConfigurationManager.AppSettings["APIKey"] ?? string.Empty,
                        ServerURL = ConfigurationManager.AppSettings["ServerURL"] ?? string.Empty,
                        Token = string.Empty,
                        TokenExpires = new System.DateTime(1900, 1, 1),
                        IsAuthorized = false
                    };
                }

                return _instance;
            }
        }

        public void LogOut()
        {
            _instance.Token = string.Empty;
            _instance.TokenExpires = new System.DateTime(1900, 1, 1);
            _instance.IsAuthorized = false;
        }
    }
}
