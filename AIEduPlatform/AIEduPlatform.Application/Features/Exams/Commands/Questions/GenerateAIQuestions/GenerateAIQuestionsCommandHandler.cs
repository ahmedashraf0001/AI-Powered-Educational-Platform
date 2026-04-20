using System.Text.Json;
using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.GenerateAIQuestions
{
    public class GenerateAIQuestionsCommandHandler : IRequestHandler<GenerateAIQuestionsCommand, GenerateAIQuestionsResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOllamaServiceClient _ollamaService;
        private readonly ILogger<GenerateAIQuestionsCommandHandler> _logger;

        public GenerateAIQuestionsCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOllamaServiceClient ollamaService,
            ILogger<GenerateAIQuestionsCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ollamaService = ollamaService;
            _logger = logger;
        }

        public async Task<GenerateAIQuestionsResult> Handle(GenerateAIQuestionsCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to generate questions.");
            }

            // Get exam with course for authorization
            var exam = await _unitOfWork.Exams.GetExamWithCourseAsync(request.ExamId, cancellationToken);

            if (exam == null)
            {
                _logger.LogWarning("Exam {ExamId} not found for AI question generation by user {UserId}.", request.ExamId, userId.Value);
                throw new NotFoundException(nameof(Exam), request.ExamId);
            }

            var course = exam.Course;

            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} not found for exam {ExamId}.", exam.CourseId, request.ExamId);
                throw new NotFoundException(nameof(Course), exam.CourseId);
            }

            if (course.TeacherId != userId.Value)
            {
                _logger.LogWarning("User {UserId} attempted to generate AI questions for exam {ExamId} without permission.", userId.Value, request.ExamId);
                throw new ForbiddenException("You are not authorized to generate questions for this exam.");
            }

            _logger.LogInformation(
                "Generating AI questions for exam. ExamId: {ExamId}, Count: {Count}, Difficulty: {Difficulty}, TeacherId: {TeacherId}",
                request.ExamId,
                request.NumberOfQuestions,
                request.Difficulty,
                userId.Value);

            var normalizedDifficulty = request.Difficulty.Trim().ToLowerInvariant();

            try
            {
                var materials = await _unitOfWork.Materials.GetMaterialsForRetrievalAsync(
                    course.Id,
                    request.LectureIds,
                    request.MaterialIds,
                    null,
                    cancellationToken);

                if (materials.Count == 0)
                {
                    _logger.LogWarning(
                        "No materials found for AI question generation. ExamId: {ExamId}, CourseId: {CourseId}",
                        request.ExamId,
                        course.Id);

                    return new GenerateAIQuestionsResult
                    {
                        Success = false,
                        Error = BuildNoMaterialsErrorMessage(course.Title)
                    };
                }

                _logger.LogInformation(
                    "Loading full course context for AI question generation. ExamId: {ExamId}, CourseId: {CourseId}, MaterialCount: {MaterialCount}",
                    request.ExamId,
                    course.Id,
                    materials.Count);

                var materialChunks = await _unitOfWork.Materials.GetAllChunksForRetrievalAsync(
                    course.Id,
                    request.LectureIds,
                    request.MaterialIds,
                    cancellationToken);

                if (materialChunks.Count == 0)
                {
                    _logger.LogWarning(
                        "No indexed chunks found for AI question generation. ExamId: {ExamId}, CourseId: {CourseId}",
                        request.ExamId,
                        course.Id);

                    return new GenerateAIQuestionsResult
                    {
                        Success = false,
                        Error = BuildNoIndexedContentMessage(course.Title)
                    };
                }

                var contextChunks = materialChunks
                    .Select(chunk => MapChunkToContextChunk(chunk, course))
                    .ToList();

                _logger.LogInformation(
                    "Retrieved {ChunkCount} context chunks for AI question generation. ExamId: {ExamId}",
                    contextChunks.Count,
                    request.ExamId);

                var questionTypes = request.QuestionTypes ?? new List<QuestionType> { QuestionType.MultipleChoice, QuestionType.TrueFalse, QuestionType.ShortAnswer };
                var questionTypeStrings = questionTypes
                    .Select(MapQuestionTypeToString)
                    .ToList();

                var generatedQuestions = await _ollamaService.GenerateExamQuestionsAsync(
                    contextChunks,
                    request.NumberOfQuestions,
                    normalizedDifficulty,
                    questionTypeStrings,
                    request.FocusTopics,
                    cancellationToken);

                if (generatedQuestions == null || generatedQuestions.Count == 0)
                {
                    _logger.LogWarning("AI generated no questions for exam {ExamId}.", request.ExamId);

                    return new GenerateAIQuestionsResult
                    {
                        Success = false,
                        Error = "AI was unable to generate questions from the provided content."
                    };
                }

                _logger.LogInformation(
                    "AI generated {QuestionCount} questions for exam {ExamId}.",
                    generatedQuestions.Count,
                    request.ExamId);

                var maxOrder = await _unitOfWork.Questions.GetMaxOrderForExamAsync(request.ExamId, cancellationToken);
                var questionIds = new List<Guid>();

                foreach (var aiQuestion in generatedQuestions)
                {
                    maxOrder++;

                    var correctAnswer = string.IsNullOrWhiteSpace(aiQuestion.CorrectAnswer)
                        ? aiQuestion.ExpectedAnswer ?? string.Empty
                        : aiQuestion.CorrectAnswer;

                    var question = new Question
                    {
                        ExamId = request.ExamId,
                        Type = MapStringToQuestionType(aiQuestion.QuestionType),
                        Text = aiQuestion.QuestionText,
                        Options = aiQuestion.Options != null ? JsonSerializer.Serialize(aiQuestion.Options) : "[]",
                        CorrectAnswer = correctAnswer,
                        Points = aiQuestion.SuggestedPoints > 0 ? aiQuestion.SuggestedPoints : GetDefaultPoints(aiQuestion.QuestionType),
                        Order = maxOrder,
                        ModelAnswer = aiQuestion.ModelAnswer ?? aiQuestion.ExpectedAnswer,
                        GradingCriteria = BuildGradingCriteriaJson(aiQuestion)
                    };

                    await _unitOfWork.Questions.AddAsync(question, cancellationToken);
                    questionIds.Add(question.Id);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully saved {QuestionCount} AI-generated questions for exam {ExamId}.",
                    questionIds.Count,
                    request.ExamId);

                return new GenerateAIQuestionsResult
                {
                    Success = true,
                    QuestionIds = questionIds,
                    QuestionsGenerated = questionIds.Count
                };
            }
            catch (Exception ex) when (ex is not (UnauthorizedException or ForbiddenException or NotFoundException))
            {
                _logger.LogError(
                    ex,
                    "Error generating AI questions for exam {ExamId}. TeacherId: {TeacherId}",
                    request.ExamId,
                    userId.Value);

                return new GenerateAIQuestionsResult
                {
                    Success = false,
                    Error = "An error occurred while generating questions. Please try again."
                };
            }
        }

        private static string BuildNoMaterialsErrorMessage(string courseTitle)
        {
            var displayName = string.IsNullOrWhiteSpace(courseTitle) ? "this course" : $"\"{courseTitle}\"";
            return $"No materials are assigned to {displayName} yet. Add at least one material to a lecture, then try generating questions again.";
        }

        private static string BuildNoIndexedContentMessage(string courseTitle)
        {
            var displayName = string.IsNullOrWhiteSpace(courseTitle) ? "this course" : $"\"{courseTitle}\"";
            return $"Materials are assigned to {displayName}, but no indexed content is available yet. Wait for indexing to complete, then try again.";
        }

        private static ContextChunk MapChunkToContextChunk(MaterialChunk chunk, Course course)
        {
            var material = chunk.Material;
            var lecture = material?.Lecture;

            return new ContextChunk
            {
                Content = chunk.Content,
                AdditionalData = chunk.AdditionalData,
                RelevanceScore = 1.0f,
                Metadata = new ChunkMetadata
                {
                    MaterialId = chunk.MaterialId,
                    CourseId = course.Id,
                    LectureId = material?.LectureId ?? Guid.Empty,
                    SourceTitle = material?.Title ?? string.Empty,
                    MaterialType = material?.Type.ToString() ?? string.Empty,
                    Section = chunk.Section ?? string.Empty,
                    PageOrTimestamp = chunk.PageOrTimestamp ?? string.Empty,
                    CourseName = chunk.CourseName ?? course.Title,
                    LectureName = chunk.LectureName ?? lecture?.Title ?? string.Empty
                }
            };
        }

        private static string? BuildGradingCriteriaJson(ExamQuestion aiQuestion)
        {
            if (aiQuestion.GradingRubric != null && aiQuestion.GradingRubric.Count > 0)
            {
                return JsonSerializer.Serialize(aiQuestion.GradingRubric);
            }

            if (string.IsNullOrWhiteSpace(aiQuestion.GradingCriteria))
            {
                return null;
            }

            var criteria = aiQuestion.GradingCriteria.Trim();

            if (IsValidJson(criteria))
            {
                return criteria;
            }

            // jsonb column requires valid JSON; serialize plain text criteria as a JSON string literal.
            return JsonSerializer.Serialize(criteria);
        }

        private static bool IsValidJson(string value)
        {
            try
            {
                using var _ = JsonDocument.Parse(value);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static string MapQuestionTypeToString(QuestionType type)
        {
            return type switch
            {
                QuestionType.MultipleChoice => "mcq",
                QuestionType.TrueFalse => "true_false",
                QuestionType.ShortAnswer => "short_answer",
                QuestionType.Essay => "essay",
                QuestionType.FillInTheBlank => "fill_blank",
                _ => "mcq"
            };
        }

        private static QuestionType MapStringToQuestionType(string type)
        {
            return type.ToLowerInvariant() switch
            {
                "mcq" or "multiple_choice" => QuestionType.MultipleChoice,
                "true_false" => QuestionType.TrueFalse,
                "short_answer" => QuestionType.ShortAnswer,
                "essay" => QuestionType.Essay,
                "fill_blank" or "fill_in_the_blank" => QuestionType.FillInTheBlank,
                _ => QuestionType.MultipleChoice
            };
        }

        private static int GetDefaultPoints(string questionType)
        {
            return questionType.ToLowerInvariant() switch
            {
                "essay" => 10,
                "short_answer" => 5,
                _ => 1
            };
        }
    }
}
