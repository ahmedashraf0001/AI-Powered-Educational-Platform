using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Recommedation;
using AIEduPlatform.Core.DTOs.Tags;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Data;
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
        public async Task AddRangeCourseTags(IEnumerable<CourseTag> courseTags, CancellationToken ct = default)
        {
            await _ctx.CourseTags.AddRangeAsync(courseTags, ct);
        }
        public void RemoveRangeCourseTags(IEnumerable<CourseTag> courseTags, CancellationToken ct = default)
        {
            _ctx.CourseTags.RemoveRange(courseTags);
        }
        public async Task<int> DeleteByIdAsync(Guid courseId, CancellationToken ct = default)
        {
            await _ctx.ConceptRelations
                .Where(r => _ctx.Concepts
                    .Where(c => c.CourseId == courseId)
                    .Select(c => c.Id)
                    .Contains(r.FromConceptId)
                    || _ctx.Concepts
                        .Where(c => c.CourseId == courseId)
                        .Select(c => c.Id)
                        .Contains(r.ToConceptId))
                .ExecuteDeleteAsync(ct);

            await _ctx.ConceptChunkMaps
                .Where(m => _ctx.Concepts
                    .Where(c => c.CourseId == courseId)
                    .Select(c => c.Id)
                    .Contains(m.ConceptId))
                .ExecuteDeleteAsync(ct);

            await _ctx.Concepts
                .Where(c => c.CourseId == courseId)
                .ExecuteDeleteAsync(ct);

            return await _ctx.Courses
                .Where(c => c.Id == courseId)
                .ExecuteDeleteAsync(ct);
        }

        public async Task<Course?> GetCourseByIdAsync(Guid courseId, CourseIncludeOptions options = null, CancellationToken ct = default)
        {
            IQueryable<Course> query = _ctx.Courses.AsNoTracking();
            query = AddIncludes(query, options);
            return await query.FirstOrDefaultAsync(e => e.Id == courseId, ct);
        }

        public async Task<List<Course>> GetCoursesByIdsAsync(IEnumerable<Guid> courseIds, CourseIncludeOptions options = null, CancellationToken ct = default)
        {
            var ids = courseIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
                return new List<Course>();

            IQueryable<Course> query = _ctx.Courses.AsNoTracking();
            query = AddIncludes(query, options);
            return await query.Where(c => ids.Contains(c.Id)).ToListAsync(ct);
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
            if (options == null)
                return query;

            if (options.IncludeTeacher)
                query = query.Include(c => c.Teacher);

            if (options.IncludeEnrollments)
                query = query.Include(c => c.Enrollments);

            if (options.IncludeExams)
                query = query.Include(c => c.Exams);

            if (options.IncludeStudySessions)
                query = query.Include(c => c.StudySessions);

            if (options.IncludeTags || options.IncludeCourseTags)
                query = query.Include(c => c.CourseTags).ThenInclude(ct => ct.Tag);

            if (options.IncludeMaterials)
            {
                query = query.Include(c => c.Lectures)
                             .ThenInclude(l => l.Materials);
            }
            else if (options.IncludeLectures)
            {
                query = query.Include(c => c.Lectures);
            }

            if (options.IncludeReviews)
                query = query.Include(c => c.Reviews);

            if (options.IncludeCategories)
                query = query.Include(c => c.CourseCategories)
                             .ThenInclude(cc => cc.Category);

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

        public async Task<(List<Course> Items, int TotalCount)> GetCoursesPagedAsync(
            bool onlyPublished,
            int page,
            int pageSize,
            CancellationToken ct = default,
            Guid? categoryId = null)
        {
            var query = _ctx.Courses.AsSplitQuery().AsNoTracking()
                .Include(c => c.Teacher)
                .Include(c => c.Lectures)
                .Include(c => c.Enrollments)
                .Include(c => c.Reviews)
                .Include(c => c.CourseCategories)
                    .ThenInclude(cc => cc.Category)
                .AsQueryable();

            if (onlyPublished)
                query = query.Where(c => c.IsPublished);

            if (categoryId.HasValue)
                query = query.Where(c => c.CourseCategories.Any(cc => cc.CategoryId == categoryId.Value));

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<(List<Course> Items, int TotalCount)> SearchCoursesPagedAsync(
            string keyword,
            bool onlyPublished,
            int page,
            int pageSize,
            CancellationToken ct = default,
            Guid? categoryId = null)
        {
            var query = _ctx.Courses.AsSplitQuery().AsNoTracking()
                .Include(c => c.Teacher)
                .Include(c => c.Lectures)
                .Include(c => c.Enrollments)
                .Include(c => c.Reviews)
                .Include(c => c.CourseCategories)
                    .ThenInclude(cc => cc.Category)
                .Where(c =>
                    EF.Functions.ILike(c.Title, $"%{keyword}%") ||
                    EF.Functions.ILike(c.Description, $"%{keyword}%"));

            if (onlyPublished)
                query = query.Where(c => c.IsPublished);

            if (categoryId.HasValue)
                query = query.Where(c => c.CourseCategories.Any(cc => cc.CategoryId == categoryId.Value));

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<(List<Course> Items, int TotalCount)> GetCoursesByInstructorPagedAsync(
            Guid instructorId,
            bool includeUnpublished,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var query = _ctx.Courses.AsSplitQuery().AsNoTracking()
                .Include(c => c.Teacher)
                .Include(c => c.Lectures)
                .Include(c => c.Enrollments)
                .Include(c => c.Reviews)
                .Include(c => c.CourseCategories)
                    .ThenInclude(cc => cc.Category)
                .Where(c => c.TeacherId == instructorId);

            if (!includeUnpublished)
                query = query.Where(c => c.IsPublished);

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
        public Task<List<CoursePopularityDto>> GetCoursePopularityAsync(IEnumerable<Guid> courseIds, CancellationToken ct = default)
        {
            var ids = courseIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
                return Task.FromResult(new List<CoursePopularityDto>());

            return _ctx.Courses.AsNoTracking()
                .Where(c => ids.Contains(c.Id))
                .Select(c => new CoursePopularityDto
                {
                    CourseId = c.Id,
                    EnrollmentCount = c.CurrentEnrollmentCount
                })
                .OrderByDescending(x => x.EnrollmentCount)
                .ToListAsync(ct);

        }

        public async Task<List<CourseTagsDto>> GetCourseTagsAsync(
            IEnumerable<Guid> courseIds,
            CancellationToken ct = default)
        {
            var ids = courseIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
                return new List<CourseTagsDto>();

            return await _ctx.CourseTags
                .AsNoTracking()
                .Where(ctg => ids.Contains(ctg.CourseId))
                .GroupBy(ctg => ctg.CourseId)
                .Select(g => new CourseTagsDto
                {
                    CourseId = g.Key,
                    TagIds = g.Select(x => x.TagId).Distinct().ToList()
                })
                .ToListAsync(ct);
        }
        public async Task<List<Course>> GetSelectedCoursesAsync(IEnumerable<Guid> courseIds, CancellationToken ct = default, CourseIncludeOptions? options = null)
        { 
            var ids = courseIds.Distinct().ToList();
            var result = new List<Course>();
            const int batchSize = 500;
            foreach (var batch in ids.Chunk(batchSize))
            { 
                IQueryable<Course> query = _ctx.Courses.Where(c => batch.Contains(c.Id));
                query = AddIncludes(query, options);
                var list = await query.ToListAsync(ct);
                result.AddRange(list); 
            } 
            return result; 
        }
        public Task<List<CourseQualityDto>> GetCourseQualityAsync(IEnumerable<Guid> courseIds, CancellationToken ct = default)
        {
            var ids = courseIds.Distinct().ToList();

            return _ctx.Courses
                .Where(c => ids.Contains(c.Id))
                .Select(c => new CourseQualityDto
                {
                    CourseId = c.Id,
                    AverageRating = c.Reviews
                        .Select(r => (double?)r.Rating)
                        .Average() ?? 0d,
                    ReviewCount = c.Reviews.Count(),
                    CompletionRate = c.Enrollments.Count(e => e.Status != AIEduPlatform.Core.Domain.Enums.EnrollmentStatus.Pending) > 0
                        ? (double)c.Enrollments.Count(e => e.Status == AIEduPlatform.Core.Domain.Enums.EnrollmentStatus.Completed)
                            / c.Enrollments.Count(e => e.Status != AIEduPlatform.Core.Domain.Enums.EnrollmentStatus.Pending)
                        : 0d
                })
                .OrderByDescending(x => x.AverageRating)
                .ToListAsync(ct);
        }

        public async Task<List<CourseRecencyDto>> GetCourseRecencyAsync(
            IEnumerable<Guid> courseIds,
            CancellationToken ct = default)
        {
            var ids = courseIds.Distinct().ToList();

            return await _ctx.Courses
                .Where(c => ids.Contains(c.Id))
                .Select(c => new CourseRecencyDto
                {
                    CourseId = c.Id,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    LastTagUpdatedAt = c.LastTagUpdatedAt
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<List<Guid>> GetCoursesBySimilarTagsAsync(
            IEnumerable<Guid> tagIds,
            int take,
            CancellationToken ct = default)
        {
            var ids = tagIds.Distinct().ToList();

            return await _ctx.CourseTags
                .Where(ctg => ids.Contains(ctg.TagId))
                .GroupBy(ctg => ctg.CourseId)
                .OrderByDescending(g => g.Count()) 
                .Select(g => g.Key)
                .Take(take)
                .ToListAsync(ct);
        }

        public Task<List<Guid>> GetTopPopularCoursesAsync(int take, CancellationToken ct = default)
        {
            if (take <= 0)
                return Task.FromResult(new List<Guid>());

            return _ctx.Courses.AsNoTracking()
                .Where(c => c.IsPublished)
                .OrderByDescending(c => c.CurrentEnrollmentCount)
                .ThenByDescending(c => c.CreatedAt)
                .Select(c => c.Id)
                .Take(take)
                .ToListAsync(ct);
        }

        public Task<List<Guid>> GetNewestCoursesAsync(int take, CancellationToken ct = default)
        {
            if (take <= 0)
                return Task.FromResult(new List<Guid>());

            return _ctx.Courses.AsNoTracking()
                .Where(c => c.IsPublished)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => c.Id)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<Dictionary<Guid, double>> GetSimilarityScoresAsync(
            Vector userEmbedding,
            IEnumerable<Guid> candidateCourseIds,
            CancellationToken ct = default)
        {
            var ids = candidateCourseIds?.Distinct().ToArray();

            if (ids == null || ids.Length == 0)
                return new Dictionary<Guid, double>();

            var sql = @"
        SELECT
            ""Id"",
            1 - (""TagEmbedding"" <=> @embedding) AS similarity
        FROM ""Courses""
        WHERE ""Id"" = ANY(@ids)
          AND ""TagEmbedding"" IS NOT NULL;
    ";

            var conn = _ctx.Database.GetDbConnection();
            var openedHere = conn.State != ConnectionState.Open;

            if (openedHere)
                await conn.OpenAsync(ct);

            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;

                var embeddingParam = cmd.CreateParameter();
                embeddingParam.ParameterName = "embedding";
                embeddingParam.Value = userEmbedding;
                cmd.Parameters.Add(embeddingParam);

                var idsParam = cmd.CreateParameter();
                idsParam.ParameterName = "ids";
                idsParam.Value = ids;
                cmd.Parameters.Add(idsParam);

                await using var reader = await cmd.ExecuteReaderAsync(ct);

                var result = new Dictionary<Guid, double>();

                while (await reader.ReadAsync(ct))
                {
                    var id = reader.GetGuid(0);
                    var score = reader.GetDouble(1);

                    result[id] = score;
                }

                return result;
            }
            finally
            {
                if (openedHere && conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }
    }

}


