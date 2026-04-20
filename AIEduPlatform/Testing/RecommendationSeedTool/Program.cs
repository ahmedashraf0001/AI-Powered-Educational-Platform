using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Infrastructure;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

const string teacherEmail = "ashrafalaslogy90@gmail.com";

var solutionRoot = FindSolutionRoot();
var apiPath = Path.Combine(solutionRoot, "AIEduPlatform.Api");
var config = new ConfigurationBuilder()
    .SetBasePath(apiPath)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();
services.AddInfrastructure(config);

using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

Console.WriteLine("Connecting to database and applying recommendation seed data...");

var now = DateTime.UtcNow;

var teacher = await db.Users.FirstOrDefaultAsync(u => u.Email == teacherEmail);
if (teacher == null)
{
    teacher = new User
    {
        Id = Guid.NewGuid(),
        Email = teacherEmail,
        UserName = teacherEmail,
        NormalizedEmail = teacherEmail.ToUpperInvariant(),
        NormalizedUserName = teacherEmail.ToUpperInvariant(),
        FirstName = "Ashraf",
        LastName = "Publisher",
        EmailConfirmed = true,
        IsEmailVerified = true,
        SecurityStamp = Guid.NewGuid().ToString("N"),
        ConcurrencyStamp = Guid.NewGuid().ToString(),
        CreatedAt = now,
        UpdatedAt = now,
        LockoutEnabled = false,
        PhoneNumberConfirmed = false,
        TwoFactorEnabled = false,
        AccessFailedCount = 0
    };

    db.Users.Add(teacher);
    Console.WriteLine($"Created publisher user: {teacherEmail}");
}

var studentSpecs = Enumerable.Range(1, 80)
    .Select(i => new SeedStudentSpec(i, $"rec.seed.student{i:000}@example.com"))
    .ToList();

var studentEmails = studentSpecs
    .Select(s => s.Email)
    .ToList();

var existingStudents = await db.Users
    .Where(u => u.Email != null && studentEmails.Contains(u.Email))
    .ToDictionaryAsync(u => u.Email!, u => u);

foreach (var spec in studentSpecs)
{
    var email = spec.Email;

    if (existingStudents.ContainsKey(email))
        continue;

    var student = new User
    {
        Id = Guid.NewGuid(),
        Email = email,
        UserName = email,
        NormalizedEmail = email.ToUpperInvariant(),
        NormalizedUserName = email.ToUpperInvariant(),
        FirstName = "Rec",
        LastName = spec.LastName,
        EmailConfirmed = true,
        IsEmailVerified = true,
        SecurityStamp = Guid.NewGuid().ToString("N"),
        ConcurrencyStamp = Guid.NewGuid().ToString(),
        CreatedAt = now,
        UpdatedAt = now,
        LockoutEnabled = false,
        PhoneNumberConfirmed = false,
        TwoFactorEnabled = false,
        AccessFailedCount = 0
    };

    db.Users.Add(student);
    existingStudents[email] = student;
}

var categoryNames = new[]
{
    "Data Science",
    "Web Development",
    "Cloud & DevOps",
    "Software Engineering",
    "AI & Machine Learning"
};

var existingCategories = await db.Categories
    .Where(c => categoryNames.Contains(c.Name))
    .ToDictionaryAsync(c => c.Name, c => c);

foreach (var name in categoryNames)
{
    if (existingCategories.ContainsKey(name))
        continue;

    var category = new Category
    {
        Id = Guid.NewGuid(),
        Name = name,
        Description = $"Seed category for recommendation tests: {name}"
    };

    db.Categories.Add(category);
    existingCategories[name] = category;
}

await db.SaveChangesAsync();

var students = existingStudents.Values.OrderBy(u => u.Email).ToList();
var categories = existingCategories;

if (students.Count < 20)
    throw new InvalidOperationException("Expected at least 20 seed students to create diverse recommendation profiles.");

