$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Core\Domain\Entities\Course.cs"
$content = Get-Content $filePath -Raw

$properties = @"
        public ICollection<CourseTag> CourseTags { get; set; }

        // Tag Rebuild Tracking
        public bool NeedsTagRebuild { get; set; }
        public int PendingContentChanges { get; set; }
        public DateTime? LastTagUpdatedAt { get; set; }
        public bool HasContentDeletions { get; set; }
"@

$content = $content -replace "        public ICollection<CourseTag> CourseTags { get; set; }", $properties
Set-Content -Path $filePath -Value $content
