using AIEduPlatform.Application.Features.Courses.Commands.Lectures.UpdateLecture;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Lectures;

public class UpdateLectureRequest
{
    public Guid LectureId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}

public class UpdateLectureEndpoint : Endpoint<UpdateLectureRequest, object>
{
    private readonly IMediator _mediator;

    public UpdateLectureEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/courses/lectures/{LectureId}");
        Roles("Teacher");
        Group<LecturesGroup>();
        Summary(s =>
        {
            s.Summary = "Update a lecture";
            s.Description = "Updates the title, description, and order of a lecture. Only the course instructor can update it.";
            s.ExampleRequest = new UpdateLectureRequest
            {
                LectureId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Title = "Deep Dive into Convolutional Neural Networks",
                Description = "Updated lecture covering CNNs, pooling layers, and image classification architectures.",
                OrderIndex = 2
            };
            s.Response(204, "Lecture updated");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(UpdateLectureRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateLectureCommand
        {
            LectureId = req.LectureId,
            Title = req.Title,
            Description = req.Description,
            OrderIndex = req.OrderIndex
        }, ct);

        await SendNoContentAsync(ct);
    }
}
