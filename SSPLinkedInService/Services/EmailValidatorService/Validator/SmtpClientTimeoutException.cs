using System;
using System.Runtime.Serialization;

namespace SSPLinkedInService.Services.EmailValidatorService.Validator
{
    public class SmtpClientTimeoutException : SmtpClientException
    {
        public SmtpClientTimeoutException()
        {
        }

        public SmtpClientTimeoutException(string message) : base(message)
        {
        }

        public SmtpClientTimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SmtpClientTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