var seeds = new List<CourseSeed>
{
    new(
        "[RecSeed] Python for Data Analysis",
        "Use Python, NumPy, and Pandas to clean noisy datasets, profile outliers, and ship repeatable analytics notebooks.",
        "Data Science",
        true,
        0m,
        AgeDays: 14,
        CompletedEnrollments: 28,
        ActiveEnrollments: 20,
        DroppedEnrollments: 6,
        PendingEnrollments: 4,
        ReviewRatings: [5, 5, 5, 4, 5, 4, 5, 5],
        TagNames: ["python", "pandas", "data-analysis", "jupyter", "eda"]),
    new(
        "[RecSeed] Applied Machine Learning with Scikit-Learn",
        "Train interpretable ML pipelines with feature engineering, robust validation, and model diagnostics for tabular problems.",
        "AI & Machine Learning",
        true,
        24.99m,
        AgeDays: 22,
        CompletedEnrollments: 20,
        ActiveEnrollments: 18,
        DroppedEnrollments: 8,
        PendingEnrollments: 4,
        ReviewRatings: [5, 4, 4, 4, 3, 4, 5],
        TagNames: ["python", "scikit-learn", "machine-learning", "feature-engineering", "model-evaluation"]),
    new(
        "[RecSeed] Deep Learning with PyTorch",
        "Build and tune neural networks for image and sequence tasks while learning practical training/debugging workflows.",
        "AI & Machine Learning",
        true,
        29.99m,
        AgeDays: 40,
        CompletedEnrollments: 10,
        ActiveEnrollments: 12,
        DroppedEnrollments: 10,
        PendingEnrollments: 6,
        ReviewRatings: [5, 4, 3, 3, 4, 3],
        TagNames: ["python", "pytorch", "deep-learning", "neural-networks", "computer-vision"]),
    new(
        "[RecSeed] ASP.NET Core API Engineering",
        "Design production-grade REST APIs with clean boundaries, secure authentication flows, and observability-ready endpoints.",
        "Web Development",
        true,
        29.99m,
        AgeDays: 9,
        CompletedEnrollments: 30,
        ActiveEnrollments: 14,
        DroppedEnrollments: 4,
        PendingEnrollments: 2,
        ReviewRatings: [5, 5, 4, 5, 5, 5, 4, 5],
        TagNames: ["csharp", "dotnet", "aspnet-core", "rest-api", "authentication"]),
    new(
        "[RecSeed] ASP.NET Core Minimal APIs in Practice",
        "Ship lightweight HTTP services using route groups, endpoint filters, validation, and versioned contracts.",
        "Web Development",
        true,
        21.99m,
        AgeDays: 11,
        CompletedEnrollments: 22,
        ActiveEnrollments: 16,
        DroppedEnrollments: 5,
        PendingEnrollments: 3,
        ReviewRatings: [5, 4, 4, 5, 4, 5, 4],
        TagNames: ["csharp", "dotnet", "aspnet-core", "minimal-api", "rest-api"]),
    new(
        "[RecSeed] ASP.NET Core Identity and JWT Security",
        "Implement identity stores, JWT/refresh tokens, authorization policies, and hardened authentication flows.",
        "Web Development",
        true,
        27.99m,
        AgeDays: 16,
        CompletedEnrollments: 19,
        ActiveEnrollments: 15,
        DroppedEnrollments: 6,
        PendingEnrollments: 2,
        ReviewRatings: [5, 5, 4, 4, 5, 4],
        TagNames: ["csharp", "dotnet", "aspnet-core", "identity", "jwt"]),
    new(
        "[RecSeed] ASP.NET Core SignalR Real-Time Apps",
        "Build low-latency collaboration features with SignalR hubs, connection management, and scalable fan-out strategies.",
        "Web Development",
        true,
        24.99m,
        AgeDays: 19,
        CompletedEnrollments: 14,
        ActiveEnrollments: 13,
        DroppedEnrollments: 4,
        PendingEnrollments: 3,
        ReviewRatings: [5, 4, 4, 4, 5],
        TagNames: ["csharp", "dotnet", "aspnet-core", "signalr", "realtime"]),
    new(
        "[RecSeed] ASP.NET Core MVC Enterprise Patterns",
        "Apply layered MVC architecture, model binding discipline, and maintainable UI patterns in large teams.",
        "Web Development",
        true,
        23.99m,
        AgeDays: 29,
        CompletedEnrollments: 12,
        ActiveEnrollments: 10,
        DroppedEnrollments: 4,
        PendingEnrollments: 2,
        ReviewRatings: [4, 4, 5, 4, 4],
        TagNames: ["csharp", "dotnet", "aspnet-core", "mvc", "web-architecture"]),
    new(
        "[RecSeed] ASP.NET Core Testing with xUnit",
        "Raise API confidence using unit tests, integration tests, WebApplicationFactory, and test data isolation patterns.",
        "Software Engineering",
        true,
        18.99m,
        AgeDays: 24,
        CompletedEnrollments: 10,
        ActiveEnrollments: 11,
        DroppedEnrollments: 3,
        PendingEnrollments: 2,
        ReviewRatings: [5, 4, 4, 5],
        TagNames: ["csharp", "dotnet", "aspnet-core", "xunit", "integration-testing"]),
    new(
        "[RecSeed] Clean Architecture for .NET Systems",
        "Apply domain boundaries, test seams, and dependency inversion to keep large .NET systems maintainable over time.",
        "Software Engineering",
        true,
        26.99m,
        AgeDays: 32,
        CompletedEnrollments: 18,
        ActiveEnrollments: 8,
        DroppedEnrollments: 3,
        PendingEnrollments: 2,
        ReviewRatings: [5, 5, 4, 4, 5, 4],
        TagNames: ["csharp", "dotnet", "clean-architecture", "ddd", "testing"]),
    new(
        "[RecSeed] Microservices with .NET and Docker",
        "Split monolith workloads into resilient services with containers, contracts, and deployment guardrails.",
        "Cloud & DevOps",
        true,
        31.99m,
        AgeDays: 18,
        CompletedEnrollments: 16,
        ActiveEnrollments: 12,
        DroppedEnrollments: 6,
        PendingEnrollments: 4,
        ReviewRatings: [4, 4, 3, 4, 3, 4, 5],
        TagNames: ["csharp", "dotnet", "docker", "microservices", "kubernetes"]),
    new(
        "[RecSeed] React + TypeScript Frontend Lab",
        "Build robust UI workflows with strict typing, reusable components, and practical testing strategies.",
        "Web Development",
        true,
        19.99m,
        AgeDays: 12,
        CompletedEnrollments: 15,
        ActiveEnrollments: 20,
        DroppedEnrollments: 9,
        PendingEnrollments: 3,
        ReviewRatings: [4, 3, 4, 3, 3, 4, 4],
        TagNames: ["react", "typescript", "frontend", "state-management", "testing-library"]),
    new(
        "[RecSeed] Advanced React Performance Patterns",
        "Diagnose render bottlenecks with profiling tools and apply memoization patterns where they matter most.",
        "Web Development",
        true,
        23.99m,
        AgeDays: 55,
        CompletedEnrollments: 8,
        ActiveEnrollments: 6,
        DroppedEnrollments: 3,
        PendingEnrollments: 2,
        ReviewRatings: [5, 5, 4, 5, 4],
        TagNames: ["react", "typescript", "frontend", "performance", "profiling"]),
    new(
        "[RecSeed] PostgreSQL Query Performance Clinic",
        "Tune slow database workloads using index design, execution plans, and transaction-aware SQL patterns.",
        "Software Engineering",
        true,
        22.99m,
        AgeDays: 65,
        CompletedEnrollments: 11,
        ActiveEnrollments: 8,
        DroppedEnrollments: 5,
        PendingEnrollments: 2,
        ReviewRatings: [4, 4, 4, 3, 4],
        TagNames: ["postgresql", "sql", "indexing", "query-optimization", "database"]),
    new(
        "[RecSeed] Kubernetes Reliability Playbook",
        "Improve service uptime with health probes, autoscaling, rollout policies, and production-grade runbooks.",
        "Cloud & DevOps",
        true,
        27.99m,
        AgeDays: 28,
        CompletedEnrollments: 7,
        ActiveEnrollments: 7,
        DroppedEnrollments: 7,
        PendingEnrollments: 3,
        ReviewRatings: [3, 3, 4, 3, 2, 3],
        TagNames: ["kubernetes", "docker", "devops", "observability", "sre"]),
    new(
        "[RecSeed] MLOps for Model Deployment",
        "Move models from notebook to production with CI/CD, drift monitoring, and repeatable release workflows.",
        "Cloud & DevOps",
        true,
        25.99m,
        AgeDays: 20,
        CompletedEnrollments: 4,
        ActiveEnrollments: 5,
        DroppedEnrollments: 4,
        PendingEnrollments: 3,
        ReviewRatings: [4, 4, 3, 4],
        TagNames: ["mlops", "python", "docker", "kubernetes", "monitoring"]),
    new(
        "[RecSeed] Prompt Engineering for Learning Apps",
        "Design prompt pipelines, evaluation loops, and guardrails tailored to adaptive educational experiences.",
        "AI & Machine Learning",
        true,
        14.99m,
        AgeDays: 6,
        CompletedEnrollments: 6,
        ActiveEnrollments: 2,
        DroppedEnrollments: 1,
        PendingEnrollments: 2,
        ReviewRatings: [5, 5, 5, 5, 4],
        TagNames: ["prompt-engineering", "llm", "education-ai", "evaluation", "rag"]),
    new(
        "[RecSeed] JavaScript Crash Course Marathon",
        "A fast-paced overview of modern JavaScript syntax and browser APIs with many short coding drills.",
        "Web Development",
        true,
        9.99m,
        AgeDays: 90,
        CompletedEnrollments: 10,
        ActiveEnrollments: 26,
        DroppedEnrollments: 18,
        PendingEnrollments: 6,
        ReviewRatings: [2, 3, 2, 3, 2, 2, 3, 2],
        TagNames: ["javascript", "frontend", "web-basics", "dom", "async"]),
    new(
        "[RecSeed] Formal Methods in Software Design",
        "Explore specification-first engineering with invariants and proof-inspired verification on critical components.",
        "Software Engineering",
        true,
        32.99m,
        AgeDays: 120,
        CompletedEnrollments: 3,
        ActiveEnrollments: 2,
        DroppedEnrollments: 1,
        PendingEnrollments: 1,
        ReviewRatings: [5, 5, 5],
        TagNames: ["formal-methods", "software-design", "verification", "architecture", "systems"])
};

