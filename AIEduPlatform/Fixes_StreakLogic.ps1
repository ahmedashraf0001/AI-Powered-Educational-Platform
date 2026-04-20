$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Application\Features\Users\Queries\GetStudentDashboard\GetStudentDashboardQueryHandler.cs"
$content = Get-Content $filePath -Raw

$streakLogic = @"
            recentActivity = recentActivity.OrderByDescending(a => a.CompletedAt).Take(10).ToList();

            // Calculate Study Streak Data
            var streakData = new StudyStreakData();
            var today = DateTime.UtcNow.Date;
            var activeDates = studySessions.Select(s => s.StartedAt.Date).Distinct().ToList();

            int currentStreak = 0;
            var checkDate = today;
            if (!activeDates.Contains(today)) {
                checkDate = today.AddDays(-1);
            }
            while (activeDates.Contains(checkDate)) {
                currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }
            streakData.CurrentStreak = currentStreak;

            var currentDayOfWeek = (int)today.DayOfWeek;
            int diff = (7 + (currentDayOfWeek - 1)) % 7; 
            var startOfWeek = today.AddDays(-1 * diff);

            for (int i = 0; i < 7; i++) {
                streakData.ActiveDays.Add(activeDates.Contains(startOfWeek.AddDays(i)));
            }

            return new StudentDashboardDto
            {
                Streak = streakData,
"@

$content = $content -replace [regex]::Escape("            recentActivity = recentActivity.OrderByDescending(a => a.CompletedAt).Take(10).ToList();`r`n`r`n            return new StudentDashboardDto`r`n            {"), $streakLogic

Set-Content -Path $filePath -Value $content
