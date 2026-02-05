using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Stats;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository for User entity operations.
    /// Provides additional user-related queries beyond what Identity provides.
    /// Note: User inherits from IdentityUser, so basic auth operations use UserManager.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a user by ID with optional related data
        /// </summary>
        Task<User?> GetUserByIdAsync(
            Guid userId,
            bool includeEnrollments = false,
            bool includeTaughtCourses = false,
            CancellationToken ct = default);

        /// <summary>
        /// Gets a user by email
        /// </summary>
        Task<User?> GetUserByEmailAsync(
            string email,
            CancellationToken ct = default);

        /// <summary>
        /// Gets all students enrolled in a course
        /// </summary>
        Task<List<User>> GetStudentsByCourseIdAsync(
            Guid courseId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets all teachers (users who have taught courses)
        /// </summary>
        Task<List<User>> GetAllTeachersAsync(
            CancellationToken ct = default);

        /// <summary>
        /// Gets all students (users who have enrollments)
        /// </summary>
        Task<List<User>> GetAllStudentsAsync(
            CancellationToken ct = default);

        /// <summary>
        /// Searches users by name or email
        /// </summary>
        Task<List<User>> SearchUsersAsync(
            string searchTerm,
            int maxResults = 20,
            CancellationToken ct = default);

        /// <summary>
        /// Gets user profile statistics
        /// </summary>
        Task<UserProfileStats> GetUserStatsAsync(
            Guid userId,
            CancellationToken ct = default);

        /// <summary>
        /// Checks if a user is enrolled in a course
        /// </summary>
        Task<bool> IsEnrolledInCourseAsync(
            Guid userId,
            Guid courseId,
            CancellationToken ct = default);

        /// <summary>
        /// Checks if a user is the teacher of a course
        /// </summary>
        Task<bool> IsTeacherOfCourseAsync(
            Guid userId,
            Guid courseId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets recently active users
        /// </summary>
        Task<List<User>> GetRecentlyActiveUsersAsync(
            int days = 7,
            int maxResults = 50,
            CancellationToken ct = default);
    }
}
