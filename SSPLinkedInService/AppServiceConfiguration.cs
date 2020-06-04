using SSPLinkedInService.Extensions;
using System.Configuration;

namespace SSPLinkedInService
{
    public class AppServiceConfiguration
    {
        private AppServiceConfiguration() { }
        private static AppServiceConfiguration _instance { get; set; }

        public string APIKey { get; set; }
        public string ServerURL { get; set; }
        public int SearchProfilesBatchSize { get; set; }
        public string Token { get; set; }
        public System.DateTime TokenExpires { get; set; }
        public bool IsAuthorized { get; set; }

        public static AppServiceConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppServiceConfiguration
                    {
                        APIKey = ConfigurationManager.AppSettings["APIKey"] ?? string.Empty,
                        ServerURL = ConfigurationManager.AppSettings["ServerURL"] ?? string.Empty,
                        SearchProfilesBatchSize = (ConfigurationManager.AppSettings["SEARCH_PROFILES_BATCH_SIZE"]).AsInt32(50),
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
