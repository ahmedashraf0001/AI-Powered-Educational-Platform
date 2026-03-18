using System.Text.Json;
using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.RAG;
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
        private readonly IRAGService _ragService;
        private readonly IOllamaServiceClient _ollamaService;
        private readonly ILogger<GenerateAIQuestionsCommandHandler> _logger;

        public GenerateAIQuestionsCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IRAGService ragService,
            IOllamaServiceClient ollamaService,
            ILogger<GenerateAIQuestionsCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ragService = ragService;
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

            try
            {
                var hasFocusTopics = request.FocusTopics != null && request.FocusTopics.Count > 0;
                var ragRequest = new RagRetrievalRequest
                {
                    Query = BuildContextQuery(request.FocusTopics, course.Title),
                    CourseId = course.Id,
                    LectureIds = request.LectureIds,
                    MaterialIds = request.MaterialIds,
                    TopK = 30,
                    FinalTopK = 15,
                    MinScore = hasFocusTopics ? 0.2f : 0.05f,
                    UseReranking = true
                };

                var ragResponse = await _ragService.RetrieveAsync(ragRequest, cancellationToken);

                if (!ragResponse.Success || ragResponse.Chunks.Count == 0)
                {
                    _logger.LogWarning(
                        "No context found for AI question generation. ExamId: {ExamId}, CourseId: {CourseId}",
                        request.ExamId,
                        course.Id);

                    return new GenerateAIQuestionsResult
                    {
                        Success = false,
                        Error = "No course content found to generate questions from. Please add materials to the course first."
                    };
                }

                _logger.LogInformation(
                    "Retrieved {ChunkCount} context chunks for AI question generation. ExamId: {ExamId}",
                    ragResponse.Chunks.Count,
                    request.ExamId);

                var questionTypes = request.QuestionTypes ?? new List<QuestionType> { QuestionType.MultipleChoice, QuestionType.TrueFalse, QuestionType.ShortAnswer };
                var questionTypeStrings = questionTypes
                    .Select(MapQuestionTypeToString)
                    .ToList();

                var generatedQuestions = await _ollamaService.GenerateExamQuestionsAsync(
                    ragResponse.Chunks,
                    request.NumberOfQuestions,
                    request.Difficulty,
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

                    var question = new Question
                    {
                        ExamId = request.ExamId,
                        Type = MapStringToQuestionType(aiQuestion.QuestionType),
                        Text = aiQuestion.QuestionText,
                        Options = aiQuestion.Options != null ? JsonSerializer.Serialize(aiQuestion.Options) : "[]",
                        CorrectAnswer = aiQuestion.CorrectAnswer,
                        Points = aiQuestion.SuggestedPoints > 0 ? aiQuestion.SuggestedPoints : GetDefaultPoints(aiQuestion.QuestionType),
                        Order = maxOrder,
                        ModelAnswer = aiQuestion.ModelAnswer,
                        GradingCriteria = aiQuestion.GradingRubric != null ? JsonSerializer.Serialize(aiQuestion.GradingRubric) : null
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

        private static string BuildContextQuery(List<string>? focusTopics, string courseTitle)
        {
            if (focusTopics != null && focusTopics.Count > 0)
            {
                return $"Key concepts and information about: {string.Join(", ", focusTopics)}";
            }

            return $"Key concepts, definitions, and important information from {courseTitle}";
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
