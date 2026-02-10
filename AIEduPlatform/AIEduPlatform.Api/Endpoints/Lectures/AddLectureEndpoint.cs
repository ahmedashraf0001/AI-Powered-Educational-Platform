using AIEduPlatform.Application.Features.Courses.Commands.Lectures.AddLecture;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Lectures;

public class AddLectureRequest
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}

public class AddLectureResponse
{
    public Guid LectureId { get; set; }
}

public class AddLectureEndpoint : Endpoint<AddLectureRequest, AddLectureResponse>
{
    private readonly IMediator _mediator;

    public AddLectureEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/courses/{CourseId}/lectures");
        Roles("Teacher");
        Group<LecturesGroup>();
        Summary(s =>
        {
            s.Summary = "Add a lecture to a course";
            s.Description = "Creates a new lecture in the specified course. Only the course instructor can add lectures.";
            s.Response<AddLectureResponse>(201, "Lecture created");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(AddLectureRequest req, CancellationToken ct)
    {
        var lectureId = await _mediator.Send(new AddLectureCommand
        {
            CourseId = req.CourseId,
            Title = req.Title,
            Description = req.Description,
            OrderIndex = req.OrderIndex
        }, ct);
        
        await SendCreatedAtAsync<GetCourseLecturesEndpoint>(
            new { courseId = req.CourseId },
            new AddLectureResponse { LectureId = lectureId },
            cancellation: ct);
    }
}
