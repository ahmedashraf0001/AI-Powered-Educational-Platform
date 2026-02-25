using AIEduPlatform.ML.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.ML.Configurations
{
    public class VisionSettings
    {
        public VisionUrlsSettings Urls { get; set; }
        public VisionConfigurations Configurations { get; set; }
        public HealthEndpointsSettings Health { get; set; }
    }
}
