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
        Post("/api/courses/{courseId}/lectures");
        Group<LecturesGroup>();
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
