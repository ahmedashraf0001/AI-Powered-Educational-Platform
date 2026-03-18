using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        // Course Management
        private ICourseRepository? _courses;
        private ILectureRepository? _lectures;
        private IMaterialRepository? _materials;
        private IEnrollmentRepository? _enrollments;

        // Study Session Features
        private IStudySessionRepository? _studySessions;
        private IFlashcardRepository? _flashcards;
        private IGeneratedQuizRepository? _generatedQuizzes;
        private IChatMessageRepository? _chatMessages;
        private IMindMapRepository? _mindMaps;

        // Exam Management
        private IExamRepository? _exams;
        private IQuestionRepository? _questions;
        private ISubmissionRepository? _submissions;
        private IGradeRepository? _grades;
        private IExamAttemptRepository? _examAttempts;

        // User Management
        private IUserRepository? _users;
        private IRefreshTokenRepository? _refreshTokens;

        // Reviews
        private IReviewRepository? _reviews;

        private IConceptRepository? _concepts;

        // Progress & Payments
        private ICategoryRepository? _categories;
        private IGenericRepository<CourseCategory>? _courseCategories;
        private IMaterialProgressRepository? _materialProgress;
        private ISemanticSectionRepository? _semanticSections;

        // Cart & Orders
        private ICartRepository? _carts;
        private IGenericRepository<CartItem>? _cartItems;
        private IOrderRepository? _orders;
        private IGenericRepository<OrderItem>? _orderItems;

        // Notifications
        private INotificationRepository? _notifications;

        // Voice Settings
        private IGenericRepository<UserVoiceSettings>? _voiceSettings;


        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        // Course Management
        public ICourseRepository Courses =>
            _courses ??= new CourseRepository(_context);

        public ILectureRepository Lectures =>
            _lectures ??= new LectureRepository(_context);

        public IMaterialRepository Materials =>
            _materials ??= new MaterialRepository(_context);

        public IEnrollmentRepository Enrollments =>
            _enrollments ??= new EnrollmentRepository(_context);

        // Study Session Features
        public IStudySessionRepository StudySessions =>
            _studySessions ??= new StudySessionRepository(_context);

        public IFlashcardRepository Flashcards =>
            _flashcards ??= new FlashcardRepository(_context);

        public IGeneratedQuizRepository GeneratedQuizzes =>
            _generatedQuizzes ??= new GeneratedQuizRepository(_context);

        public IChatMessageRepository ChatMessages =>
            _chatMessages ??= new ChatMessageRepository(_context);

        public IMindMapRepository MindMaps =>
            _mindMaps ??= new MindMapRepository(_context);

        // Exam Management
        public IExamRepository Exams =>
            _exams ??= new ExamRepository(_context);

        public IQuestionRepository Questions =>
            _questions ??= new QuestionRepository(_context);

        public ISubmissionRepository Submissions =>
            _submissions ??= new SubmissionRepository(_context);

        public IGradeRepository Grades =>
            _grades ??= new GradeRepository(_context);

        public IExamAttemptRepository ExamAttempts =>
            _examAttempts ??= new ExamAttemptRepository(_context);

        // User Management
        public IUserRepository Users =>
            _users ??= new UserRepository(_context);

        public IRefreshTokenRepository RefreshTokens =>
            _refreshTokens ??= new RefreshTokenRepository(_context);

        // Reviews
        public IReviewRepository Reviews =>
            _reviews ??= new ReviewRepository(_context);

        public IConceptRepository Concepts =>
            _concepts ??= new ConceptRepository(_context);

        // Progress & Payments
        public ICategoryRepository Categories =>
            _categories ??= new CategoryRepository(_context);

        public IGenericRepository<CourseCategory> CourseCategories =>
            _courseCategories ??= new GenericRepository<CourseCategory>(_context);

        public IMaterialProgressRepository MaterialProgress =>
            _materialProgress ??= new MaterialProgressRepository(_context);

        public ISemanticSectionRepository SemanticSections =>
            _semanticSections ??= new SemanticSectionRepository(_context);

        // Cart & Orders
        public ICartRepository Carts =>
            _carts ??= new CartRepository(_context);

        public IGenericRepository<CartItem> CartItems =>
            _cartItems ??= new GenericRepository<CartItem>(_context);

        public IOrderRepository Orders =>
            _orders ??= new OrderRepository(_context);

        public IGenericRepository<OrderItem> OrderItems =>
            _orderItems ??= new GenericRepository<OrderItem>(_context);

        // Notifications
        public INotificationRepository Notifications =>
            _notifications ??= new NotificationRepository(_context);

        // Voice Settings
        public IGenericRepository<UserVoiceSettings> VoiceSettings =>
            _voiceSettings ??= new GenericRepository<UserVoiceSettings>(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction is in progress.");
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction is in progress.");
            }

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
    }
}
