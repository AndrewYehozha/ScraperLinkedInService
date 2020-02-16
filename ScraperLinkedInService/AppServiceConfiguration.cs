using System.Configuration;

namespace ScraperLinkedInService
{
    public class AppServiceConfiguration
    {
        private static AppServiceConfiguration _instance { get; set; }

        public string APIKey { get; set; }
        public string ServerURL { get; set; }

        public static AppServiceConfiguration Instance
        {
            get
            {
                if(_instance == null) {
                    _instance = new AppServiceConfiguration
                    {
                        APIKey = ConfigurationManager.AppSettings["APIKey"],
                        ServerURL = ConfigurationManager.AppSettings["ServerURL"]
                    };
                }

                return _instance;
            }
        }
    }
}
