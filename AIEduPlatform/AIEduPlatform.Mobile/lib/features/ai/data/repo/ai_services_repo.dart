import 'package:graduation_app/core/networking/api_error_handler.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/core/networking/api_service.dart';
import 'package:graduation_app/features/ai/data/models/flash_cards_request_model.dart';
import 'package:graduation_app/features/ai/data/models/flash_cards_response_model.dart';
import 'package:graduation_app/features/ai/data/models/generate_mind_map_request_model.dart';
import 'package:graduation_app/features/ai/data/models/generate_mind_map_response_model.dart';
import 'package:graduation_app/features/ai/data/models/generate_quiz_request_model.dart';
import 'package:graduation_app/features/ai/data/models/generate_quiz_response_model.dart';
import 'package:graduation_app/features/ai/data/models/submit_quiz_request_model.dart';
import 'package:graduation_app/features/ai/data/models/submit_quiz_response_model.dart';
import 'package:graduation_app/features/ai/data/models/summary_topic_request_model.dart';
import 'package:graduation_app/features/ai/data/models/summary_topic_response_model.dart';

class AiServicesRepo {
  final ApiService apiService;

  AiServicesRepo({required this.apiService});

  Future<ApiResult<FlashCardsResponseModel>> generateFlashCards(
    String? topic,
    int numberOfCards,
    String sessionId,
  ) async {
    try {
      final response = await apiService.generateFlashCards(
        FlashCardsRequestModel(topic: topic, numberOfCards: numberOfCards),
        sessionId,
      );
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<SummaryTopicResponseModel>> summaryTopic(
    String? topic,
    int summaryLength,
    String sessionId,
    bool includeKeyPoints,
  ) async {
    try {
      final response = await apiService.summaryTopic(
        SummaryTopicRequestModel(
          topic: topic,
          summaryLength: summaryLength,
          includeKeyPoints: includeKeyPoints,
        ),
        sessionId,
      );
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<GenerateQuizResponseModel>> generateQuiz(
    String? topic,
    int numberOfQuestions,
    String difficulty,
    String sessionId,
  ) async {
    try {
      final response = await apiService.generateQuiz(
        GenerateQuizRequestModel(
          topic: topic,
          numberOfQuestions: numberOfQuestions,
          difficulty: difficulty,
          questionTypes: ["mcq", "true_false"],
        ),
        sessionId,
      );
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<SubmitQuizResponseModel>> submitQuizAnswers(
    String quizId,
    String sessionId,
    Map<String, String> answers,
  ) async {
    try {
      final response = await apiService.submitQuizAnswers(
        SubmitQuizRequestModel(answers: answers),
        sessionId,
        quizId,
      );
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<CreateMindMapResponseModel>> generateMindMap(
    String sessionId,
    String topic,
    int maxDepth,
  ) async {
    try {
      final response = await apiService.generateMindMap(
        GenerateMindMapRequestModel(
          centralTopic: topic,
          maxDepth: maxDepth,
          sessionId: sessionId,
        ),
        sessionId,
      );
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }
}
