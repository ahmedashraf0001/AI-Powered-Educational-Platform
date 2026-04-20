$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Core\Domain\Entities\Course.cs"
$content = Get-Content $filePath -Raw

$updatedCourseBlock = @"
        public bool IncludeLectures { get; set; } = false;
        public bool IncludeTags { get; set; } = false;
        public bool IncludeExams { get; set; } = false;
"@

$content = $content -replace "        public bool IncludeLectures { get; set; } = false;\r\n        public bool IncludeExams { get; set; } = false;", $updatedCourseBlock

Set-Content -Path $filePath -Value $content
