using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.AI.Common;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IEnrollmentRepository : IGenericRepository<Enrollment>
    {
        Task<Enrollment?> GetEnrollmentAsync(Guid studentId, Guid courseId, CancellationToken ct = default);

        Task<List<Enrollment>> GetEnrollmentsByStudentAsync(Guid studentId, bool includeCourse = false, CancellationToken ct = default);

        Task<List<Enrollment>> GetEnrollmentsByCourseAsync(Guid courseId, bool includeStudent = false, CancellationToken ct = default);

        Task<bool> IsStudentEnrolledAsync(Guid studentId, Guid courseId, CancellationToken ct = default);

        Task<int> GetCourseEnrollmentCountAsync(Guid courseId, CancellationToken ct = default);

        Task<int> GetStudentEnrollmentCountAsync(Guid studentId, CancellationToken ct = default);

        Task<int> DeleteEnrollmentAsync(Guid studentId, Guid courseId, CancellationToken ct = default);

        Task<List<Enrollment>> GetActiveEnrollmentsByStudentAsync(Guid studentId, CancellationToken ct = default);

        Task<List<Enrollment>> GetActiveEnrollmentsByCourseAsync(Guid courseId, CancellationToken ct = default);

        Task<bool> UpdateEnrollmentStatusAsync(Guid studentId, Guid courseId, EnrollmentStatus status, CancellationToken ct = default);

        Task<bool> EnrollmentExistsAsync(Guid enrollmentId, CancellationToken ct = default);

        Task<List<Enrollment>> GetEnrollmentsByStatusAsync(EnrollmentStatus status, CancellationToken ct = default);
    }
}
