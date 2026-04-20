using AIEduPlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IUserTagService
    {
        // =========================
        // Core behavior (ONLY enrollment-based learning)
        // =========================

        Task ApplyCourseEnrollmentAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);

        Task ApplyBatchCourseEnrollmentsAsync(Guid userId, IEnumerable<Guid> courseIds, CancellationToken cancellationToken = default);

        Task RebuildUserTagsFromEnrollmentsAsync(Guid userId, CancellationToken cancellationToken = default);

        Task ReembedUserTagsAsync(Guid userId, CancellationToken cancellationToken = default);

        // =========================
        // Weight operations
        // =========================

        Task IncreaseTagWeightAsync(Guid userId, Guid tagId, double amount, CancellationToken cancellationToken = default);

        Task DecreaseUnmatchedTagsAsync(Guid userId, IEnumerable<Guid> activeTagIds, double decayAmount, CancellationToken cancellationToken = default);

        Task ApplyDecayAsync(Guid userId, double decayFactor, CancellationToken cancellationToken = default);

        // =========================
        // Forgetting logic
        // =========================

        Task RemoveLowWeightTagsAsync(Guid userId, double threshold, CancellationToken cancellationToken = default);

        // =========================
        // Query
        // =========================

        Task<List<UserTag>> GetUserTagsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
