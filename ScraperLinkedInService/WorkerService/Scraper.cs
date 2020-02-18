using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ScraperLinkedInService.Models.Entities;
using ScraperLinkedInService.Models.Types;
using ScraperLinkedInService.Services;

namespace ScraperLinkedInService.WorkerService
{
    public class Scraper
    {
        //https://sites.google.com/a/chromium.org/chrome_driver/downloads

        private readonly AccountService _accountService;
        private readonly SettingService _settingService;

        private IWebDriver _driver;
        private IJavaScriptExecutor js;

        private AdvanceSettingsViewModel _advanceSettingsVM;
        private SettingsViewModel _settingsVM;
        private AppServiceConfiguration _configuration;

        public Scraper()
        {
            _accountService = new AccountService();
            _settingService = new SettingService();
            _configuration = AppServiceConfiguration.Instance;
        }

        public bool Initialize()
        {
            try
            {
                if (!_configuration.IsAuthorized)
                {
                    _accountService.Authorization();

                    if (!_configuration.IsAuthorized)
                        return false;
                }

                var advanceSettingsResponse = _settingService.GetAdvanceSettings();
                var settingsResponse = _settingService.GetSettings();

                if (advanceSettingsResponse.StatusCode != (int)HttpStatusCode.OK
                    || settingsResponse.StatusCode != (int)HttpStatusCode.OK)
                {
                    return false;
                }

                _advanceSettingsVM = advanceSettingsResponse.AdvanceSettingsViewModel;
                _settingsVM = settingsResponse.SettingsViewModel;

                var options = new ChromeOptions();
                options.AddArgument("no-sandbox");
                _driver = new ChromeDriver(options);
                js = (IJavaScriptExecutor)_driver;

                _driver.Manage().Window.Maximize();
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
                _driver.Navigate().GoToUrl("https://www.linkedin.com");


                return true;
            }
            catch (Exception ex)
            {
                _settingService.UpdateScraperStatus(ScraperStatus.Exception);
                //_loggerService.Add("Error initialize scraper", ex.ToString());

                return false;
            }
        }

        public void Run()
        {

        }

        public void Close()
        {
            try
            {
                _driver?.Close();

                var chrome_drivers = Process.GetProcessesByName("chrome_driver");
                foreach (var chrome_driver in chrome_drivers)
                {
                    chrome_driver.Kill();
                }
            }
            catch { }
        }
    }
}
