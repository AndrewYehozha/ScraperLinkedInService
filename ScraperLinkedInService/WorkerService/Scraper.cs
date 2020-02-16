﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        //https://developer.microsoft.com/en-us/microsoft-edge/tools/web_driver/

        private readonly AccountService _accountService;
        private readonly SettingService _settingService;

        private IWebDriver _driver;
        private IJavaScriptExecutor js;

        private AdvanceSettingsViewModel _advanceSettingsVM;
        private SettingsViewModel _settingsVM;
        private string _APIKey;

        public Scraper()
        {
            _accountService = new AccountService();
            _settingService = new SettingService();
        }

        public bool Initialize(string APIKey)
        {
            try
            {
                var options = new ChromeOptions();
                options.AddArgument("no-sandbox");
                _driver = new ChromeDriver(options);
                js = (IJavaScriptExecutor)_driver;

                _driver.Manage().Window.Maximize();
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
                _driver.Navigate().GoToUrl("https://www.linkedin.com");

                _advanceSettingsVM = _settingService.GetAdvanceSettings(_APIKey).AdvanceSettingsViewModel; //if statuscode = 401 => authorized, if statuscode = 401 => return false;
                _settingsVM = _settingService.GetSettings(_APIKey).SettingsViewModel;
                _APIKey = APIKey;

                return true;
            }
            catch (Exception ex)
            {
                _settingService.UpdateScraperStatus(_APIKey, ScraperStatus.Exception).Wait();
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
