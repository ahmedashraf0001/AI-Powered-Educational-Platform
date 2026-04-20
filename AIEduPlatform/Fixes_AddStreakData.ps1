$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Core\DTOs\Stats\StudentDashboardDto.cs"
$content = Get-Content $filePath -Raw

$newProperty = @"
        /// <summary>
        /// Study streak tracking data including current streak and weekly activity.
        /// </summary>
        public StudyStreakData Streak { get; set; } = new();

        /// <summary>
        /// Engagement analytics — sessions, time spent, materials viewed.
"@

$content = $content -replace [regex]::Escape("        /// <summary>`r`n        /// Engagement analytics"), $newProperty

$newClass = @"
    public class StudyStreakData
    {
        public int CurrentStreak { get; set; }
        public List<bool> ActiveDays { get; set; } = new();
    }

    public class RecentActivityItem
"@

$content = $content -replace [regex]::Escape("    public class RecentActivityItem"), $newClass

Set-Content -Path $filePath -Value $content
