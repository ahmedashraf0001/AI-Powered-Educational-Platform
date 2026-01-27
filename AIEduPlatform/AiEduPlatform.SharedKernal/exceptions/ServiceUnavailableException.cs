using System;
using System.Collections.Generic;
using System.Text;

namespace AiEduPlatform.SharedKernal.exceptions
{
    public class ServiceUnavailableException : Exception
    {
        public ServiceUnavailableException(string message) : base(message) { }

        public ServiceUnavailableException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
