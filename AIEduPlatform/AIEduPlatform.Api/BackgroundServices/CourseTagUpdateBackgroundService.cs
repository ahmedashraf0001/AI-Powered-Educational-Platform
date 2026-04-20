using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace AIEduPlatform.Api.BackgroundServices
{
    public class CourseTagUpdateBackgroundService : BackgroundService
    {
        private static readonly ConcurrentDictionary<Guid, byte> _propagationInProgress = new();
        private static readonly ConcurrentDictionary<Guid, DateTime> _lastPropagationByCourse = new();
        private static readonly TimeSpan PropagationCooldown = TimeSpan.FromMinutes(2);

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CourseTagUpdateBackgroundService> _logger;

        public CourseTagUpdateBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<CourseTagUpdateBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CourseTagUpdateBackgroundService started.");

            // Poll every 30 seconds
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var courseIds = await GetPendingCourseIdsAsync(stoppingToken);

                    foreach (var courseId in courseIds)
                    {
                        try
                        {
                            await ProcessCourseTagUpdateAsync(courseId, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to update tags for CourseId: {CourseId}", courseId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during CourseTagUpdateBackgroundService polling.");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task<List<Guid>> GetPendingCourseIdsAsync(CancellationToken stoppingToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var pendingCourses = await unitOfWork.Courses.FindAsync(c => c.NeedsTagRebuild, stoppingToken);
            return pendingCourses
                .Select(c => c.Id)
                .Distinct()
                .ToList();
        }

        private async Task ProcessCourseTagUpdateAsync(Guid courseId, CancellationToken stoppingToken)
        {
            bool fullRebuild;
            DateTime deltaSince;

            await using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var course = await unitOfWork.Courses.GetCourseByIdAsync(
                    courseId,
                    new AIEduPlatform.Core.Domain.Entities.CourseIncludeOptions { IncludeTags = true },
                    stoppingToken);

                if (course == null)
                    return;

                _logger.LogInformation(
                    "Processing tag updates for CourseId: {CourseId}. Pending changes: {Changes}",
                    course.Id,
                    course.PendingContentChanges);

                var hasTags = course.CourseTags != null && course.CourseTags.Any();

                // Guard against stale flags: if nothing changed and tags already exist, clear and skip.
                if (course.PendingContentChanges <= 0 && !course.HasContentDeletions && hasTags)
                {
                    var trackedCourse = await unitOfWork.Courses.GetByIdAsync(courseId, stoppingToken);
                    if (trackedCourse != null)
                    {
                        trackedCourse.NeedsTagRebuild = false;
                        trackedCourse.PendingContentChanges = 0;
                        trackedCourse.HasContentDeletions = false;
                        trackedCourse.LastTagUpdatedAt = DateTime.UtcNow;
                        await unitOfWork.SaveChangesAsync(stoppingToken);
                    }

                    _logger.LogInformation(
                        "Skipping tag extraction for CourseId: {CourseId}. No pending content changes.",
                        course.Id);
                    return;
                }

                fullRebuild = course.PendingContentChanges >= 5 || course.HasContentDeletions || !hasTags;
                deltaSince = course.LastTagUpdatedAt ?? DateTime.MinValue;
            }

            await using (var extractionScope = _serviceProvider.CreateAsyncScope())
            {
                var tagExtractionService = extractionScope.ServiceProvider.GetRequiredService<ITagExtractionService>();

                if (fullRebuild)
                {
                    _logger.LogInformation("Running full tag extraction for CourseId: {CourseId}", courseId);
                    await tagExtractionService.ExtractCourseTagsAsync(courseId, stoppingToken);
                }
                else
                {
                    _logger.LogInformation("Running delta tag update for CourseId: {CourseId}", courseId);
                    await tagExtractionService.ExtractCourseDeltaTagsAsync(courseId, deltaSince, stoppingToken);
                }
            }

            await using (var finalizeScope = _serviceProvider.CreateAsyncScope())
            {
                var unitOfWork = finalizeScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var trackedCourse = await unitOfWork.Courses.GetByIdAsync(courseId, stoppingToken);

                if (trackedCourse == null)
                    return;

                trackedCourse.NeedsTagRebuild = false;
                trackedCourse.PendingContentChanges = 0;
                trackedCourse.HasContentDeletions = false;
                trackedCourse.LastTagUpdatedAt = DateTime.UtcNow;

                await unitOfWork.SaveChangesAsync(stoppingToken);
            }

            await PropagateUserTagUpdatesAsync(courseId, stoppingToken);

            _logger.LogInformation("Successfully updated tags for CourseId: {CourseId}", courseId);
        }

        private async Task PropagateUserTagUpdatesAsync(Guid courseId, CancellationToken stoppingToken)
        {
            var nowUtc = DateTime.UtcNow;

            if (_lastPropagationByCourse.TryGetValue(courseId, out var lastPropagation) &&
                nowUtc - lastPropagation < PropagationCooldown)
            {
                _logger.LogInformation(
                    "Skipping user-tag propagation for CourseId: {CourseId}. Cooldown active ({Seconds}s remaining).",
                    courseId,
                    Math.Max(0, (int)(PropagationCooldown - (nowUtc - lastPropagation)).TotalSeconds));
                return;
            }

            if (!_propagationInProgress.TryAdd(courseId, 0))
            {
                _logger.LogInformation(
                    "Skipping user-tag propagation for CourseId: {CourseId}. A propagation run is already in progress.",
                    courseId);
                return;
            }

            List<Guid> affectedStudentIds;
            try
            {
                await using (var readScope = _serviceProvider.CreateAsyncScope())
                {
                    var unitOfWork = readScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var enrollments = await unitOfWork.Enrollments
                        .GetActiveEnrollmentsByCourseAsync(courseId, stoppingToken);

                    affectedStudentIds = enrollments
                        .Select(e => e.StudentId)
                        .Distinct()
                        .ToList();
                }

                if (!affectedStudentIds.Any())
                {
                    _lastPropagationByCourse[courseId] = nowUtc;
                    return;
                }

                _logger.LogInformation(
                    "Rebuilding user tag profiles for {Count} students after CourseId {CourseId} tag update.",
                    affectedStudentIds.Count,
                    courseId);

                foreach (var studentId in affectedStudentIds)
                {
                    try
                    {
                        await using var rebuildScope = _serviceProvider.CreateAsyncScope();
                        var userTagService = rebuildScope.ServiceProvider.GetRequiredService<IUserTagService>();
                        await userTagService.RebuildUserTagsFromEnrollmentsAsync(studentId, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to rebuild user tags for StudentId: {StudentId} after CourseId {CourseId} tag update.",
                            studentId,
                            courseId);
                    }
                }

                _lastPropagationByCourse[courseId] = nowUtc;
            }
            finally
            {
                _propagationInProgress.TryRemove(courseId, out _);
            }
        }
    }
}




