using AIEduPlatform.Application.Features.Courses.Commands.Courses.CreateCourse;
using AIEduPlatform.Application.Features.Courses.Commands.Courses.DeleteCourse;
using AIEduPlatform.Application.Features.Courses.Commands.Courses.PublishCourse;
using AIEduPlatform.Application.Features.Courses.Commands.Courses.UpdateCourse;
using AIEduPlatform.Application.Features.Courses.Commands.Enrollments.EnrollStudent;
using AIEduPlatform.Application.Features.Courses.Commands.Enrollments.UnenrollStudent;
using AIEduPlatform.Application.Features.Courses.Commands.Lectures.AddLecture;
using AIEduPlatform.Application.Features.Courses.Commands.Lectures.DeleteLecture;
using AIEduPlatform.Application.Features.Courses.Commands.Lectures.UpdateLecture;
using AIEduPlatform.Application.Features.Courses.Commands.Materials.DeleteMaterial;
using AIEduPlatform.Application.Features.Courses.Commands.Materials.UploadMaterial;
using AIEduPlatform.Application.Features.Courses.Queries.Courses.GetAllCourses;
using AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCourseById;
using AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCoursesByInstructor;
using AIEduPlatform.Application.Features.Courses.Queries.Courses.SearchCourses;
using AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetCourseEnrollments;
using AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetEnrolledCourses;
using AIEduPlatform.Application.Features.Courses.Queries.Lectures.GetCourseLectures;
using AIEduPlatform.Application.Features.Courses.Queries.Lectures.GetLectureMaterials;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.DTOs.Courses.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIEduPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CoursesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Courses - Public (for browsing)

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<CourseListDto>>> GetAllCourses()
        {
            var result = await _mediator.Send(new GetAllCoursesQuery { OnlyPublished = true });
            return Ok(result);
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CourseListDto>>> SearchCourses([FromQuery] string keyword)
        {
            var result = await _mediator.Send(new SearchCoursesQuery
            {
                Keyword = keyword,
                OnlyPublished = true
            });
            return Ok(result);
        }

        #endregion

        #region Courses - Authenticated

        [HttpGet("{courseId:guid}")]
        public async Task<ActionResult<CourseDetailDto>> GetCourseById(
            Guid courseId,
            [FromQuery] bool includeLectures = true,
            [FromQuery] bool includeMaterials = true)
        {
            var result = await _mediator.Send(new GetCourseByIdQuery
            {
                CourseId = courseId,
                IncludeLectures = includeLectures,
                IncludeMaterials = includeMaterials
            });
            return Ok(result);
        }

        [HttpGet("instructor/{instructorId:guid}")]
        public async Task<ActionResult<List<CourseListDto>>> GetCoursesByInstructor(
            Guid instructorId,
            [FromQuery] bool includeUnpublished = false)
        {
            var result = await _mediator.Send(new GetCoursesByInstructorQuery
            {
                InstructorId = instructorId,
                IncludeUnpublished = includeUnpublished
            });
            return Ok(result);
        }

        [HttpGet("my-courses")]
        public async Task<ActionResult<List<CourseListDto>>> GetMyCourses([FromQuery] bool includeUnpublished = true)
        {
            var result = await _mediator.Send(new GetCoursesByInstructorQuery
            {
                IncludeUnpublished = includeUnpublished
            });
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateCourse([FromBody] CreateCourseRequest request)
        {
            var courseId = await _mediator.Send(new CreateCourseCommand
            {
                Title = request.Title,
                Description = request.Description
            });
            return CreatedAtAction(nameof(GetCourseById), new { courseId }, new { CourseId = courseId });
        }

        [HttpPut("{courseId:guid}")]
        public async Task<IActionResult> UpdateCourse(Guid courseId, [FromBody] UpdateCourseRequest request)
        {
            await _mediator.Send(new UpdateCourseCommand
            {
                CourseId = courseId,
                Title = request.Title,
                Description = request.Description
            });
            return NoContent();
        }

        [HttpDelete("{courseId:guid}")]
        public async Task<IActionResult> DeleteCourse(Guid courseId)
        {
            await _mediator.Send(new DeleteCourseCommand { CourseId = courseId });
            return NoContent();
        }

        [HttpPost("{courseId:guid}/publish")]
        public async Task<IActionResult> PublishCourse(Guid courseId)
        {
            await _mediator.Send(new PublishCourseCommand { CourseId = courseId });
            return Ok(new { Message = "Course published successfully." });
        }

        #endregion

        #region Lectures

        [HttpGet("{courseId:guid}/lectures")]
        public async Task<ActionResult<List<LectureDto>>> GetCourseLectures(
            Guid courseId,
            [FromQuery] bool includeMaterials = true)
        {
            var result = await _mediator.Send(new GetCourseLecturesQuery
            {
                CourseId = courseId,
                IncludeMaterials = includeMaterials
            });
            return Ok(result);
        }

        [HttpPost("{courseId:guid}/lectures")]
        public async Task<ActionResult<Guid>> AddLecture(Guid courseId, [FromBody] AddLectureRequest request)
        {
            var lectureId = await _mediator.Send(new AddLectureCommand
            {
                CourseId = courseId,
                Title = request.Title,
                Description = request.Description,
                OrderIndex = request.OrderIndex
            });
            return CreatedAtAction(nameof(GetCourseLectures), new { courseId }, new { LectureId = lectureId });
        }

        [HttpPut("lectures/{lectureId:guid}")]
        public async Task<IActionResult> UpdateLecture(Guid lectureId, [FromBody] UpdateLectureRequest request)
        {
            await _mediator.Send(new UpdateLectureCommand
            {
                LectureId = lectureId,
                Title = request.Title,
                Description = request.Description,
                OrderIndex = request.OrderIndex
            });
            return NoContent();
        }

        [HttpDelete("lectures/{lectureId:guid}")]
        public async Task<IActionResult> DeleteLecture(Guid lectureId)
        {
            await _mediator.Send(new DeleteLectureCommand { LectureId = lectureId });
            return NoContent();
        }

        #endregion

        #region Materials

        [HttpGet("lectures/{lectureId:guid}/materials")]
        public async Task<ActionResult<List<MaterialDto>>> GetLectureMaterials(Guid lectureId)
        {
            var result = await _mediator.Send(new GetLectureMaterialsQuery { LectureId = lectureId });
            return Ok(result);
        }

        [HttpPost("lectures/{lectureId:guid}/materials")]
        public async Task<ActionResult<Guid>> UploadMaterial(
            Guid lectureId,
            [FromForm] UploadMaterialFormRequest request)
        {
            var materialId = await _mediator.Send(new UploadMaterialCommand
            {
                LectureId = lectureId,
                Title = request.Title,
                Type = request.Type,
                FileUrl = request.FileUrl,
                FileStream = request.File?.OpenReadStream(),
                FileName = request.File?.FileName,
                ContentType = request.File?.ContentType
            });
            return CreatedAtAction(nameof(GetLectureMaterials), new { lectureId }, new { MaterialId = materialId });
        }

        [HttpDelete("materials/{materialId:guid}")]
        public async Task<IActionResult> DeleteMaterial(Guid materialId)
        {
            await _mediator.Send(new DeleteMaterialCommand { MaterialId = materialId });
            return NoContent();
        }

        #endregion

        #region Enrollments

        [HttpGet("{courseId:guid}/enrollments")]
        public async Task<ActionResult<List<EnrollmentDto>>> GetCourseEnrollments(Guid courseId)
        {
            var result = await _mediator.Send(new GetCourseEnrollmentsQuery { CourseId = courseId });
            return Ok(result);
        }

        [HttpGet("enrolled")]
        public async Task<ActionResult<List<EnrollmentDto>>> GetMyEnrolledCourses()
        {
            var result = await _mediator.Send(new GetEnrolledCoursesQuery());
            return Ok(result);
        }

        [HttpPost("{courseId:guid}/enroll")]
        public async Task<ActionResult<Guid>> EnrollInCourse(Guid courseId)
        {
            var enrollmentId = await _mediator.Send(new EnrollStudentCommand { CourseId = courseId });
            return Ok(new { EnrollmentId = enrollmentId, Message = "Enrolled successfully." });
        }

        [HttpDelete("{courseId:guid}/unenroll")]
        public async Task<IActionResult> UnenrollFromCourse(Guid courseId)
        {
            await _mediator.Send(new UnenrollStudentCommand { CourseId = courseId });
            return Ok(new { Message = "Unenrolled successfully." });
        }

        #endregion
    }

    public record UploadMaterialFormRequest
    {
        public string Title { get; init; } = string.Empty;
        public Core.Domain.Enums.MaterialType Type { get; init; }
        public IFormFile? File { get; init; }
        public string? FileUrl { get; init; }
    }
}
