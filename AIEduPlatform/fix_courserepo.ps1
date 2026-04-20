$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Infrastructure\Repositories\CourseRepository.cs"
$content = Get-Content $filePath -Raw

$updatedRepoBlock = @"
            if (options.IncludeLectures)
                query = query.Include(c => c.Lectures);

            if (options.IncludeTags)
                query = query.Include(c => c.CourseTags).ThenInclude(ct => ct.Tag);

            if (options.IncludeMaterials)
"@

$content = $content -replace "            if \(options.IncludeLectures\)\r\n                query = query.Include\(c => c.Lectures\);\r\n\r\n            if \(options.IncludeMaterials\)", $updatedRepoBlock

Set-Content -Path $filePath -Value $content
