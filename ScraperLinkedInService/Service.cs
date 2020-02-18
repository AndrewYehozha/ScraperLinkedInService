using ScraperLinkedInService.Models.Types;
using ScraperLinkedInService.Scheduler;
using ScraperLinkedInService.Services;
using ScraperLinkedInService.WorkerService;
using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ScraperLinkedInService
{
    public partial class Service : ServiceBase
    {
        private readonly Scraper _scraper;
        private readonly AccountService _accountService;
        private readonly SettingService _settingService;
        private readonly DebugLogService _debugLogService;
        private AppServiceConfiguration _configuration;

        public Service()
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

            if (_configuration.IsAuthorized)
            {
                Task.Run(() => _debugLogService.SendDebugLogAsync(_debugLogService.GenerateViewModel("", "Scheduler service are starting...")));
                
                var advanceSettingsResponse = _settingService.GetAdvanceSettings();
                var intervalType = advanceSettingsResponse.AdvanceSettingsViewModel.IntervalType;
                var timeStart = advanceSettingsResponse.AdvanceSettingsViewModel.TimeStart;
                var intervalValue = advanceSettingsResponse.AdvanceSettingsViewModel.IntervalValue;

                switch (intervalType)
                {
                    case IntervalType.Hour:
                        Task.Run(() => _debugLogService.SendDebugLogAsync(_debugLogService.GenerateViewModel("With an interval in Hours", "Start a schedule")));

                        MyScheduler.IntervalInHours(timeStart.Hours, timeStart.Minutes, intervalValue,
                        () =>
                        {
                            RunScraper();
                        });
                        break;

                    case IntervalType.Day:
                        Task.Run(() => _debugLogService.SendDebugLogAsync(_debugLogService.GenerateViewModel("With an interval in Days", "Start a schedule")));

                        MyScheduler.IntervalInDays(timeStart.Hours, timeStart.Minutes, intervalValue,
                        () =>
                        {
                            RunScraper();
                        });
                        break;

                    default:
                        Task.Run(() => _debugLogService.SendDebugLogAsync(_debugLogService.GenerateViewModel("Invalid IntervalType.Please, check the value of < INTERVAL_TYPE > in App.config.", "Error INTERVAL_TYPE")));
                        break;
                }
            }
        }

        protected override void OnShutdown()
        {
            Task.Run(() => _debugLogService.SendDebugLogAsync(_debugLogService.GenerateViewModel("", "Scheduler service is stoping...")));

            _scraper.Close();

            Task.Run(() => _debugLogService.SendDebugLogAsync(_debugLogService.GenerateViewModel("System shutdown", "Scheduler service stopped")));

            if (_configuration.IsAuthorized)
            {
                _settingService.UpdateScraperStatus(ScraperStatus.Exception);
            }
        }

        protected override void OnStop()
        {
            Task.Run(() => _debugLogService.SendDebugLogAsync(_debugLogService.GenerateViewModel("", "Scheduler service is stoping...")));
            
            _scraper.Close();

            Task.Run(() => _debugLogService.SendDebugLogAsync(_debugLogService.GenerateViewModel("System shutdown", "Scheduler service stopped")));

            if (_configuration.IsAuthorized)
            {
                _settingService.UpdateScraperStatus(ScraperStatus.OFF);
            }
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
            }
            Thread.Sleep(90000);
            _scraper.Run();
            _scraper.Close();
        }
    }
}
