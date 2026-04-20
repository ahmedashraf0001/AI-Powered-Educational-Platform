using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.DTOs.Tags;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.Extensions.Options;
using Pgvector;
using System.Text;

namespace AIEduPlatform.Application.Common.Services
{
    public class UserTagConfiguration
    {
        public double BoostAmount { get; set; } = 0.2;
        public double DecayAmount { get; set; } = 0.05;
        public double ForgetThreshold { get; set; } = 0.05;
    }
    public class UserTagService : IUserTagService
    {
        private readonly IUnitOfWork _repo;
        private readonly IEmbeddingService _embeddingService;
        private readonly UserTagConfiguration _configuration;

        public UserTagService(
            IUnitOfWork repo,
            IEmbeddingService embeddingService,
            IOptions<UserTagConfiguration> configuration)
        {
            _repo = repo;
            _embeddingService = embeddingService;
            _configuration = configuration.Value;
        }

        // =========================
        // Enrollment (MAIN SIGNAL)
        // =========================

        public async Task ApplyCourseEnrollmentAsync(
            Guid userId,
            Guid courseId,
            CancellationToken cancellationToken = default)
        {
            var user = await _repo.Users.GetUserByIdAsync(
                userId,
                includeEnrollments: false,
                includeTaughtCourses: false,
                includeUserTags: true,
                cancellationToken);

            if (user == null)
                throw new InvalidDataException("User not found");

            var course = await _repo.Courses.GetCourseByIdAsync(
                courseId,
                new CourseIncludeOptions { IncludeTags = true, IncludeCourseTags = true },
                cancellationToken);

            if (course == null)
                throw new InvalidDataException("Course not found");

            var userTags = user.UserTags ??= new List<UserTag>();

            var courseTagIds = (course.CourseTags ?? new List<CourseTag>())
                .Select(ct => ct.TagId)
                .ToHashSet();

            // =========================
            // 1. BOOST matching tags
            // =========================

            foreach (var tagId in courseTagIds)
            {
                var userTag = userTags.FirstOrDefault(ut => ut.TagId == tagId);

                if (userTag == null)
                {
                    userTags.Add(new UserTag
                    {
                        UserId = userId,
                        TagId = tagId,
                        Weight = _configuration.BoostAmount,
                        LastUpdated = DateTime.UtcNow
                    });
                }
                else
                {
                    userTag.Weight = Math.Min(1.0, userTag.Weight + _configuration.BoostAmount);
                    userTag.LastUpdated = DateTime.UtcNow;
                }
            }

            // =========================
            // 2. DECAY non-matching tags
            // =========================

            foreach (var userTag in userTags)
            {
                if (!courseTagIds.Contains(userTag.TagId))
                {
                    userTag.Weight = Math.Max(0, userTag.Weight - _configuration.DecayAmount);
                    userTag.LastUpdated = DateTime.UtcNow;
                }
            }

            // =========================
            // 3. REMOVE weak tags
            // =========================

            var weakTags = userTags
                .Where(ut => ut.Weight < _configuration.ForgetThreshold)
                .ToList();

            _repo.Users.RemoveRangeUserTags(weakTags, cancellationToken);

            await SaveAndReembedUserTagsAsync(userId, cancellationToken);
        }

        // =========================
        // Batch enrollments
        // =========================

        public async Task ApplyBatchCourseEnrollmentsAsync(
            Guid userId,
            IEnumerable<Guid> courseIds,
            CancellationToken cancellationToken = default)
        {
            var normalizedCourseIds = courseIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (!normalizedCourseIds.Any())
                return;

            var user = await _repo.Users.GetUserByIdAsync(
                userId,
                includeEnrollments: false,
                includeTaughtCourses: false,
                includeUserTags: true,
                cancellationToken);

            if (user == null)
                throw new InvalidDataException("User not found");

            var userTags = user.UserTags ??= new List<UserTag>();

            var courses = await _repo.Courses.GetCoursesByIdsAsync(
                normalizedCourseIds,
                new CourseIncludeOptions { IncludeTags = true, IncludeCourseTags = true },
                cancellationToken);

            var foundCourseIds = courses.Select(c => c.Id).ToHashSet();
            var missingCourseId = normalizedCourseIds.FirstOrDefault(id => !foundCourseIds.Contains(id));
            if (missingCourseId != Guid.Empty)
                throw new InvalidDataException($"Course {missingCourseId} not found");

            // Aggregate tags across all enrolled courses once.
            var tagBoostCounts = courses
                .SelectMany(course => (course.CourseTags ?? new List<CourseTag>())
                    .Select(ct => ct.TagId)
                    .Distinct())
                .GroupBy(tagId => tagId)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var (tagId, boostCount) in tagBoostCounts)
            {
                var userTag = userTags.FirstOrDefault(ut => ut.TagId == tagId);
                var totalBoost = _configuration.BoostAmount * boostCount;

                if (userTag == null)
                {
                    userTags.Add(new UserTag
                    {
                        UserId = userId,
                        TagId = tagId,
                        Weight = Math.Min(1.0, totalBoost),
                        LastUpdated = DateTime.UtcNow
                    });
                }
                else
                {
                    userTag.Weight = Math.Min(1.0, userTag.Weight + totalBoost);
                    userTag.LastUpdated = DateTime.UtcNow;
                }
            }

