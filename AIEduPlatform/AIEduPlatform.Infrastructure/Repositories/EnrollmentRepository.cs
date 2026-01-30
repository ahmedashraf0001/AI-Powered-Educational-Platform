using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<int> DeleteEnrollmentAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(e => e.StudentId == studentId && e.CourseId == courseId)
                .ExecuteDeleteAsync(ct);
        }

        public async Task<bool> EnrollmentExistsAsync(Guid enrollmentId, CancellationToken ct = default)
        {
            return await _dbSet.AnyAsync(e => e.Id == enrollmentId, ct);
        }

        public async Task<List<Enrollment>> GetActiveEnrollmentsByCourseAsync(Guid courseId, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active)
                .ToListAsync(ct);
        }

        public async Task<List<Enrollment>> GetActiveEnrollmentsByStudentAsync(Guid studentId, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
                .ToListAsync(ct);
        }

        public async Task<int> GetCourseEnrollmentCountAsync(Guid courseId, CancellationToken ct = default)
        {
            return await _dbSet.CountAsync(e => e.CourseId == courseId, ct);
        }

        public async Task<Enrollment?> GetEnrollmentAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId, ct);
        }

        public async Task<List<Enrollment>> GetEnrollmentsByCourseAsync(Guid courseId, bool includeStudent = false, CancellationToken ct = default)
        {
            var query = _dbSet.Where(e => e.CourseId == courseId);
            
            if (includeStudent)
            {
                query = query.Include(e => e.Student);
            }
            
            return await query.ToListAsync(ct);
        }

        public async Task<List<Enrollment>> GetEnrollmentsByStatusAsync(EnrollmentStatus status, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(e => e.Status == status)
                .ToListAsync(ct);
        }

        public async Task<List<Enrollment>> GetEnrollmentsByStudentAsync(Guid studentId, bool includeCourse = false, CancellationToken ct = default)
        {
            var query = _dbSet
                .Where(e => e.StudentId == studentId);
            if (includeCourse)
            {
                query = query.Include(e => e.Course);
            }
            return await query.ToListAsync(ct);
        }

        public async Task<int> GetStudentEnrollmentCountAsync(Guid studentId, CancellationToken ct = default)
        {
            return await _dbSet.CountAsync(e => e.StudentId == studentId, ct);
        }

        public async Task<bool> IsStudentEnrolledAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        {
            return await _dbSet
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId, ct);
        }

        public async Task<bool> UpdateEnrollmentStatusAsync(Guid studentId, Guid courseId, EnrollmentStatus status, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(e => e.StudentId == studentId && e.CourseId == courseId)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Status, status), ct) > 0;
        }
    }
}
