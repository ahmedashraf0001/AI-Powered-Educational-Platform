using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using FastEndpoints;
using System.Text.Json;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class StartExamAttemptRequest
{
    public Guid ExamId { get; set; }
}

/// <summary>
/// Starts or resumes an exam attempt for timer persistence.
/// </summary>
public class StartExamAttemptEndpoint : Endpoint<StartExamAttemptRequest, ApiResponse<ExamAttemptDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public StartExamAttemptEndpoint(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public override void Configure()
    {
        Post("/api/exams/{ExamId}/attempt");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Start or resume exam attempt";
            s.Description = "Starts a new exam attempt or returns existing one for timer persistence.";
            s.Response<ApiResponse<ExamAttemptDto>>(200, "Exam attempt details");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not enrolled in the course");
            s.Response(404, "Exam not found");
        });
    }

    public override async Task HandleAsync(StartExamAttemptRequest req, CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            throw new UnauthorizedException("You must be logged in.");

        var exam = await _unitOfWork.Exams.GetByIdAsync(req.ExamId, ct);
        if (exam == null)
            throw new NotFoundException("Exam", req.ExamId);

        // Check enrollment
        var isEnrolled = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(userId.Value, exam.CourseId, ct);
        if (!isEnrolled)
            throw new ForbiddenException("You must be enrolled in the course.");

        // Check if already submitted
        var hasSubmitted = await _unitOfWork.Submissions.HasStudentSubmittedAsync(req.ExamId, userId.Value, ct);
        if (hasSubmitted)
            throw new BadRequestException("You have already submitted this exam.");

        // Get or create attempt
        var attempt = await _unitOfWork.ExamAttempts.GetOrCreateAttemptAsync(req.ExamId, userId.Value, ct);

        // Calculate remaining time
        var elapsed = (int)(DateTime.UtcNow - attempt.StartedAt).TotalSeconds;
        var totalDuration = exam.DurationMinutes * 60;
        var remaining = Math.Max(0, totalDuration - elapsed);

        // Parse saved answers
        Dictionary<string, string>? savedAnswers = null;
        if (!string.IsNullOrEmpty(attempt.SavedAnswers))
        {
            try
            {
                savedAnswers = JsonSerializer.Deserialize<Dictionary<string, string>>(attempt.SavedAnswers);
            }
            catch { /* ignore */ }
        }

        var dto = new ExamAttemptDto
        {
            Id = attempt.Id,
            ExamId = attempt.ExamId,
            StudentId = attempt.StudentId,
            StartedAt = attempt.StartedAt,
            IsSubmitted = attempt.IsSubmitted,
            RemainingSeconds = remaining,
            SavedAnswers = savedAnswers
        };

        await SendOkAsync(ApiResponse<ExamAttemptDto>.Ok(dto), ct);
    }
}