            var activeTagIds = tagBoostCounts.Keys.ToHashSet();

            foreach (var userTag in userTags)
            {
                if (!activeTagIds.Contains(userTag.TagId))
                {
                    userTag.Weight = Math.Max(0, userTag.Weight - _configuration.DecayAmount);
                    userTag.LastUpdated = DateTime.UtcNow;
                }
            }

            var weakTags = userTags
                .Where(ut => ut.Weight < _configuration.ForgetThreshold)
                .ToList();

            _repo.Users.RemoveRangeUserTags(weakTags, cancellationToken);

            await SaveAndReembedUserTagsAsync(userId, cancellationToken);
        }

        public async Task RebuildUserTagsFromEnrollmentsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _repo.Users.GetUserByIdAsync(
                userId,
                includeEnrollments: false,
                includeTaughtCourses: false,
                includeUserTags: true,
                cancellationToken);

            if (user == null)
                throw new InvalidDataException("User not found");

            var nowUtc = DateTime.UtcNow;
            var userTags = user.UserTags ??= new List<UserTag>();

            var activeEnrollments = await _repo.Enrollments.GetActiveEnrollmentsByStudentAsync(
                userId,
                cancellationToken);

            var enrolledCourseIds = activeEnrollments
                .Select(e => e.CourseId)
                .Distinct()
                .ToList();

            if (!enrolledCourseIds.Any())
            {
                var existing = userTags.ToList();
                if (existing.Any())
                    _repo.Users.RemoveRangeUserTags(existing, cancellationToken);

                user.TagEmbedding = null;
                user.UserTags = new List<UserTag>();
                await _repo.SaveChangesAsync(cancellationToken);
                return;
            }

            var courses = await _repo.Courses.GetCoursesByIdsAsync(
                enrolledCourseIds,
                new CourseIncludeOptions { IncludeCourseTags = true },
                cancellationToken);

            var desiredWeights = courses
                .SelectMany(course => (course.CourseTags ?? new List<CourseTag>())
                    .Select(ct => ct.TagId)
                    .Where(tagId => tagId != Guid.Empty)
                    .Distinct())
                .GroupBy(tagId => tagId)
                .ToDictionary(
                    g => g.Key,
                    g => Math.Min(1.0, _configuration.BoostAmount * g.Count()));

            // Clean up accidental duplicate rows and keep a single tracked entity per tag.
            var duplicateRows = userTags
                .GroupBy(ut => ut.TagId)
                .SelectMany(g => g.Skip(1))
                .ToList();

            if (duplicateRows.Any())
                _repo.Users.RemoveRangeUserTags(duplicateRows, cancellationToken);

            var currentByTag = userTags
                .GroupBy(ut => ut.TagId)
                .ToDictionary(g => g.Key, g => g.First());

            var tagsToRemove = new List<UserTag>();

            foreach (var (tagId, existingTag) in currentByTag)
            {
                if (desiredWeights.TryGetValue(tagId, out var newWeight))
                {
                    existingTag.Weight = newWeight;
                    existingTag.LastUpdated = nowUtc;
                    existingTag.Source = TagSource.Derived;
                    desiredWeights.Remove(tagId);
                }
                else
                {
                    tagsToRemove.Add(existingTag);
                }
            }

            if (tagsToRemove.Any())
                _repo.Users.RemoveRangeUserTags(tagsToRemove, cancellationToken);

            var tagsToAdd = desiredWeights
                .Select(kv => new UserTag
                {
                    UserId = userId,
                    TagId = kv.Key,
                    Weight = kv.Value,
                    LastUpdated = nowUtc,
                    Source = TagSource.Derived
                })
                .ToList();

            if (tagsToAdd.Any())
                await _repo.Users.AddRangeUserTags(tagsToAdd, cancellationToken);

