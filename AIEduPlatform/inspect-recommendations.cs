#:package Npgsql@8.0.6

using System.Text.Json;
using Npgsql;

var recommendedCourseIds = new[]
{
    Guid.Parse("4122f09c-9997-4e92-8002-08038a1ffe65"),
    Guid.Parse("21b656e2-3d15-4e50-b45e-b40175e67800"),
    Guid.Parse("136d76d2-bc4d-4127-8343-e1aa7401403c"),
    Guid.Parse("dc5bd7c6-db41-4791-a433-7fddce2f7871"),
    Guid.Parse("3955375c-1a0a-4647-9476-4c1121a4faac"),
    Guid.Parse("92531b7d-6ac6-4877-b4d6-f8e75c284731"),
    Guid.Parse("b0e533d7-59ca-46b8-afdd-1ec284bb3b67"),
    Guid.Parse("7ea84e92-3466-4b0e-baa2-f4f34e436419"),
    Guid.Parse("0d559b8b-7e03-44d4-bca3-3d59709dafd2"),
    Guid.Parse("b2d70807-a775-40c9-a8e8-ab8bd1dbe5b1")
};

var userLookup = "ahmed mohamed";

var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "AIEduPlatform.Api", "appsettings.json");
if (!File.Exists(appSettingsPath))
{
    Console.WriteLine($"Could not find appsettings at: {appSettingsPath}");
    return;
}

var appSettingsJson = await File.ReadAllTextAsync(appSettingsPath);
using var doc = JsonDocument.Parse(appSettingsJson);

if (!doc.RootElement.TryGetProperty("ConnectionStrings", out var connStrings) ||
    !connStrings.TryGetProperty("DefaultConnection", out var defaultConn))
{
    Console.WriteLine("DefaultConnection not found in appsettings.json");
    return;
}

var connectionString = defaultConn.GetString();
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("DefaultConnection is empty.");
    return;
}

await using var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();

var findUserSql = @"
SELECT u.""Id"", u.""Email"", u.""FirstName"", u.""LastName""
FROM ""AspNetUsers"" u
WHERE lower(trim(u.""FirstName"" || ' ' || u.""LastName"")) = lower(@fullName)
   OR lower(u.""Email"") LIKE lower(@emailLike)
ORDER BY u.""CreatedAt"" DESC
LIMIT 5;";

Guid userId = Guid.Empty;

await using (var cmd = new NpgsqlCommand(findUserSql, conn))
{
    cmd.Parameters.AddWithValue("fullName", userLookup);
    cmd.Parameters.AddWithValue("emailLike", "%ahmed%" + "%");

    await using var reader = await cmd.ExecuteReaderAsync();

    if (!reader.HasRows)
    {
        Console.WriteLine("No user found for lookup: ahmed mohamed");
        return;
    }

    Console.WriteLine("Matched users:");

    var matchCount = 0;
    while (await reader.ReadAsync())
    {
        var id = reader.GetGuid(0);
        var email = reader.IsDBNull(1) ? "" : reader.GetString(1);
        var firstName = reader.IsDBNull(2) ? "" : reader.GetString(2);
        var lastName = reader.IsDBNull(3) ? "" : reader.GetString(3);

        matchCount++;
        Console.WriteLine($"  {matchCount}. {firstName} {lastName} | {email} | {id}");

        if (userId == Guid.Empty)
            userId = id;
    }
}

if (userId == Guid.Empty)
{
    Console.WriteLine("No user selected.");
    return;
}

var userTagsSql = @"
SELECT ut.""TagId"",
       coalesce(nullif(t.""DisplayName"", ''), t.""Name"") AS ""TagName"",
       ut.""Weight"",
       ut.""Source""
FROM ""UserTags"" ut
JOIN ""Tags"" t ON t.""Id"" = ut.""TagId""
WHERE ut.""UserId"" = @userId
ORDER BY ut.""Weight"" DESC, ""TagName"";";

var userTagIds = new List<Guid>();

Console.WriteLine();
Console.WriteLine("User tags:");

