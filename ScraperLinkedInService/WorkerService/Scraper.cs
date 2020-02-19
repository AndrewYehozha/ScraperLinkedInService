using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ScraperLinkedInService.Models.Entities;
using ScraperLinkedInService.Models.Types;
using ScraperLinkedInService.Services;
using Cookie = OpenQA.Selenium.Cookie;

namespace ScraperLinkedInService.WorkerService
{
    public class Scraper
    {
        //https://sites.google.com/a/chromium.org/chrome_driver/downloads

        private readonly AccountService _accountService;
        private readonly SettingService _settingService;
        private readonly DebugLogService _debugLogService;

        private IWebDriver _driver;
        private IJavaScriptExecutor js;

        private AdvanceSettingsViewModel _advanceSettingsVM;
        private SettingsViewModel _settingsVM;
        private AppServiceConfiguration _configuration;
        private List<DebugLogViewModel> debugLogVMs;

        public Scraper()
        {
            _accountService = new AccountService();
            _settingService = new SettingService();
            _debugLogService = new DebugLogService();
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
                debugLogVMs = new List<DebugLogViewModel>();

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
                if (_configuration.IsAuthorized)
                {
                    _settingService.UpdateScraperStatus(ScraperStatus.Exception);
                    _debugLogService.SendDebugLogAsync(ex.ToString(), "Error initialize scraper");
                }
                return false;
            }
        }

