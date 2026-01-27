using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IOllamaService
    {
        Task<> ChatAsync();
        Task<> GenerateAsync();
    }
}
