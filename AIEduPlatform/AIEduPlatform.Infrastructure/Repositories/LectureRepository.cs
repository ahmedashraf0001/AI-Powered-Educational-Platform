using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class LectureRepository : GenericRepository<Lecture>, ILectureRepository
    {
        private readonly AppDbContext _ctx;
        public LectureRepository(AppDbContext ctx):base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<bool> CourseHasLecturesAsync(Guid courseId, CancellationToken cancellationToken)
        {
            return await _ctx.Lectures.AnyAsync(l => l.CourseId == courseId, cancellationToken);
        }

        public async Task<int> DeleteByIdAsync(Guid lectureId, CancellationToken ct = default)
        {
           return await _ctx.Lectures
                .Where(e => e.Id == lectureId)
                .ExecuteDeleteAsync(ct);
        }   
        public async Task<Lecture?> GetLectureByIdAsync(Guid lectureId, bool includeMaterials = true, CancellationToken ct = default)
        {
            IQueryable<Lecture> query = _ctx.Lectures.AsNoTracking();
            query = AddInclude(query, includeMaterials);
            return await query.FirstOrDefaultAsync(e => e.Id ==  lectureId, ct);
        }
        public async Task<List<Lecture>> GetLecturesByCourseIdAsync(Guid courseId, bool includeMaterials = true, CancellationToken ct = default)
        {
            IQueryable<Lecture> query = _ctx.Lectures.AsNoTracking();
            query = AddInclude(query, includeMaterials);
            return await query.Where(e => e.CourseId == courseId).ToListAsync(ct);
        }

        public async Task<List<Lecture>> GetLecturesByCourseIdAsync(Guid courseId, CancellationToken cancellationToken)
        {
            return await _ctx.Lectures
               .Where(l => l.CourseId == courseId)
               .ToListAsync(cancellationToken);
        }

        public async Task<bool> LecturesExistInCourseAsync(Guid courseId, List<Guid> lectureIds, CancellationToken cancellationToken)
        {
            var existingCount = await _ctx.Lectures
                    .Where(l => l.CourseId == courseId && lectureIds.Contains(l.Id))
                    .CountAsync(cancellationToken);

            return existingCount == lectureIds.Count;
        }

        public async Task<List<Lecture>> SearchLecturesByKeywordAsync(string keyword, bool includeMaterials = true, CancellationToken ct = default)
        {
            IQueryable<Lecture> query = _ctx.Lectures.AsNoTracking();
            query = AddInclude(query, includeMaterials);
            return await query.Where(e => e.Title.Contains(keyword) || e.Description.Contains(keyword)).ToListAsync(ct);
        }
        private IQueryable<Lecture> AddInclude(IQueryable<Lecture> query, bool includeMaterials) => includeMaterials ? query.Include(e => e.Materials) : query;

    }
}
