using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        private readonly AppDbContext _ctx;
        public CourseRepository(AppDbContext ctx):base(ctx) {   
            _ctx = ctx;     
        }

        public async Task<int> DeleteByIdAsync(Guid courseId, CancellationToken ct = default)
        {
           return await _ctx.Courses
                .Where(e => e.Id == courseId)
                .ExecuteDeleteAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(Guid courseId, CourseIncludeOptions options = null, CancellationToken ct = default)
        {
            IQueryable<Course> query = _ctx.Courses.AsNoTracking();
            query = AddIncludes(query, options);
            return await query.FirstOrDefaultAsync(e => e.Id == courseId, ct);
        }


        public async Task<List<Course>?> SearchCoursesByKeywordAsync(string keyword, CourseIncludeOptions options = null, CancellationToken ct = default)
        {
            IQueryable<Course> query = _ctx.Courses.AsNoTracking();
            query = AddIncludes(query, options);
            return await query.Where(e => e.Title.Contains(keyword) || e.Description.Contains(keyword))
                         .ToListAsync(ct);
        }

        private IQueryable<Course> AddIncludes(IQueryable<Course> query, CourseIncludeOptions options)
        {
            if (options == null) return query;

            if (options.IncludeEnrollments)
                query = query.Include(c => c.Enrollments);

            if (options.IncludeExams)
                query = query.Include(c => c.Exams);

            if (options.IncludeStudySessions)
                query = query.Include(c => c.StudySessions);

            if (options.IncludeLectures)
                query = query.Include(c => c.Lectures);

            if (options.IncludeMaterials)
                query = query.Include(c => c.Lectures)
                             .ThenInclude(l => l.Materials);

            return query;
        }
        public async Task<bool> HasUnindexedMaterialsAsync(Guid courseId, CancellationToken cancellationToken)
        {
            return await _ctx.Materials
                .Where(m => m.Lecture.CourseId == courseId && !m.Indexed)
                .AnyAsync(cancellationToken);
        }

        public async Task<List<Material>> GetMaterialsToIndexAsync(Guid courseId, bool reindex, CancellationToken cancellationToken)
        {
            var query = _ctx.Materials
                .Include(m => m.Lecture)
                .Where(m => m.Lecture.CourseId == courseId);

            if (!reindex)
            {
                query = query.Where(m => !m.Indexed);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<int> GetMaterialsCountAsync(Guid courseId, CancellationToken cancellationToken)
        {
            return await _ctx.Materials
                .Where(m => m.Lecture.CourseId == courseId)
                .CountAsync(cancellationToken);
        }

        public async Task<bool> CourseExistsAsync(Guid courseId, CancellationToken cancellationToken)
        {
            return await _ctx.Courses.AnyAsync(c => c.Id == courseId, cancellationToken);
        }
    }

}
