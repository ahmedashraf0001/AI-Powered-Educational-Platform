using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Pdf
{
    public class PageContent
    {
        public int PageNumber { get; set; }
        public string Content { get; set; }
        public List<PageSection> Sections { get; set; } = new();
        public string PrimarySection { get; set; }
        public string SourceFile { get; set; }
        public int WordCount { get; set; }
    }

    public class PageSection
    {
        public string Title { get; set; }
        public int Level { get; set; }
        public double StartY { get; set; }
    }
}