var requiredTagNames = seeds
    .SelectMany(s => s.TagNames)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToList();

var existingTags = await db.Tags
    .Where(t => requiredTagNames.Contains(t.Name))
    .ToListAsync();

var tagByName = existingTags
    .ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

foreach (var tagName in requiredTagNames)
{
    if (tagByName.ContainsKey(tagName))
        continue;

    var tag = new Tag
    {
        Id = Guid.NewGuid(),
        Name = tagName,
        DisplayName = ToDisplayName(tagName),
        CreatedAt = now,
        UpdatedAt = now
    };

    db.Tags.Add(tag);
    tagByName[tagName] = tag;
}

await db.SaveChangesAsync();

var createdCourses = 0;
var createdLectures = 0;
var createdReviews = 0;
var updatedReviews = 0;
var removedReviews = 0;
var createdEnrollments = 0;
var updatedEnrollments = 0;
var droppedExtraEnrollments = 0;
var createdCourseTags = 0;

var seedStudentIdSet = students
    .Select(s => s.Id)
    .ToHashSet();

for (var idx = 0; idx < seeds.Count; idx++)
{
    var seed = seeds[idx];
    var publicationDate = now.AddDays(-seed.AgeDays);

    var course = await db.Courses.FirstOrDefaultAsync(c => c.TeacherId == teacher.Id && c.Title == seed.Title);
    if (course == null)
    {
        course = new Course
        {
            Id = Guid.NewGuid(),
            TeacherId = teacher.Id,
            Title = seed.Title,
            Description = seed.Description,
            IsPublished = seed.IsPublished,
            Price = seed.Price,
            CurrentEnrollmentCount = 0,
            NeedsTagRebuild = false,
            PendingContentChanges = 0,
            HasContentDeletions = false,
            CreatedAt = publicationDate,
            UpdatedAt = publicationDate,
            LastTagUpdatedAt = publicationDate
        };

        db.Courses.Add(course);
        createdCourses++;
    }
    else
    {
        course.Description = seed.Description;
        course.IsPublished = seed.IsPublished;
        course.Price = seed.Price;
        course.UpdatedAt = publicationDate;
        course.LastTagUpdatedAt = publicationDate;
        course.NeedsTagRebuild = false;
        course.PendingContentChanges = 0;
        course.HasContentDeletions = false;
    }

    await db.SaveChangesAsync();

    var category = categories[seed.Category];
    var hasCategory = await db.CourseCategories.AnyAsync(cc => cc.CourseId == course.Id && cc.CategoryId == category.Id);
    if (!hasCategory)
    {
        db.CourseCategories.Add(new CourseCategory
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            CategoryId = category.Id
        });
    }

    var desiredTagIds = seed.TagNames
        .Select(tagName => tagByName[tagName].Id)
        .ToHashSet();

    var existingCourseTagIds = await db.CourseTags
        .Where(ct => ct.CourseId == course.Id)
        .Select(ct => ct.TagId)
        .ToListAsync();

    foreach (var tagId in desiredTagIds)
    {
        if (existingCourseTagIds.Contains(tagId))
            continue;

        db.CourseTags.Add(new CourseTag
        {
            CourseId = course.Id,
            TagId = tagId
        });

        createdCourseTags++;
    }

    var lectureSeeds = new[]
    {
        $"Introduction to {seed.Title.Replace("[RecSeed] ", string.Empty)}",
        "Core Concepts and Foundations",
        "Hands-on Lab and Guided Walkthrough",
        "Capstone Exercise and Wrap-up"
    };

    for (var order = 1; order <= lectureSeeds.Length; order++)
    {
        var exists = await db.Lectures.AnyAsync(l => l.CourseId == course.Id && l.OrderIndex == order);
        if (exists)
            continue;

        db.Lectures.Add(new Lecture
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            OrderIndex = order,
            Title = lectureSeeds[order - 1],
            Description = $"Lecture {order} of {seed.Title}: detailed explanation, examples, and actionable exercises."
        });

        createdLectures++;
    }

    var targetEnrollmentCount = seed.CompletedEnrollments + seed.ActiveEnrollments + seed.DroppedEnrollments + seed.PendingEnrollments;
    var targetStudents = GetStudentWindow(students, startIndex: idx * 7, count: targetEnrollmentCount);
    var targetStatusByStudentId = new Dictionary<Guid, EnrollmentStatus>();

    for (var position = 0; position < targetStudents.Count; position++)
    {
        var status = position < seed.CompletedEnrollments
            ? EnrollmentStatus.Completed
            : position < seed.CompletedEnrollments + seed.ActiveEnrollments
                ? EnrollmentStatus.Active
                : position < seed.CompletedEnrollments + seed.ActiveEnrollments + seed.DroppedEnrollments
                    ? EnrollmentStatus.Dropped
                    : EnrollmentStatus.Pending;

        targetStatusByStudentId[targetStudents[position].Id] = status;
    }

    var existingSeedEnrollments = await db.Enrollments
        .Where(e => e.CourseId == course.Id && seedStudentIdSet.Contains(e.StudentId))
        .ToListAsync();

    var existingEnrollmentByStudent = existingSeedEnrollments
        .GroupBy(e => e.StudentId)
        .ToDictionary(g => g.Key, g => g.First());

    foreach (var (studentId, status) in targetStatusByStudentId)
    {
        if (!existingEnrollmentByStudent.TryGetValue(studentId, out var enrollment))
        {
            var enrollmentOffset = (idx * 13) + (Math.Abs(studentId.GetHashCode()) % 17) + 2;

            db.Enrollments.Add(new Enrollment
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                StudentId = studentId,
                EnrolledAt = now.AddDays(-enrollmentOffset),
                Status = status,
                AmountPaid = seed.Price,
                OrderId = null,
                RefundedAt = null,
                RefundAmount = null,
                StripeRefundId = null,
                UnenrolledAt = status == EnrollmentStatus.Dropped ? now.AddDays(-Math.Max(1, enrollmentOffset / 2)) : null,
            });

            createdEnrollments++;
            continue;
        }

        var changed = false;

        if (enrollment.Status != status)
        {
            enrollment.Status = status;
            changed = true;
        }

        if (enrollment.AmountPaid != seed.Price)
        {
            enrollment.AmountPaid = seed.Price;
            changed = true;
        }

        var expectedUnenrolledAt = status == EnrollmentStatus.Dropped
            ? (DateTime?)(enrollment.UnenrolledAt ?? now.AddDays(-1))
            : null;

        if (enrollment.UnenrolledAt != expectedUnenrolledAt)
        {
            enrollment.UnenrolledAt = expectedUnenrolledAt;
            changed = true;
        }

        if (changed)
            updatedEnrollments++;
    }

    foreach (var enrollment in existingSeedEnrollments)
    {
        if (targetStatusByStudentId.ContainsKey(enrollment.StudentId))
            continue;

        if (enrollment.Status == EnrollmentStatus.Dropped)
            continue;

        enrollment.Status = EnrollmentStatus.Dropped;
        enrollment.UnenrolledAt ??= now.AddDays(-1);
        droppedExtraEnrollments++;
    }

    var reviewTargets = targetStudents
        .Take(seed.ReviewRatings.Count)
        .ToList();

    var targetReviewByStudent = reviewTargets
        .Select((student, reviewIndex) => new
        {
            student.Id,
            Rating = seed.ReviewRatings[reviewIndex]
        })
        .ToDictionary(x => x.Id, x => x.Rating);

    var existingSeedReviews = await db.Reviews
        .Where(rv => rv.CourseId == course.Id && seedStudentIdSet.Contains(rv.StudentId))
        .ToListAsync();

    var existingReviewByStudent = existingSeedReviews
        .GroupBy(rv => rv.StudentId)
        .ToDictionary(g => g.Key, g => g.First());

    foreach (var (studentId, rating) in targetReviewByStudent)
    {
        var comment = BuildReviewComment(seed.Title, rating);

        if (!existingReviewByStudent.TryGetValue(studentId, out var review))
        {
            db.Reviews.Add(new Review
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                StudentId = studentId,
                Rating = rating,
                Comment = comment,
                CreatedAt = publicationDate,
                UpdatedAt = publicationDate
            });

            createdReviews++;
            continue;
        }

        if (review.Rating != rating || review.Comment != comment)
        {
            review.Rating = rating;
            review.Comment = comment;
            review.UpdatedAt = now;
            updatedReviews++;
        }
    }

    foreach (var review in existingSeedReviews)
    {
        if (targetReviewByStudent.ContainsKey(review.StudentId))
            continue;

        db.Reviews.Remove(review);
        removedReviews++;
    }

    await db.SaveChangesAsync();

    course.CurrentEnrollmentCount = await db.Enrollments.CountAsync(
        e => e.CourseId == course.Id && e.Status != EnrollmentStatus.Dropped);
    course.LastTagUpdatedAt = publicationDate;
}

