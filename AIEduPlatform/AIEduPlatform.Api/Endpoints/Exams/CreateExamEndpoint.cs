using AIEduPlatform.Application.Features.Exams.Commands.Exams.CreateExam;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class CreateExamRequest
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
}

public class CreateExamResponse
{
    public Guid ExamId { get; set; }
}

public class CreateExamEndpoint : Endpoint<CreateExamRequest, ApiResponse<CreateExamResponse>>
{
    private readonly IMediator _mediator;

    public CreateExamEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/courses/{CourseId}/exams");
        Roles("Teacher");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Create an exam";
            s.Description = "Creates a new exam for a course with a time window and duration. Only the course instructor can create exams.";
            s.Response<ApiResponse<CreateExamResponse>>(201, "Exam created");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(CreateExamRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateExamCommand
        {
            CourseId = req.CourseId,
            Title = req.Title,
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            DurationMinutes = req.DurationMinutes
        }, ct);
        
        await SendCreatedAtAsync<GetExamByIdEndpoint>(
            new { examId = result },
            ApiResponse<CreateExamResponse>.Ok(new CreateExamResponse { ExamId = result }, "Exam created successfully."),
            cancellation: ct);
    }
}
