using SSPLinkedInService.Models.Entities;
using SSPLinkedInService.Models.Types;
using SSPLinkedInService.Services;
using SSPLinkedInService.Services.EmailValidatorService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SSPLinkedInService.WorkerService
{
    public class SearchSuitableProfiles
    {
        private readonly AccountService _accountService;
        private readonly CompanyService _companyService;
        private readonly SettingService _settingService;
        private readonly DebugLogService _debugLogService;
        private readonly ProfileService _profileService;
        private readonly SuitableProfileService _suitableProfileService;
        private readonly EmailHandler _emailHandler;
        private AppServiceConfiguration _configuration;

        public SearchSuitableProfiles()
        {
            _accountService = new AccountService();
            _companyService = new CompanyService();
            _settingService = new SettingService();
            _debugLogService = new DebugLogService();
            _profileService = new ProfileService();
            _suitableProfileService = new SuitableProfileService();
            _emailHandler = new EmailHandler();
            _configuration = AppServiceConfiguration.Instance;
        }

        public void Run()
        {
            while (true)
            {
                var activeAccountsIds = _accountService.GetActiveAccountsIds()?.Ids;
                foreach (var activeAccountId in activeAccountsIds)
                {
                    var companiesForSearch = _companyService.GetCompaniesForSearch(activeAccountId, _configuration.SearchProfilesBatchSize);
                    if (companiesForSearch.CompanyProfilesViewModel != null && companiesForSearch.CompanyProfilesViewModel.Any())
                    {
                        SearchSuitableProfilesCompanies(activeAccountId, companiesForSearch.CompanyProfilesViewModel);
                        if (!_configuration.IsAuthorized)
                        {
                            break;
                        }
                    }
                }

                var timeOut = new TimeSpan(0, 30, 0);
                _debugLogService.SendDebugLog($"The next launch in { timeOut.TotalMinutes } minutes", "Pause");
                Thread.Sleep(timeOut);
            }
        }

        public void SearchSuitableProfilesCompanies(int accountId, IEnumerable<CompanyProfilesViewModel> companiesEmployees)
        {
            _debugLogService.SendDebugLog($"AccountID = { accountId }, Company ids: [ { string.Join(", ", companiesEmployees.Select(x => $"{ x.Id }")) } ]", "Start SearchSuitableProfilesCompanies");
            var settings = _settingService.GetSettingsByAccountId(accountId);
            var rolesSearch = settings?.SettingsViewModel?.RolesSearch?.Split(',') ?? new string[] { };

            foreach (var companyEmployees in companiesEmployees)
            {
                var result = new List<SuitableProfileViewModel>();
                var technologiesStack = new List<string>();
                var location = !string.IsNullOrEmpty(companyEmployees.HeadquartersLocation) ? companyEmployees.HeadquartersLocation.Split(',') : new string[] { };
                var profileSkills = companyEmployees.ProfilesViewModel.Where(x => x.ProfileStatus == ProfileStatus.Developer).Select(z => z.AllSkills.Split(','));

                foreach (var skills in profileSkills)
                {
                    foreach (var skill in skills)
                    {
                        if (!technologiesStack.Contains(skill.Trim()))
                        {
                            technologiesStack.Add(skill.Trim());
                        }
                    }
                }

                if (profileSkills == null || profileSkills.Count() == 0)
                {
                    _profileService.UpdateProfilesExecutionStatusByCompanyID(accountId, ExecutionStatus.Success, companyEmployees.Id);
                    continue;
                }

                if (!_configuration.IsAuthorized)
                {
                    _accountService.Authorization();
                    if (!_configuration.IsAuthorized)
                    {
                        break;
                    }
                }

                if (settings.SettingsViewModel.IsSearchChiefs)
                {
                    foreach (var employee in companyEmployees.ProfilesViewModel.Where(x => x.ProfileStatus == ProfileStatus.Chief))
                    {
                        var emails = _emailHandler.GetValidEmails(employee.FirstName, employee.LastName, companyEmployees.Website);
                        result.Add(GenerateResultViewModel(accountId, employee, companyEmployees, rolesSearch, technologiesStack, emails, location));
                    }


                    foreach (var fullName in companyEmployees.Founders?.Split(',') ?? new string[0])
                    {
                        if (string.IsNullOrEmpty(fullName.Trim()))
                            continue;

                        var founderProfile = companyEmployees.ProfilesViewModel.Where(x => x.FullName == fullName.Trim() && x.ProfileStatus == ProfileStatus.Chief).FirstOrDefault();
                        if (founderProfile == null)
                        {
                            var firstName = fullName.Trim().Split(' ')[0];
                            var lastName = fullName.Trim().Split(' ')[1];
                            var emails = _emailHandler.GetValidEmails(firstName, lastName, companyEmployees.Website);
                            result.Add(GenerateResultViewModel(accountId, null, companyEmployees, rolesSearch, technologiesStack, emails, location, firstName, lastName, "Founder", ""));
                        }
                    }
                }

                if (settings.SettingsViewModel.IsSearchDevelopers)
                {
                    foreach (var employee in companyEmployees.ProfilesViewModel.Where(x => x.ProfileStatus == ProfileStatus.Developer))
                    {
                        var emails = _emailHandler.GetValidEmails(employee.FirstName, employee.LastName, companyEmployees.Website);
                        result.Add(GenerateResultViewModel(accountId, employee, companyEmployees, rolesSearch, employee.AllSkills.Split(','), emails, location));
                    }
                }

                //After company scraped company data processing
                _suitableProfileService.InsertSuitableProfile(result);
                _profileService.UpdateProfilesExecutionStatusByCompanyID(accountId, ExecutionStatus.Success, companyEmployees.Id);
                _debugLogService.SendDebugLog($"AccountID = { accountId }, Suitable profiles ids: [ { string.Join(", ", result.Select(x => $"{ x.Id }")) } ]", "End SearchSuitableProfilesCompanies");
            }
        }

        private SuitableProfileViewModel GenerateResultViewModel(int accountId, ProfileViewModel employee, CompanyProfilesViewModel companyEmployees, string[] rolesSearch, IEnumerable<string> technologiesStack,
                                                                    List<string> emails, string[] location, string firstName = null, string lastName = null, string job = null, string personLinkedIn = null)
        {
            return new SuitableProfileViewModel
            {
                FirstName = firstName != null ? firstName : employee.FirstName,
                LastName = lastName != null ? lastName: employee.LastName,
                Job = employee.Job,
                PersonLinkedIn = personLinkedIn != null ? personLinkedIn : employee.ProfileUrl,
                Company = companyEmployees.OrganizationName,
                Website = companyEmployees.Website,
                CompanyLogoUrl = companyEmployees.LogoUrl,
                CrunchUrl = companyEmployees.OrganizationURL,
                Email = emails.Count > 0 ? emails.FirstOrDefault() : "",
                EmailStatus = emails.Count > 0 && emails.Count < 4 ? "OK" : "",
                City = location.Count() > 0 ? location[0] : "",
                State = location.Count() > 1 ? location[1] : "",
                Country = location.Count() > 2 ? location[2] : "",
                PhoneNumber = companyEmployees.PhoneNumber,
                AmountEmployees = companyEmployees.AmountEmployees,
                Industry = companyEmployees.Industry,
                Twitter = companyEmployees.Twitter,
                Facebook = companyEmployees.Facebook,
                TechStack = string.Join(", ", technologiesStack),
                AccountID = accountId,
                CompanyID = companyEmployees.Id,
                ProfileStatus = employee.ProfileStatus
            };
        }
    }
}
