using ScraperLinkedInService.Models.Types;
using ScraperLinkedInService.Scheduler;
using ScraperLinkedInService.Services;
using ScraperLinkedInService.WorkerService;
using System;
using System.ServiceProcess;
using System.Threading;

namespace ScraperLinkedInService
{
    public partial class ScraperService : ServiceBase
    {
        private readonly Scraper _scraper;
        private readonly AccountService _accountService;
        private readonly SettingService _settingService;
        private readonly DebugLogService _debugLogService;
        private AppServiceConfiguration _configuration;

        public ScraperService()
        {
            _scraper = new Scraper();
            _accountService = new AccountService();
            _settingService = new SettingService();
            _debugLogService = new DebugLogService();
            _configuration = AppServiceConfiguration.Instance;
            InitializeComponent();
        }


        protected override void OnStart(string[] args)
        {
            _accountService.Authorization();

            RunScraper();
            if (_configuration.IsAuthorized)
            {
                _debugLogService.SendDebugLog("", "Scheduler service are starting...");

                var advanceSettingsResponse = _settingService.GetAdvanceSettings();
                var intervalType = advanceSettingsResponse.AdvanceSettingsViewModel.IntervalType;
                var timeStart = advanceSettingsResponse.AdvanceSettingsViewModel.TimeStart;
                var intervalValue = advanceSettingsResponse.AdvanceSettingsViewModel.IntervalValue;

                switch (intervalType)
                {
                    case IntervalType.Hour:
                        _debugLogService.SendDebugLog("With an interval in Hours", "Start a schedule");
                        MyScheduler.IntervalInHours(timeStart.Hours, timeStart.Minutes, intervalValue,
                        () =>
                        {
                            RunScraper();
                        });
                        break;

                    case IntervalType.Day:
                        _debugLogService.SendDebugLog("With an interval in Days", "Start a schedule");
                        MyScheduler.IntervalInDays(timeStart.Hours, timeStart.Minutes, intervalValue,
                        () =>
                        {
                            RunScraper();
                        });
                        break;

                    default:
                        _debugLogService.SendDebugLog("Invalid IntervalType.Please, check the value of < INTERVAL_TYPE > in App.config.", "Error INTERVAL_TYPE");
                        break;
                }
            }
        }

        protected override void OnShutdown()
        {
            _debugLogService.SendDebugLog("", "Scheduler service is stoping...");
            _scraper.Close();
            _debugLogService.SendDebugLog("System shutdown", "Scheduler service stopped");
            _settingService.UpdateScraperStatus(ScraperStatus.Exception);
        }

        protected override void OnStop()
        {
            _debugLogService.SendDebugLog("", "Scheduler service is stoping...");
            _scraper.Close();
            _debugLogService.SendDebugLog("System shutdown", "Scheduler service stopped");
            _settingService.UpdateScraperStatus(ScraperStatus.OFF);
        }

        public void RunAsConsole(string[] args)
        {
            OnStart(args);
            Console.ReadKey(true);
            OnStop();
        }

        private void RunScraper()
        {
            if (!_scraper.Initialize())
            {
                OnStop();
                _settingService.UpdateScraperStatus(ScraperStatus.Exception);
                return;
            }

            Thread.Sleep(90000);

            if (_scraper.LoginToLinkedIn())
            {
                _scraper.RunScraperProcess();
            }

            _scraper.Close();
        }
    }
}
