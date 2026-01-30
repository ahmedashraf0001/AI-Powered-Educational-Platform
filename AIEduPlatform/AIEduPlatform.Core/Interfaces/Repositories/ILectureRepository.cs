using AIEduPlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface ILectureRepository : IGenericRepository<Lecture>
    {
        Task<Lecture?> GetLectureByIdAsync(Guid lectureId, bool includeMaterials = true, CancellationToken ct = default);
        Task<List<Lecture>> GetLecturesByCourseIdAsync(Guid courseId, bool includeMaterials = true, CancellationToken ct = default);
        Task<List<Lecture>> SearchLecturesByKeywordAsync(string keyword, bool includeMaterials = true, CancellationToken ct = default);

        Task<bool> CourseHasLecturesAsync(Guid courseId, CancellationToken cancellationToken);
        Task<bool> LecturesExistInCourseAsync(Guid courseId, List<Guid> lectureIds, CancellationToken cancellationToken);
        Task<List<Lecture>> GetLecturesByCourseIdAsync(Guid courseId, CancellationToken cancellationToken); Task<int> DeleteByIdAsync(Guid lectureId, CancellationToken ct = default);
    }

}
