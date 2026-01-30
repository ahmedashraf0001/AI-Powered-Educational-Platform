using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICourseRepository Courses { get; }
        ILectureRepository Lectures { get; }
        IEnrollmentRepository Enrollments { get; }
        IMaterialRepository Materials { get; }
        IFlashcardRepository Flashcards { get; }
        IGeneratedQuizRepository GeneratedQuizzes { get; }
        IChatMessageRepository ChatMessages { get; }
        IMindMapRepository MindMaps { get; }
        IRefreshTokenRepository RefreshTokens { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
