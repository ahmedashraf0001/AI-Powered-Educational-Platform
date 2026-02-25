using AIEduPlatform.Application.Features.Courses.Commands.Lectures.DeleteLecture;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Lectures;

public class DeleteLectureRequest
{
    public Guid LectureId { get; set; }
}

public class DeleteLectureEndpoint : Endpoint<DeleteLectureRequest, object>
{
    private readonly IMediator _mediator;

    public DeleteLectureEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/courses/lectures/{LectureId}");
        Roles("Teacher");
        Group<LecturesGroup>();
        Summary(s =>
        {
            s.Summary = "Delete a lecture";
            s.Description = "Permanently deletes a lecture and its materials. Only the course instructor can delete it.";
            s.Response(204, "Lecture deleted");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(DeleteLectureRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteLectureCommand { LectureId = req.LectureId }, ct);
        await SendNoContentAsync(ct);
    }
}
