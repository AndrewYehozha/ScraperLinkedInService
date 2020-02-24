using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using SSPLinkedInService.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace SSPLinkedInService.Services.EmailValidatorService.Validator
{
    public class EmailValidator
    {
        public bool Validate(string email, out EmailValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                result = EmailValidationResult.AddressIsEmpty;
                return true;
            }

            email = email.Trim();

            MailAddress mailAddress = null;

            try
            {
                mailAddress = new MailAddress(email);
            }
            catch (ArgumentNullException)
            {
                result = EmailValidationResult.AddressIsEmpty;
                return true;
            }
            catch (ArgumentException)
            {
                result = EmailValidationResult.AddressIsEmpty;
                return true;
            }
            catch (FormatException)
            {
                result = EmailValidationResult.InvalidFormat;
                return true;
            }

            if (mailAddress.Address != email)
            {
                result = EmailValidationResult.InvalidFormat;
                return true;
            }

            //////////////////

            DomainName domainName = DomainName.Parse(mailAddress.Host);
            DnsMessage dnsResponse = DnsClient.Default.Resolve(domainName, RecordType.Mx);

            IList<MxRecord> mxRecords = dnsResponse.AnswerRecords.OfType<MxRecord>().ToList();

            if (mxRecords.Count == 0)
            {
                result = EmailValidationResult.NoMailForDomain;
                return true;
            }

            foreach (MxRecord mxRecord in mxRecords)
            {
                try
                {
                    SmtpClient smtpClient = new SmtpClient(mxRecord.ExchangeDomainName.ToString());
                    SmtpStatusCode resultCode;

                    if (smtpClient.CheckMailboxExists(email, out resultCode))
                    {
                        switch (resultCode)
                        {
                            case SmtpStatusCode.Ok:
                                result = EmailValidationResult.OK;
                                return true;

                            case SmtpStatusCode.ExceededStorageAllocation:
                                result = EmailValidationResult.MailboxStorageExceeded;
                                return true;

                            case SmtpStatusCode.MailboxUnavailable:
                                result = EmailValidationResult.MailboxUnavailable;
                                return true;
                        }
                    }
                }
                catch (SmtpClientException)
                {
                }
                catch (ArgumentNullException)
                {
                }
            }

            if (mxRecords.Count > 0)
            {
                result = EmailValidationResult.MailServerUnavailable;
                return false;
            }

            result = EmailValidationResult.Undefined;
            return false;
        }
    }
}
