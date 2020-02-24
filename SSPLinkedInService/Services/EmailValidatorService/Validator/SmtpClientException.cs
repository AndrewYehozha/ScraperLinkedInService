﻿using System;
using System.Runtime.Serialization;

namespace SSPLinkedInService.Services.EmailValidatorService.Validator
{
    public class SmtpClientException : EmailValidatorException
    {
        public SmtpClientException()
        {
        }

        public SmtpClientException(string message) : base(message)
        {
        }

        public SmtpClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SmtpClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