await using (var userTagsCmd = new NpgsqlCommand(userTagsSql, conn))
{
    userTagsCmd.Parameters.AddWithValue("userId", userId);

    await using var reader = await userTagsCmd.ExecuteReaderAsync();
    if (!reader.HasRows)
    {
        Console.WriteLine("  (none)");
    }
    else
    {
        while (await reader.ReadAsync())
        {
            var tagId = reader.GetGuid(0);
            var tagName = reader.IsDBNull(1) ? "" : reader.GetString(1);
            var weight = reader.GetDouble(2);
            var source = reader.GetInt32(3) switch
            {
                0 => "Manual",
                1 => "Derived",
                var other => $"Unknown({other})"
            };

            userTagIds.Add(tagId);
            Console.WriteLine($"  - {tagName} | weight={weight:F4} | source={source} | tagId={tagId}");
        }
    }
}

var coursesSql = @"
SELECT c.""Id"",
       c.""Title"",
       coalesce(array_agg(DISTINCT coalesce(nullif(t.""DisplayName"", ''), t.""Name""))
                FILTER (WHERE t.""Id"" IS NOT NULL), ARRAY[]::text[]) AS ""CourseTags""
FROM ""Courses"" c
LEFT JOIN ""CourseTags"" ct ON ct.""CourseId"" = c.""Id""
LEFT JOIN ""Tags"" t ON t.""Id"" = ct.""TagId""
WHERE c.""Id"" = ANY(@courseIds)
GROUP BY c.""Id"", c.""Title"";";

var overlapSql = @"
SELECT ct.""CourseId"",
       coalesce(array_agg(DISTINCT coalesce(nullif(t.""DisplayName"", ''), t.""Name""))
                FILTER (WHERE t.""Id"" IS NOT NULL), ARRAY[]::text[]) AS ""OverlapTags""
FROM ""CourseTags"" ct
JOIN ""Tags"" t ON t.""Id"" = ct.""TagId""
WHERE ct.""CourseId"" = ANY(@courseIds)
  AND ct.""TagId"" = ANY(@userTagIds)
GROUP BY ct.""CourseId"";";

var courseById = new Dictionary<Guid, (string Title, List<string> Tags)>();
await using (var coursesCmd = new NpgsqlCommand(coursesSql, conn))
{
    coursesCmd.Parameters.AddWithValue("courseIds", recommendedCourseIds);

    await using var reader = await coursesCmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        var id = reader.GetGuid(0);
        var title = reader.IsDBNull(1) ? "" : reader.GetString(1);
        var tags = reader.IsDBNull(2)
            ? new List<string>()
            : reader.GetFieldValue<string[]>(2).Where(x => !string.IsNullOrWhiteSpace(x)).OrderBy(x => x).ToList();

        courseById[id] = (title, tags);
    }
}

var overlapByCourseId = new Dictionary<Guid, List<string>>();
if (userTagIds.Count > 0)
{
    await using var overlapCmd = new NpgsqlCommand(overlapSql, conn);
    overlapCmd.Parameters.AddWithValue("courseIds", recommendedCourseIds);
    overlapCmd.Parameters.AddWithValue("userTagIds", userTagIds.ToArray());

    await using var reader = await overlapCmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        var courseId = reader.GetGuid(0);
        var overlapTags = reader.IsDBNull(1)
            ? new List<string>()
            : reader.GetFieldValue<string[]>(1).Where(x => !string.IsNullOrWhiteSpace(x)).OrderBy(x => x).ToList();

        overlapByCourseId[courseId] = overlapTags;
    }
}

Console.WriteLine();
Console.WriteLine("Recommended course tag summary:");

foreach (var courseId in recommendedCourseIds)
{
    if (!courseById.TryGetValue(courseId, out var courseData))
    {
        Console.WriteLine($"- {courseId} | NOT FOUND");
        continue;
    }

    var overlapTags = overlapByCourseId.TryGetValue(courseId, out var overlap)
        ? overlap
        : new List<string>();

    Console.WriteLine($"- {courseId} | {courseData.Title}");
    Console.WriteLine($"  tags: {(courseData.Tags.Count == 0 ? "(none)" : string.Join(", ", courseData.Tags))}");
    Console.WriteLine($"  overlap with user tags ({overlapTags.Count}): {(overlapTags.Count == 0 ? "(none)" : string.Join(", ", overlapTags))}");
}