await db.SaveChangesAsync();

var seededCourseTitles = seeds
    .Select(s => s.Title)
    .ToHashSet();

var seededCourseSnapshot = await db.Courses
    .Where(c => c.TeacherId == teacher.Id && seededCourseTitles.Contains(c.Title))
    .Select(c => new
    {
        c.Title,
        c.CurrentEnrollmentCount,
        AverageRating = c.Reviews.Select(r => (double?)r.Rating).Average() ?? 0d,
        ReviewCount = c.Reviews.Count(),
        CompletionRate = c.Enrollments.Count(e => e.Status != EnrollmentStatus.Pending) > 0
            ? (double)c.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed)
              / c.Enrollments.Count(e => e.Status != EnrollmentStatus.Pending)
            : 0d,
        Tags = c.CourseTags.Select(ct => ct.Tag.Name).OrderBy(t => t).ToList()
    })
    .OrderByDescending(c => c.CurrentEnrollmentCount)
    .ThenBy(c => c.Title)
    .ToListAsync();

Console.WriteLine(
    $"Seed complete. CreatedCourses={createdCourses}, CreatedLectures={createdLectures}, CreatedCourseTags={createdCourseTags}, " +
    $"CreatedReviews={createdReviews}, UpdatedReviews={updatedReviews}, RemovedReviews={removedReviews}, " +
    $"CreatedEnrollments={createdEnrollments}, UpdatedEnrollments={updatedEnrollments}, DroppedExtraEnrollments={droppedExtraEnrollments}");
