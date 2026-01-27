using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.ML_Health
{
    public class GpuInfo
    {
        public int GpuCount { get; set; }
        public string GpuName { get; set; }
        public double GpuMemoryAllocatedMb { get; set; }
        public double GpuMemoryReservedMb { get; set; }
    }
}