            var finalTags = currentByTag.Values
                .Where(tag => !tagsToRemove.Contains(tag))
                .Concat(tagsToAdd)
                .ToList();

            user.UserTags = finalTags;

            await SaveAndReembedUserTagsAsync(userId, cancellationToken);
        }

        // =========================
        // Direct boost
        // =========================

        public async Task IncreaseTagWeightAsync(
            Guid userId,
            Guid tagId,
            double amount,
            CancellationToken cancellationToken = default)
        {
            var user = await _repo.Users.GetUserByIdAsync(
                userId,
                includeEnrollments: false,
                includeTaughtCourses: false,
                includeUserTags: true,
                cancellationToken);

            if (user == null)
                throw new InvalidDataException("User not found");

            var userTags = user.UserTags ??= new List<UserTag>();

            var userTag = userTags.FirstOrDefault(ut => ut.TagId == tagId);

            if (userTag == null)
            {
                userTags.Add(new UserTag
                {
                    UserId = userId,
                    TagId = tagId,
                    Weight = amount,
                    LastUpdated = DateTime.UtcNow
                });
            }
            else
            {
                userTag.Weight = Math.Min(1.0, userTag.Weight + amount);
                userTag.LastUpdated = DateTime.UtcNow;
            }

            await SaveAndReembedUserTagsAsync(userId, cancellationToken);
        }

        // =========================
        // Global decay
        // =========================

        public async Task ApplyDecayAsync(
            Guid userId,
            double decayFactor,
            CancellationToken cancellationToken = default)
        {
            var user = await _repo.Users.GetUserByIdAsync(
                userId,
                includeEnrollments: false,
                includeTaughtCourses: false,
                includeUserTags: true,
                cancellationToken);

            if (user == null)
                throw new InvalidDataException("User not found");

            var userTags = user.UserTags ??= new List<UserTag>();

            foreach (var tag in userTags)
            {
                tag.Weight = Math.Max(0, tag.Weight * decayFactor);
                tag.LastUpdated = DateTime.UtcNow;
            }

            await SaveAndReembedUserTagsAsync(userId, cancellationToken);
        }

        // =========================
        // Decay unmatched tags
        // =========================

        public async Task DecreaseUnmatchedTagsAsync(
            Guid userId,
            IEnumerable<Guid> activeTagIds,
            double decayAmount,
            CancellationToken cancellationToken = default)
        {
            var user = await _repo.Users.GetUserByIdAsync(
                userId,
                includeEnrollments: false,
                includeTaughtCourses: false,
                includeUserTags: true,
                cancellationToken);

            if (user == null)
                throw new InvalidDataException("User not found");

            var userTags = user.UserTags ??= new List<UserTag>();

            var activeSet = activeTagIds.ToHashSet();

            foreach (var tag in userTags)
            {
                if (!activeSet.Contains(tag.TagId))
                {
                    tag.Weight = Math.Max(0, tag.Weight - decayAmount);
                    tag.LastUpdated = DateTime.UtcNow;
                }
            }

            await SaveAndReembedUserTagsAsync(userId, cancellationToken);
        }

        // =========================
        // Query
        // =========================

        public async Task<List<UserTag>> GetUserTagsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _repo.Users.GetUserByIdAsync(
                userId,
                includeEnrollments: false,
                includeTaughtCourses: false,
                includeUserTags: true,
                cancellationToken);

            if (user == null)
                throw new InvalidDataException("User not found");

            var userTags = user.UserTags ??= new List<UserTag>();

            return userTags.ToList();
        }

        public async Task ReembedUserTagsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _repo.Users.GetUserByIdAsync(
                userId,
                includeEnrollments: false,
                includeTaughtCourses: false,
                includeUserTags: false,
                cancellationToken);

            if (user == null)
                throw new InvalidDataException("User not found");

            var persistedUserTags = await _repo.Users.GetUserTagsAsync(userId, cancellationToken);
            var effectiveUserTags = persistedUserTags
                .Select(tag => new UserTag
                {
                    UserId = userId,
                    TagId = tag.TagId,
                    Weight = tag.Weight
                })
                .ToList();

            await UpdateUserTagEmbeddingAsync(user, effectiveUserTags, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);
        }

        // =========================
        // Cleanup
        // =========================

        public async Task RemoveLowWeightTagsAsync(
            Guid userId,
            double threshold,
            CancellationToken cancellationToken = default)
        {
            var user = await _repo.Users.GetUserByIdAsync(
                userId,
                includeEnrollments: false,
                includeTaughtCourses: false,
                includeUserTags: true,
                cancellationToken);

            if (user == null)
                throw new InvalidDataException("User not found");

            var userTags = user.UserTags ??= new List<UserTag>();

            var weakTags = userTags
                .Where(ut => ut.Weight < threshold)
                .ToList();

            _repo.Users.RemoveRangeUserTags(weakTags, cancellationToken);

            await SaveAndReembedUserTagsAsync(userId, cancellationToken);
        }

        private async Task SaveAndReembedUserTagsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await _repo.SaveChangesAsync(cancellationToken);
            await ReembedUserTagsAsync(userId, cancellationToken);
        }

        private async Task UpdateUserTagEmbeddingAsync(
            User user,
            IEnumerable<UserTag> effectiveUserTags,
            CancellationToken cancellationToken = default)
        {
            var weightedTags = effectiveUserTags
                .Where(ut => ut.Weight > 0)
                .GroupBy(ut => ut.TagId)
                .Select(g => new UserTagDto
                {
                    TagId = g.Key,
                    Weight = g.Max(x => x.Weight)
                })
                .OrderByDescending(ut => ut.Weight)
                .ToList();

            if (!weightedTags.Any())
            {
                user.TagEmbedding = null;
                return;
            }

            var tags = await _repo.Tags.GetAllByIdsAsync(weightedTags.Select(ut => ut.TagId), cancellationToken);
            var embeddingText = BuildUserEmbeddingText(weightedTags, tags);

            if (string.IsNullOrWhiteSpace(embeddingText))
            {
                user.TagEmbedding = null;
                return;
            }

            var response = await _embeddingService.GetEmbeddingAsync(
                new EmbeddingRequest
                {
                    Text = embeddingText,
                    Normalize = true
                },
                cancellationToken);

            user.TagEmbedding = response.Embedding?.Any() == true
                ? new Vector(response.Embedding.ToArray())
                : null;
        }

        private string BuildUserEmbeddingText(IEnumerable<UserTagDto> userTags, IEnumerable<Tag> tags)
        {
            var tagLookup = tags
                .GroupBy(t => t.Id)
                .ToDictionary(g => g.Key, g =>
                {
                    var tag = g.First();
                    return string.IsNullOrWhiteSpace(tag.DisplayName) ? tag.Name : tag.DisplayName;
                });

            var weightedTags = userTags
                .Where(ut => ut.Weight > 0)
                .OrderByDescending(ut => ut.Weight)
                .ToList();

            if (!weightedTags.Any())
                return string.Empty;

            var strong = new List<string>();
            var medium = new List<string>();
            var low = new List<string>();
            var weightedTerms = new StringBuilder();

            foreach (var ut in weightedTags)
            {
                if (!tagLookup.TryGetValue(ut.TagId, out var tagName) || string.IsNullOrWhiteSpace(tagName))
                    continue;

                if (ut.Weight >= 0.67)
                    strong.Add(tagName);
                else if (ut.Weight >= 0.34)
                    medium.Add(tagName);
                else
                    low.Add(tagName);

                var repeatCount = Math.Clamp((int)Math.Round(ut.Weight * 5, MidpointRounding.AwayFromZero), 1, 5);

                for (int i = 0; i < repeatCount; i++)
                {
                    weightedTerms.Append(tagName);
                    weightedTerms.Append(' ');
                }
            }

            var sentenceParts = new List<string>();

            if (strong.Any())
                sentenceParts.Add($"Strong interest in {JoinNaturalList(strong)}.");

            if (medium.Any())
                sentenceParts.Add($"Medium interest in {JoinNaturalList(medium)}.");

            if (low.Any())
                sentenceParts.Add($"Low interest in {JoinNaturalList(low)}.");

            var naturalSentence = string.Join(" ", sentenceParts).Trim();
            var repetitionSentence = weightedTerms.ToString().Trim();

            if (string.IsNullOrWhiteSpace(naturalSentence))
                return repetitionSentence;

            if (string.IsNullOrWhiteSpace(repetitionSentence))
                return naturalSentence;

            return $"{naturalSentence}\n{repetitionSentence}";
        }

        private static string JoinNaturalList(IReadOnlyList<string> items)
        {
            if (items.Count == 0)
                return string.Empty;

            if (items.Count == 1)
                return items[0];

            if (items.Count == 2)
                return $"{items[0]} and {items[1]}";

            return string.Join(", ", items.Take(items.Count - 1)) + $", and {items[^1]}";
        }
    }
}