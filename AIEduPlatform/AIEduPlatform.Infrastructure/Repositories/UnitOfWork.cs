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

        private ICourseRepository? _courses;
        private ILectureRepository? _lectures;
        private IEnrollmentRepository? _enrollments;
        private IMaterialRepository? _materials;
        private IFlashcardRepository? _flashcards;
        private IGeneratedQuizRepository? _generatedQuizzes;
        private IChatMessageRepository? _chatMessages;
        private IMindMapRepository? _mindMaps;
        private IRefreshTokenRepository? _refreshTokens;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public ICourseRepository Courses =>
            _courses ??= new CourseRepository(_context);

        public ILectureRepository Lectures =>
            _lectures ??= new LectureRepository(_context);

        public IEnrollmentRepository Enrollments =>
            _enrollments ??= new EnrollmentRepository(_context);

        public IMaterialRepository Materials =>
            _materials ??= new MaterialRepository(_context);

        public IFlashcardRepository Flashcards =>
            _flashcards ??= new FlashcardRepository(_context);

        public IGeneratedQuizRepository GeneratedQuizzes =>
            _generatedQuizzes ??= new GeneratedQuizRepository(_context);

        public IChatMessageRepository ChatMessages =>
            _chatMessages ??= new ChatMessageRepository(_context);

        public IMindMapRepository MindMaps =>
            _mindMaps ??= new MindMapRepository(_context);

        public IRefreshTokenRepository RefreshTokens =>
            _refreshTokens ??= new RefreshTokenRepository(_context);

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
