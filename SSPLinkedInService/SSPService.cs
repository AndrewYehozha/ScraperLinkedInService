using SSPLinkedInService.Models.Types;
using SSPLinkedInService.Services;
using SSPLinkedInService.WorkerService;
using System;
using System.ServiceProcess;

namespace SSPLinkedInService
{
    public partial class SSPService : ServiceBase
    {
        private readonly SearchSuitableProfiles _searchSuitableProfiles;
        private readonly AccountService _accountService;
        private readonly SettingService _settingService;
        private readonly DebugLogService _debugLogService;
        private AppServiceConfiguration _configuration;

        public SSPService()
        {
            InitializeComponent();
            _searchSuitableProfiles = new SearchSuitableProfiles();
            _accountService = new AccountService();
            _settingService = new SettingService();
            _debugLogService = new DebugLogService();
            _configuration = AppServiceConfiguration.Instance;
        }

        protected override void OnStart(string[] args)
        {
            _accountService.Authorization();

            if (_configuration.IsAuthorized)
            {
                _debugLogService.SendDebugLog("", "SSPService starting...");
                _searchSuitableProfiles.Run();
            }
        }

        protected override void OnShutdown()
        {
            _debugLogService.SendDebugLog("", "Scheduler service is stoping...");
            _settingService.UpdateScraperStatus(ScraperStatus.Exception);
            _debugLogService.SendDebugLog("System shutdown", "Scheduler service stopped");
        }

        protected override void OnStop()
        {
            _debugLogService.SendDebugLog("", "Scheduler service is stoping...");
            _settingService.UpdateScraperStatus(ScraperStatus.OFF);
            _debugLogService.SendDebugLog("", "Scheduler service stopped");
        }

        public void RunAsConsole(string[] args)
        {
            OnStart(args);
            Console.ReadKey(true);
            OnStop();
        }
    }
}
