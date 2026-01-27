using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
