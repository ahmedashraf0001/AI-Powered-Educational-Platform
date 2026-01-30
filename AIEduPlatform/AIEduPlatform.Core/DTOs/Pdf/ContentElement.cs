using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Pdf
{
    public enum ContentType
    {
        Text,
        Image
    }

    public class ContentElement
    {
        public ContentType Type { get; set; }
        public string Content { get; set; }
        public int ImageIndex { get; set; }
        public Position Position { get; set; }
        public double FontSize { get; set; }
    }

    public class Position
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
