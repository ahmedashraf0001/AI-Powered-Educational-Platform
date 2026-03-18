using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using FastEndpoints;
using System.Text.Json;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class SaveExamAnswersRequest
{
    public Guid ExamId { get; set; }
    public Dictionary<string, string> Answers { get; set; } = new();
}

/// <summary>
/// Saves exam answers periodically for auto-save functionality.
/// </summary>
public class SaveExamAnswersEndpoint : Endpoint<SaveExamAnswersRequest, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SaveExamAnswersEndpoint(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public override void Configure()
    {
        Put("/api/exams/{ExamId}/attempt/answers");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Save exam answers";
            s.Description = "Auto-saves exam answers for persistence.";
            s.Response<ApiResponse<bool>>(200, "Answers saved");
            s.Response(401, "Not authenticated");
            s.Response(404, "Exam attempt not found");
        });
    }

    public override async Task HandleAsync(SaveExamAnswersRequest req, CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            throw new UnauthorizedException("You must be logged in.");

        var attempt = await _unitOfWork.ExamAttempts.GetByExamAndStudentAsync(req.ExamId, userId.Value, ct);
        if (attempt == null)
            throw new NotFoundException("ExamAttempt", req.ExamId);

        if (attempt.IsSubmitted)
            throw new BadRequestException("This exam has already been submitted.");

        var answersJson = JsonSerializer.Serialize(req.Answers);
        await _unitOfWork.ExamAttempts.SaveAnswersAsync(req.ExamId, userId.Value, answersJson, ct);

        await SendOkAsync(ApiResponse<bool>.Ok(true), ct);
    }
}