Console.WriteLine($"Publisher used: {teacherEmail}");

Console.WriteLine();
Console.WriteLine("Seeded course snapshot (popularity + quality + tag clusters):");
foreach (var course in seededCourseSnapshot)
{
    Console.WriteLine(
        $"- {course.Title} | Enrollments={course.CurrentEnrollmentCount} | AvgRating={course.AverageRating:F2} ({course.ReviewCount} reviews) | " +
        $"CompletionRate={course.CompletionRate:P0} | Tags=[{string.Join(", ", course.Tags)}]");
}

return;

static string FindSolutionRoot()
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir != null)
    {
        var slnxPath = Path.Combine(dir.FullName, "AIEduPlatform.slnx");
        if (File.Exists(slnxPath))
            return dir.FullName;

        dir = dir.Parent;
    }

    throw new InvalidOperationException("Could not locate AIEduPlatform.slnx from current directory.");
}

static List<User> GetStudentWindow(IReadOnlyList<User> students, int startIndex, int count)
{
    if (count <= 0)
        return new List<User>();

    var result = new List<User>(count);
    var normalizedStart = Math.Abs(startIndex) % students.Count;

    for (var i = 0; i < count; i++)
    {
        var studentIndex = (normalizedStart + i) % students.Count;
        result.Add(students[studentIndex]);
    }

    return result;
}

static string ToDisplayName(string tagName)
{
    var spaced = tagName.Replace('-', ' ');
    return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(spaced);
}

static string BuildReviewComment(string title, int rating)
{
    var sentiment = rating switch
    {
        >= 5 => "Excellent depth and practical value.",
        4 => "Strong course with useful examples.",
        3 => "Decent material but uneven pacing.",
        2 => "Needs clearer structure and more guidance.",
        _ => "Did not meet expectations yet."
    };

    return $"[Seed Review] {sentiment} Course: {title}.";
}

internal sealed record SeedStudentSpec(int Index, string Email)
{
    public string LastName => $"Student{Index:000}";
}

internal sealed record CourseSeed(
    string Title,
    string Description,
    string Category,
    bool IsPublished,
    decimal Price,
    int AgeDays,
    int CompletedEnrollments,
    int ActiveEnrollments,
    int DroppedEnrollments,
    int PendingEnrollments,
    IReadOnlyList<int> ReviewRatings,
    IReadOnlyList<string> TagNames);
