using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _ctx;

        public UserRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public Task<List<User>> GetAllStudentsAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<User>> GetAllTeachersAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<User>> GetRecentlyActiveUsersAsync(int days = 7, int maxResults = 50, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<User>> GetStudentsByCourseIdAsync(Guid courseId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByIdAsync(Guid userId, bool includeEnrollments = false, bool includeTaughtCourses = false, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<UserProfileStats> GetUserStatsAsync(Guid userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsEnrolledInCourseAsync(Guid userId, Guid courseId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsTeacherOfCourseAsync(Guid userId, Guid courseId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<User>> SearchUsersAsync(string searchTerm, int maxResults = 20, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
