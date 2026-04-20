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
        ICategoryRepository Categories { get; }
        IGenericRepository<AIEduPlatform.Core.Domain.Entities.CourseCategory> CourseCategories { get; }

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
        IExamAttemptRepository ExamAttempts { get; }

        // User Management
        IUserRepository Users { get; }
        IRefreshTokenRepository RefreshTokens { get; }

        // Reviews
        IReviewRepository Reviews { get; }

        IConceptRepository Concepts { get; }

        // Progress
        IMaterialProgressRepository MaterialProgress { get; }
        ISemanticSectionRepository SemanticSections { get; }

        // Cart & Orders
        ICartRepository Carts { get; }
        IGenericRepository<AIEduPlatform.Core.Domain.Entities.CartItem> CartItems { get; }
        IOrderRepository Orders { get; }
        IGenericRepository<AIEduPlatform.Core.Domain.Entities.OrderItem> OrderItems { get; }

        // Notifications
        INotificationRepository Notifications { get; }

        // Voice Settings
        IGenericRepository<AIEduPlatform.Core.Domain.Entities.UserVoiceSettings> VoiceSettings { get; }

        ITagRepository Tags { get; }


        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
