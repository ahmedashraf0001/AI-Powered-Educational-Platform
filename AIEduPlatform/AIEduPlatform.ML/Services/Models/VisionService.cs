using AIEduPlatform.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.ML.Services.Models
{
    public class VisionService : IVisionService
    {
        public Task<string> ExtractInfoFromImageAsync(byte[] imageData)
        {
            throw new NotImplementedException();
        }
    }
}
