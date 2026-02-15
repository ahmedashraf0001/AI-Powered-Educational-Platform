using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        // Course Management
        ICourseRepository Courses { get; }
        ILectureRepository Lectures { get; }
        IMaterialRepository Materials { get; }
        IEnrollmentRepository Enrollments { get; }

        // Study Session Features
        IStudySessionRepository StudySessions { get; }
        IFlashcardRepository Flashcards { get; }
        IGeneratedQuizRepository GeneratedQuizzes { get; }
        IChatMessageRepository ChatMessages { get; }
        IMindMapRepository MindMaps { get; }

        // Exam Management
        IExamRepository Exams { get; }
        IQuestionRepository Questions { get; }
        ISubmissionRepository Submissions { get; }
        IGradeRepository Grades { get; }

        // User Management
        IUserRepository Users { get; }
        IRefreshTokenRepository RefreshTokens { get; }

        // Reviews
        IReviewRepository Reviews { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
