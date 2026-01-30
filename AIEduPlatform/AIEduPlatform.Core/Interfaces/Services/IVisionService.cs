using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IVisionService
    {
        Task<string> ExtractTextFromImageAsync(byte[] imageData, CancellationToken ct = default);
    }
}
