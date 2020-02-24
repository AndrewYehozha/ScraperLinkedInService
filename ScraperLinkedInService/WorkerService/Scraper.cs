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
using Cookie = OpenQA.Selenium.Cookie;

namespace ScraperLinkedInService.WorkerService
{
    public class Scraper
    {
        //https://sites.google.com/a/chromium.org/chrome_driver/downloads

        private readonly AccountService _accountService;
        private readonly SettingService _settingService;
        private readonly DebugLogService _debugLogService;
        private readonly ProfileService _profileService;
        private readonly CompanyService _companyService;

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
            _profileService = new ProfileService();
            _companyService = new CompanyService();
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
                    _debugLogService.SendDebugLog(ex.ToString(), "Error initialize scraper");
                }
                return false;
            }
        }

        public bool LoginToLinkedIn()
        {
            try
            {
                _settingService.UpdateScraperStatus(ScraperStatus.ON);
                _debugLogService.SendDebugLog("", "Connecting to LinkedIn...");
                GoToUrl("https://www.linkedin.com");

                var cookie = new Cookie("li_at", _settingsVM.Token.Trim(), ".www.linkedin.com", "/", DateTime.Now.AddDays(1));
                _driver.Manage().Cookies.AddCookie(cookie);
                GoToUrl("https://www.linkedin.com");

                // Validation of the entered token
                Thread.Sleep(3000);
                var profileName = FindElementByClassName("profile-rail-card__actor-link", false, false);
                if (profileName != null && !string.IsNullOrEmpty(profileName.Text))
                {
                    _debugLogService.SendDebugLog(profileName.Text, "Connected successfully as");
                }
                else if (!SignIn() || CheckBrowserErrors() || !CheckLinkedInAuthorization())
                {
                    _debugLogService.SendDebugLog("", "Error login to LinkedIn");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _settingService.UpdateScraperStatus(ScraperStatus.Exception);
                _debugLogService.SendDebugLog(ex.ToString(), "Error login to LinkedIn");
                return false;
            }
        }

        public void RunScraperProcess() //Scraper process
        {
            _debugLogService.SendDebugLog("", "Start scraper process");
            var profilesInProcessCount = _profileService.GetCountProfilesInProcess();
            var newPofilesCount = _profileService.GetCountNewProfiles();

            if (profilesInProcessCount < _advanceSettingsVM.ProfileBatchSize)
            {
                while (newPofilesCount <= _advanceSettingsVM.ProfileBatchSize * 1.6)
                {
                    var companiesForSearch = _companyService.GetCompaniesForSearch(_advanceSettingsVM.CompanyBatchSize);
                    if (!companiesForSearch.CompaniesViewModel.Any() || GetCompaniesEmployees(companiesForSearch.CompaniesViewModel))
                    {
                        break;
                    }

                    if (!_configuration.IsAuthorized)
                        return;

                    newPofilesCount = _profileService.GetCountNewProfiles();
                }
            }
            else
            {
                _advanceSettingsVM.ProfileBatchSize *= 2;
            }

            if (!_configuration.IsAuthorized)
                return;

            var rolesSearch = _settingsVM.RolesSearch.Split(',');
            var technologiesSearch = _settingsVM.TechnologiesSearch.Split(',');
            var profilesForSearch = _profileService.GetProfilesForSearch(_advanceSettingsVM.ProfileBatchSize);
            if (profilesForSearch?.ProfilesViewModel != null && profilesForSearch.ProfilesViewModel.Any())
            {
                GetEmployeesProfiles(profilesForSearch.ProfilesViewModel, rolesSearch, technologiesSearch);
            }

            _debugLogService.SendDebugLog("", "End scraper process");
            var nextRunDate = _advanceSettingsVM.IntervalType == IntervalType.Day ? DateTime.UtcNow.AddDays(_advanceSettingsVM.IntervalValue).Date
                            : _advanceSettingsVM.IntervalType == IntervalType.Hour ? DateTime.UtcNow.AddHours(_advanceSettingsVM.IntervalValue).Date
                            : DateTime.UtcNow.Date;
            _debugLogService.SendDebugLog($"{ (nextRunDate + _advanceSettingsVM.TimeStart.Add(new TimeSpan(0, 1, 30))) } (UTC)", "Next Scraper run");
            _settingService.UpdateScraperStatus(ScraperStatus.OFF);
        }

        private bool GetCompaniesEmployees(IEnumerable<CompanyViewModel> companies)
        {
            _debugLogService.SendDebugLog($"[ { string.Join(", ", companies.Select(x => $"{ x.LinkedInURL }")) } ]", "Companies URLs to scrape");
            var isError = true;

            foreach (var company in companies)
            {
                Thread.Sleep(30000); //A break between requests.

                _debugLogService.SendDebugLog(company.LinkedInURL, "Getting employees for company with url");
                GoToUrl(company.LinkedInURL, $"Error opening company page with url: { company.LinkedInURL }");

                if (CheckBrowserErrors() || !CheckLinkedInAuthorization() || !_configuration.IsAuthorized)
                {
                    return isError;
                }

                var isPrivateCompanyProfileElement = FindElementByClassName("nav-header__guest-nav", false, true, company.LinkedInURL, "Find and apply for a job to contact and learn more about this company");
                var isNotFoundCompanyElement = FindElementByClassName("not-found__main-heading", false, true, company.LinkedInURL, "Could not scrape company, because this page was not found");
                var isUnavailableCompanyProfileElement = FindElementByClassName("profile-unavailable", false, true, company.LinkedInURL, "Could not scrape company, because this profile is not available");
                //If incorrect url
                var isErrorElement = FindElementByClassName("error-container", false, true, company.LinkedInURL, "Could not scrape company, because the page could not be loaded");
                var generalInformationButton = FindElementByCssSelector("a[data-control-name='page_member_main_nav_about_tab']", false, company.LinkedInURL, "Error: \"This isn't a company\"");
                
                if (isPrivateCompanyProfileElement != null || isNotFoundCompanyElement != null
                    || isUnavailableCompanyProfileElement != null || isErrorElement != null
                    || generalInformationButton == null)
                {
                    company.ExecutionStatus = ExecutionStatus.Failed;
                    _companyService.UpdateCompany(company);
                    continue;
                }

                var logoCompanyUrlAttr = FindElementByClassName("org-top-card-primary-content__logo", false, false)?.GetAttribute("src");
                if (!string.IsNullOrEmpty(logoCompanyUrlAttr))
                {
                    company.LogoUrl = logoCompanyUrlAttr.Contains("https://media-exp1.licdn.com") ? logoCompanyUrlAttr : string.Empty;
                }

                if (string.IsNullOrEmpty(company.OrganizationName))
                {
                    var organizationName = FindElementByCssSelector("h1.org-top-card-summary__title > span", false)?.Text;
                    company.OrganizationName = organizationName ?? string.Empty;
                }

                generalInformationButton.Click();

                if (string.IsNullOrEmpty(company.Website))
                {
                    var organizationWebsite = FindElementByCssSelector(".overflow-hidden > dd > a .link-without-visited-state", false)?.Text;
                    company.Website = organizationWebsite ?? string.Empty;
                    if (string.IsNullOrEmpty(company.Website))
                    {
                        company.ExecutionStatus = ExecutionStatus.Failed;
                        _companyService.UpdateCompany(company);
                        continue;
                    }
                }

                var specialtyCompanyTitleText = FindElementByCssSelector(".overflow-hidden dt:last-of-type", false)?.Text;
                if (specialtyCompanyTitleText != null && (new List<string> { "Специализация", "Specialties" }).Contains(specialtyCompanyTitleText))
                {
                    company.Specialties = FindElementByCssSelector(".overflow-hidden dd:last-of-type", false)?.Text ?? string.Empty;
                }

                var showAllEmployeesLink = FindElementByCssSelector("div.mt2 > .link-without-visited-state", false, company.LinkedInURL, "Error: Could not scrape company because No employees found");
                if (showAllEmployeesLink != null)
                {
                    showAllEmployeesLink.Click();
                    Thread.Sleep(2000); // Waiting for page to load

                    var paginationUrl = _driver.Url;
                    GoToUrl($"{ paginationUrl }&page=1");

                    var notFoundPageElement = FindElementByClassName("not-found", false, true, company.LinkedInURL, "This page not found");
                    if(notFoundPageElement != null)
                    {
                        continue;
                    }

                    var profiles = new List<ProfileViewModel>();
                    var startPage = company.LastScrapedPage > 0 ? company.LastScrapedPage : 1;
                    for (int i = startPage; ; i++)
                    {
                        GoToUrl($"{ paginationUrl }&page={ i }"); //Pagination Employees
                        if (CheckBrowserErrors() || !CheckLinkedInAuthorization() || !_configuration.IsAuthorized)
                        {
                            _companyService.UpdateCompany(company);
                            return isError;
                        }

                        js.ExecuteScript("window.scrollBy(0,1000)");
                        Thread.Sleep(2000); // Waiting for page to load

                        // Stop searching if next page is missing
                        var nextPageNotExistElement = FindElementByClassName("search-no-results__container", false, false, company.LinkedInURL, "Scrap All pages for company");
                        if(nextPageNotExistElement != null)
                        {
                            break;
                        }

                        js.ExecuteScript("window.scrollBy(0,1000)");
                        Thread.Sleep(1000); // Waiting for page to load

                        var employeesLinks = FindElementsByCssSelector(".search-result__info > a", false, $"Getting employees for page { i }...", "Error");
                        if(employeesLinks != null && employeesLinks.Any())
                        {
                            foreach (var employeeLink in employeesLinks) // Link selection for employees
                            {
                                string href = employeeLink.GetAttribute("href");
                                if (href != $"{ paginationUrl }&page={ i }#")
                                {
                                    profiles.Add(new ProfileViewModel { ProfileUrl = href, CompanyID = company.Id, AccountID = company.AccountId });
                                }
                            }

                            _debugLogService.SendDebugLog("", $"Getting employees for page { i }...");
                        }

                        if (profiles.Any() && profiles.Count >= 100)
                        {
                            _profileService.InsertProfiles(profiles);
                            _companyService.UpdateLastPageCompany(company.Id, i);
                            profiles = default;
                        }
                    }

                    _profileService.InsertProfiles(profiles);
                }

                company.ExecutionStatus = ExecutionStatus.Success;
                _companyService.UpdateCompany(company);
            }

            return !isError;
        }

        private void GetEmployeesProfiles(IEnumerable<ProfileViewModel> employees, IEnumerable<string> rolesSearch, IEnumerable<string> technologiesSearch)
        {
            _debugLogService.SendDebugLog($"[ { string.Join(", ", employees.Select(x => $"{ x.ProfileUrl }")) } ]", "Profiles URLs to scrape");
            var profilesReady = new List<ProfileViewModel>();

            foreach (var employee in employees)
            {
                Thread.Sleep(10000); //A break between requests.

                _debugLogService.SendDebugLog(employee.ProfileUrl, "Opening profile");
                GoToUrl(employee.ProfileUrl, "", $"Error opening profile page with url: { employee.ProfileUrl }");

                Thread.Sleep(3000);

                if (CheckBrowserErrors() || !CheckLinkedInAuthorization() || !_configuration.IsAuthorized)
                {
                    return;
                }

                // Stop searching if incorrect url
                var unavailableProfileElement = FindElementByClassName("profile-unavailable", false, true, employee.ProfileUrl, "Could not scrape profile, because this profile is not available");
                if (unavailableProfileElement != null)
                {
                    employee.ExecutionStatus = ExecutionStatus.Failed;
                    profilesReady.Add(employee);
                    continue;
                }

                _debugLogService.SendDebugLog("", "Scrolling to load all data of the profile...");

                var fullNameElement = FindElementByCssSelector(".pv-top-card-v3--list .break-words", false);
                if (fullNameElement != null)
                {
                    employee.FullName = fullNameElement.Text;
                    employee.FirstName = employee.FullName.Split(' ')[0];
                    employee.LastName = employee.FullName.Split(' ')[1];
                }

                var lastJobsElements = FindElementsByCssSelector(".pv-top-card-v3--experience-list-item > span", false);
                if (lastJobsElements == null && !lastJobsElements.Any(x => x.Text.ToUpper().Contains(employee.OrganizationName.ToUpper())))
                {
                    employee.ProfileStatus = ProfileStatus.Unsuited;
                }

                var jobElement = FindElementByCssSelector(".flex-1 h2", false);
                employee.Job = jobElement != null ? jobElement.Text : string.Empty;
                employee.AllSkills = string.Empty;

                for (int i = 0; i < 6; i++)
                {
                    js.ExecuteScript("window.scrollBy(0,750)");

                    if (employee.ProfileStatus == ProfileStatus.Unsuited)
                    {
                        var lastWorkElementH3 = FindElementByCssSelector(".pv-profile-section__list-item .pv-entity__company-summary-info h3, .pv-profile-section__list-item .pv-entity__summary-info h3", false);
                        var lastWorkH3 = lastWorkElementH3 != null ? lastWorkElementH3.Text : string.Empty;
                        var lastWorkElementP = FindElementByCssSelector(".pv-profile-section__list-item .pv-entity__company-summary-info .pv-entity__secondary-title, .pv-profile-section__list-item .pv-entity__summary-info .pv-entity__secondary-title", false);
                        var lastWorkP = lastWorkElementP != null ? lastWorkElementP.Text : string.Empty;
                        if (lastWorkH3.ToUpper().Contains(employee.OrganizationName.ToUpper()) || lastWorkP.ToUpper().Contains(employee.OrganizationName.ToUpper()))
                        {
                            employee.ProfileStatus = ProfileStatus.Undefined;
                        }
                    }

                    var skillsBlock = FindElementByClassName("pv-skill-category-entity__name-text", false, false);
                    if (skillsBlock == null)
                    {
                        continue;
                    }

                    js.ExecuteScript("window.scrollBy(0,300)");
                    Thread.Sleep(500);
                    //Find Show more button
                    var loadSkillsButton = FindElementByClassName("pv-skills-section__additional-skills", false, false);
                    if (loadSkillsButton != null)
                    {
                        loadSkillsButton.Click();
                        js.ExecuteScript("window.scrollBy(0,250)");
                    }

                    Thread.Sleep(500); // Waiting for loading block skills

                    var arrSkills = new List<string>();
                    var skillsNamesElements = FindElementsByClassName("pv-skill-category-entity__name-text", false);
                    if (skillsNamesElements != null)
                    {
                        foreach (var skillNameElement in skillsNamesElements)
                        {
                            arrSkills.Add(skillNameElement.Text);
                        }

                        employee.AllSkills = string.Join(",", arrSkills);
                    }
                    else
                    {
                        employee.AllSkills = string.Empty;
                    }

                    break;
                }

                if (!string.IsNullOrEmpty(employee.AllSkills) || !string.IsNullOrEmpty(employee.Job))
                {
                    _debugLogService.SendDebugLog("", "All data loaded");
                }
                else
                {
                    _debugLogService.SendDebugLog(employee.ProfileUrl, "Could not scrape because: Could not load the profile");
                    employee.ExecutionStatus = ExecutionStatus.Failed;
                }

                profilesReady.Add(employee);
                if (profilesReady.Count >= 50)
                {
                    _profileService.UpdateProfiles(profilesReady, rolesSearch, technologiesSearch);
                    profilesReady = default;
                }
            }

            _profileService.UpdateProfiles(profilesReady, rolesSearch, technologiesSearch);
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
                _debugLogService.SendDebugLog(errorElement.Text, "Browser Error");
            }

            return errorElement != null;
        }

        private bool CheckLinkedInAuthorization()
        {
            var errorElement = FindElementByCssSelector(".login__form_action_container,.join-form-container,.login-form,body > #FunCAPTCHA", false, "Invalid Token", "Error");
            if (errorElement != null)
                return LoginToLinkedIn();

            return true;
        }

        private bool SignIn()
        {
            GoToUrl("https://www.linkedin.com/login?fromSignIn=true&trk=guest_homepage-basic_nav-header-signin");
            Thread.Sleep(3000); //Loading Sign in page

            var userNameElement = FindElementById("username", true, remarks: "Error authorization");
            var passwordElement = FindElementById("password", true, remarks: "Error authorization");
            var buttonConfirm = FindElementByClassName("btn__primary--large", true, false, remarks: "Error authorization");

            if (userNameElement == null || passwordElement == null || buttonConfirm == null)
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

            var profileName = FindElementByClassName("profile-rail-card__actor-link", false, false)?.Text;
            if (profileName != null)
            {
                _debugLogService.SendDebugLog(profileName, "Connected successfully as");
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

        private IWebElement FindElementByClassName(string className, bool isEnableException, bool sendDebugLogIfFind, string logs = "", string remarks = "")
        {
            try
            {
                var element = _driver.FindElement(By.ClassName(className));

                if(sendDebugLogIfFind && (!string.IsNullOrEmpty(logs) || !string.IsNullOrEmpty(remarks)))
                {
                    debugLogVMs.Add(_debugLogService.GenerateViewModel(logs, remarks));
                }

                return element;
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
