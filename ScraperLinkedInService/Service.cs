using ScraperLinkedInService.Models.Types;
using ScraperLinkedInService.Scheduler;
using ScraperLinkedInService.Services;
using ScraperLinkedInService.WorkerService;
using System.Net;
using System.ServiceProcess;
using System.Threading;

namespace ScraperLinkedInService
{
    public partial class Service : ServiceBase
    {
        private readonly Scraper _scraper;
        private readonly AccountService _accountService;
        private readonly SettingService _settingService;

        private bool _isAuthorized = false;
        private string _APIKey = string.Empty;

        public Service()
        {
            _scraper = new Scraper();
            _accountService = new AccountService();
            _settingService = new SettingService();
            InitializeComponent();
        }


        protected override void OnStart(string[] args)
        {
            var authorizationResponse = _accountService.Authorization();
            if (authorizationResponse != null && authorizationResponse.StatusCode == (int)HttpStatusCode.OK)
            {
                _isAuthorized = true;
                _APIKey = authorizationResponse.Token;
                //_loggerService.Add("Scheduler service are starting...", "");
                var advanceSettingsResponse = _settingService.GetAdvanceSettings(_APIKey);
                var settingsResponse = _settingService.GetSettings(_APIKey);

                var intervalType = advanceSettingsResponse.AdvanceSettingsViewModel.IntervalType;
                var timeStart = advanceSettingsResponse.AdvanceSettingsViewModel.TimeStart;
                var intervalValue = advanceSettingsResponse.AdvanceSettingsViewModel.IntervalValue;

                switch (intervalType)
                {
                    case IntervalType.Hour:

                        //_loggerService.Add("Start a schedule", "With an interval in hours");

                        MyScheduler.IntervalInHours(timeStart.Hours, timeStart.Minutes, intervalValue,
                        () =>
                        {
                            RunScraper();
                        });
                        break;

                    case IntervalType.Day:

                        //_loggerService.Add("Start a schedule", "With an interval in days");

                        MyScheduler.IntervalInDays(timeStart.Hours, timeStart.Minutes, intervalValue,
                        () =>
                        {
                            RunScraper();
                        });
                        break;

                    default:
                        //_loggerService.Add("Error INTERVAL_TYPE", "Invalid IntervalType.Please, check the value of < INTERVAL_TYPE > in App.config.");
                        break;
                }
            }
        }

        protected override void OnShutdown()
        {
            //_loggerService.Add("Scheduler service is stoping...", "");
            _scraper.Close();
            //_loggerService.Add("Scheduler service stopped", "System shutdown");

            if (!string.IsNullOrEmpty(_APIKey))
            {
                _settingService.UpdateScraperStatus(_APIKey, ScraperStatus.Exception).Wait();
            }
        }

        protected override void OnStop()
        {
            //_loggerService.Add("Scheduler service is stoping...", "");
            _scraper.Close();
            //_loggerService.Add("Scheduler service stopped", "");

            if (!string.IsNullOrEmpty(_APIKey) && _isAuthorized)
            {
                _settingService.UpdateScraperStatus(_APIKey, ScraperStatus.OFF).Wait();
            }
        }

        public void RunAsConsole(string[] args)
        {
            OnStart(args);
        }

        private void RunScraper()
        {
            if (!_scraper.Initialize(_APIKey))
            {
                OnStop();
                _settingService.UpdateScraperStatus(_APIKey, ScraperStatus.Exception).Wait();
            }
            Thread.Sleep(90000);
            _scraper.Run();
            _scraper.Close();
        }
    }
}
