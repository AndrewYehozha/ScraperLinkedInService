using SSPLinkedInService.Models.Types;
using SSPLinkedInService.Services.EmailValidatorService.Validator;
using System;
using System.Collections.Generic;

namespace SSPLinkedInService.Services.EmailValidatorService
{
    public class EmailHandler
    {
        private List<string> Emails { get; set; }

        private readonly EmailValidator _emailValidator;
        private EmailValidationResult _result;

        public EmailHandler()
        {
            _emailValidator = new EmailValidator();
        }

        public List<string> GetValidEmails(string firstName, string lastName, string companyDomain)
        {
            var isCorrectDomain = true;
            Emails = new List<string>();

            if (!string.IsNullOrEmpty(companyDomain))
            {
                foreach (var email in EmailsGenerator(firstName, lastName, companyDomain)) //Execution time: 25-30s
                {
                    if (!isCorrectDomain || Emails.Count > 4) { break; }

                    if (!_emailValidator.Validate(email, out _result))
                    {
                        Console.WriteLine("\nNo internet connection or mailserver is down / busy\n\n");
                    }

                    switch (_result)
                    {
                        case EmailValidationResult.OK:
                            Emails.Add(email);
                            break;
                        case EmailValidationResult.NoMailForDomain:
                            //Emails are not configured for domain
                            isCorrectDomain = false;
                            break;
                    }
                }
            }
            return Emails;
        }

        private List<string> EmailsGenerator(string firstName, string lastName, string companyDomain = "gmail.com")
        {
            var generatedEmailsList = new List<string>();

            firstName = firstName.Trim().ToLower();
            lastName = lastName.Trim().ToLower();
            companyDomain = companyDomain.Trim().ToLower();

            if (!string.IsNullOrEmpty(companyDomain))
            {
                try
                {
                    var uri = new Uri(companyDomain);
                    string host = uri.Host;

                    var arrDomain = host.Split('.');

                    if (arrDomain.Length >= 2)
                    {
                        companyDomain = $"{ arrDomain[arrDomain.Length - 2] }.{ arrDomain[arrDomain.Length - 1] }";

                        if (!string.IsNullOrEmpty(firstName))
                        {
                            generatedEmailsList.Add($"{ firstName }@{ companyDomain }");
                        }

                        if (!string.IsNullOrEmpty(lastName))
                        {
                            generatedEmailsList.Add($"{ lastName }@{ companyDomain }");
                        }

                        if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                        {
                            generatedEmailsList.Add($"{ firstName }{ lastName }@{ companyDomain }");
                            generatedEmailsList.Add($"{ lastName }{ firstName }@{ companyDomain }");

                            generatedEmailsList.Add($"{ firstName[0] }{ lastName }@{ companyDomain }");
                            generatedEmailsList.Add($"{ lastName }{ firstName[0] }@{ companyDomain }");
                            generatedEmailsList.Add($"{ firstName[0] }.{ lastName }@{ companyDomain }");
                            generatedEmailsList.Add($"{ lastName }.{ firstName[0] }@{ companyDomain }");
                            generatedEmailsList.Add($"{ firstName[0] }_{ lastName }@{ companyDomain }");
                            generatedEmailsList.Add($"{ lastName }_{ firstName[0] }@{ companyDomain }");

                            generatedEmailsList.Add($"{ lastName[0] }{ firstName }@{ companyDomain }");
                            generatedEmailsList.Add($"{ firstName }{ lastName[0] }@{ companyDomain }");
                            generatedEmailsList.Add($"{ lastName[0] }.{ firstName }@{ companyDomain }");
                            generatedEmailsList.Add($"{ firstName }.{ lastName[0] }@{ companyDomain }");
                            generatedEmailsList.Add($"{ lastName[0] }_{ firstName }@{ companyDomain }");
                            generatedEmailsList.Add($"{ firstName }_{ lastName[0] }@{ companyDomain }");

                            generatedEmailsList.Add($"{ firstName }.{ lastName }@{ companyDomain }");
                            generatedEmailsList.Add($"{ lastName }.{ firstName }@{ companyDomain }");
                            generatedEmailsList.Add($"{ firstName }_{ lastName }@{ companyDomain }");
                            generatedEmailsList.Add($"{ lastName }_{ firstName }@{ companyDomain }");
                        }
                    }
                }
                catch { }
            }
            return generatedEmailsList;
        }
    }
}
