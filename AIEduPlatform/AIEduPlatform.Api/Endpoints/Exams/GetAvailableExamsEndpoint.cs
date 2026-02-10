using AIEduPlatform.Application.Features.Exams.Queries.Exams.GetAvailableExamsForStudent;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class GetAvailableExamsEndpoint : EndpointWithoutRequest<List<ExamDto>>
{
    private readonly IMediator _mediator;

    public GetAvailableExamsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/available");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Get my available exams";
            s.Description = "Returns exams available to the authenticated student based on their enrolled courses.";
            s.Response<List<ExamDto>>(200, "Available exams");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAvailableExamsForStudentQuery(), ct);
        await SendOkAsync(result, ct);
    }
}