        public bool LoginToLinkedIn()
        {
            try
            {
                _settingService.UpdateScraperStatus(ScraperStatus.ON);
                _debugLogService.SendDebugLogAsync("", "Connecting to LinkedIn...");
                GoToUrl("https://www.linkedin.com");

                var cookie = new Cookie("li_at", _settingsVM.Token.Trim(), ".www.linkedin.com", "/", DateTime.Now.AddDays(1));
                _driver.Manage().Cookies.AddCookie(cookie);
                GoToUrl("https://www.linkedin.com");

                // Validation of the entered token
                Thread.Sleep(3000);
                var profileName = FindElementByClassName("profile-rail-card__actor-link", false);
                if (profileName != null && !string.IsNullOrEmpty(profileName.Text))
                {
                    _debugLogService.SendDebugLogAsync(profileName.Text, "Connected successfully as");
                }
                else if (!SignIn() || CheckBrowserErrors() || !CheckAuthorization())
                {
                    _debugLogService.SendDebugLogAsync("", "Error login to LinkedIn");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _settingService.UpdateScraperStatus(ScraperStatus.Exception);
                _debugLogService.SendDebugLogAsync(ex.ToString(), "Error login to LinkedIn");
                return false;
            }
        }

        public void RunScraperProcess() //Scraper process
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

        private bool CheckBrowserErrors()
        {
            var errorElement = FindElementByCssSelector("div .error-code", false);
            if (errorElement != null)
            {
                _debugLogService.SendDebugLogAsync(errorElement.Text, "Browser Error");
            }

            return errorElement != null;
        }

        private bool CheckAuthorization()
        {
            var errorElement = FindElementByCssSelector(".login__form_action_container,.join-form-container,.login-form,body > #FunCAPTCHA", false, "Invalid Token", "Error");
            return errorElement != null;
        }

        private bool SignIn()
        {
            GoToUrl("https://www.linkedin.com/login?fromSignIn=true&trk=guest_homepage-basic_nav-header-signin");
            Thread.Sleep(3000); //Loading Sign in page

            var userNameElement = FindElementById("username", true, remarks: "Error authorization");
            var passwordElement = FindElementById("password", true, remarks: "Error authorization");
            var buttonConfirm = FindElementByClassName("btn__primary--large", true, remarks: "Error authorization");

            if (userNameElement == null && passwordElement == null && buttonConfirm == null)
            {
                return false;
            }

            //Authorization process
            userNameElement.Clear();
            userNameElement.SendKeys(_settingsVM.Login);
            Thread.Sleep(500);
            passwordElement.Clear();
            passwordElement.SendKeys(_settingsVM.Password);
            Thread.Sleep(500);
            buttonConfirm.Click();

            //Wait and recheck authorization
            Thread.Sleep(5000);
            FindElementByTagName("body", false).SendKeys("Keys.ESCAPE");
            userNameElement = FindElementById("username", false);
            if (userNameElement != null)
            {
                return false;
            }

            //Check captcha
            FindElementByCssSelector(".ember-view > .password-prompt-wrapper", false)?.Submit();
            Thread.Sleep(5000);

            var profileName = FindElementByClassName("profile-rail-card__actor-link", false)?.Text;
            if (profileName != null)
            {
                _debugLogService.SendDebugLogAsync(profileName, "Connected successfully as");
            }

            return true;
        }

        private void GoToUrl(string url, string logs = "", string remarks = "")
        {
            try
            {
                _driver.Navigate().GoToUrl(url);
            }
            catch
            {
                _driver.FindElement(By.TagName("body")).SendKeys("Keys.ESCAPE");
                if (!string.IsNullOrEmpty(logs) || !string.IsNullOrEmpty(remarks))
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(logs, remarks));
                }
            }
        }

        private IWebElement FindElementByTagName(string tagName, bool isEnableException, string logs = "", string remarks = "")
        {
            try
            {
                return _driver.FindElement(By.TagName(tagName));
            }
            catch (Exception ex)
            {
                if (!isEnableException && (!string.IsNullOrEmpty(logs) || !string.IsNullOrEmpty(remarks)))
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(logs, remarks));
                }
                else if (isEnableException)
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(ex.ToString(), remarks));
                }

                return null;
            }
        }

        private IWebElement FindElementById(string id, bool isEnableException, string logs = "", string remarks = "")
        {
            try
            {
                return _driver.FindElement(By.Id(id));
            }
            catch (Exception ex)
            {
                if (!isEnableException && (!string.IsNullOrEmpty(logs) || !string.IsNullOrEmpty(remarks)))
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(logs, remarks));
                }
                else if (isEnableException)
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(ex.ToString(), remarks));
                }

                return null;
            }
        }

        private IWebElement FindElementByClassName(string className, bool isEnableException, string logs = "", string remarks = "")
        {
            try
            {
                return _driver.FindElement(By.ClassName(className));
            }
            catch (Exception ex)
            {
                if (!isEnableException && (!string.IsNullOrEmpty(logs) || !string.IsNullOrEmpty(remarks)))
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(logs, remarks));
                }
                else if (isEnableException)
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(ex.ToString(), remarks));
                }

                return null;
            }
        }

        private IWebElement FindElementByCssSelector(string cssSelector, bool isEnableException, string logs = "", string remarks = "")
        {
            try
            {
                return _driver.FindElement(By.CssSelector(cssSelector));
            }
            catch (Exception ex)
            {
                if (!isEnableException && (!string.IsNullOrEmpty(logs) || !string.IsNullOrEmpty(remarks)))
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(logs, remarks));
                }
                else if (isEnableException)
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(ex.ToString(), remarks));
                }

                return null;
            }
        }

        private IEnumerable<IWebElement> FindElementsByClassName(string className, bool isEnableException, string logs = "", string remarks = "")
        {
            try
            {
                return _driver.FindElements(By.ClassName(className));
            }
            catch (Exception ex)
            {
                if (!isEnableException && (!string.IsNullOrEmpty(logs) || !string.IsNullOrEmpty(remarks)))
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(logs, remarks));
                }
                else if (isEnableException)
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(ex.ToString(), remarks));
                }

                return null;
            }
        }

        private IEnumerable<IWebElement> FindElementsByCssSelector(string cssSelector, bool isEnableException, string logs = "", string remarks = "")
        {
            try
            {
                return _driver.FindElements(By.CssSelector(cssSelector));
            }
            catch (Exception ex)
            {
                if (!isEnableException && (!string.IsNullOrEmpty(logs) || !string.IsNullOrEmpty(remarks)))
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(logs, remarks));
                }
                else if (isEnableException)
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(ex.ToString(), remarks));
                }

                return null;
            }
        }
    }
}
