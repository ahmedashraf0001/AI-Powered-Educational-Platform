using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Tags
{
    public class CourseTaggingDto
    {
        public Guid CourseId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public List<LectureTaggingDto> Lectures { get; set; } = new();

    }
    public class LectureTaggingDto
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public List<MaterialTaggingDto> Materials { get; set; } = new();
    }
    public class MaterialTaggingDto
    {
        public string Title { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public string Type { get; set; } = string.Empty;

        public int? DurationSeconds { get; set; }

        public int? TotalPages { get; set; }
    }

    //response

    public class CourseTagsResultDto
    {
        public Guid CourseId { get; set; }

        public List<string> Tags { get; set; } = new();

        public List<string> Concepts { get; set; } = new();

        public List<TagExplanationDto> Explanations { get; set; } = new();
    }
    public class TagExplanationDto
    {
        public string Tag { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
    }
}
