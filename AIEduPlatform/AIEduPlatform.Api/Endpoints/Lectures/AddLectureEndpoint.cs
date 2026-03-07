using AIEduPlatform.Application.Features.Courses.Commands.Lectures.AddLecture;
using AIEduPlatform.Core.DTOs.Common;
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

public class AddLectureEndpoint : Endpoint<AddLectureRequest, ApiResponse<AddLectureResponse>>
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
            s.ExampleRequest = new AddLectureRequest
            {
                CourseId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Title = "Introduction to Neural Networks",
                Description = "This lecture covers the basics of neural networks, including perceptrons, activation functions, and backpropagation.",
                OrderIndex = 1
            };
            s.Response<ApiResponse<AddLectureResponse>>(201, "Lecture created");
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
            ApiResponse<AddLectureResponse>.Ok(new AddLectureResponse { LectureId = lectureId }, "Lecture created successfully."),
            cancellation: ct);
    }
}
