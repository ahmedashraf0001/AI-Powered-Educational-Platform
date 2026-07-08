import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/features/ai/data/repo/ai_services_repo.dart';
import 'package:graduation_app/features/ai/logic/ai_services_cubit/cubit/ai_services_state.dart';

class AiServicesCubit extends Cubit<AiServicesState> {
  final AiServicesRepo aiServicesRepo;
  AiServicesCubit(this.aiServicesRepo) : super(AiServicesState.initial());

  Future generateFlashCards(String? topic, String sessionId) async {
    emit(LoadingFlashCards());
    final data = await aiServicesRepo.generateFlashCards(topic, 10, sessionId);

    data.whenOrNull(
      success: (response) {
        emit(SuccessFlashCards(response.dataList));
      },
      failure: (errorHandler) {
        emit(FailureFlashCards(message: errorHandler.apiErrorModel.message));
      },
    );
  }

  Future summaryTopic(String? topic, String sessionId) async {
    emit(LoadingSummaryTopic());
    final data = await aiServicesRepo.summaryTopic(topic, 200, sessionId, true);
    data.whenOrNull(
      success: (response) {
        emit(SuccessSummaryTopic(response.summaryData!));
      },
      failure: (errorHandler) {
        emit(FailureSummaryTopic(message: errorHandler.apiErrorModel.message));
      },
    );
  }

  Future generateQuiz(
    String? topic,
    int numberOfQuestions,
    String difficulty,
    String sessionId,
  ) async {
    emit(LoadingGenerateQuiz());
    final data = await aiServicesRepo.generateQuiz(
      topic,
      numberOfQuestions,
      difficulty,
      sessionId,
    );
    data.when(
      success: (response) {
        emit(SuccessGenerateQuiz(response.data!));
      },
      failure: (failure) {
        emit(FailureGenerateQuiz(message: failure.apiErrorModel.message));
      },
    );
  }

  Future submitQuiz(
    String quizId,
    String sessionId,
    Map<String, String> answers,
  ) async {
    emit(LoadingSubmitQuiz());
    final data = await aiServicesRepo.submitQuizAnswers(
      quizId,
      sessionId,
      answers,
    );
    data.when(
      success: (success) {
        emit(SuccessSubmitQuiz(success.data!));
      },
      failure: (failure) {
        emit(FailureSubmitQuiz(message: failure.apiErrorModel.message));
      },
    );
  }

  Future generateMindMap(String sessionId, String topic, int maxDepth) async {
    emit(LoadingMindMap());
    final data = await aiServicesRepo.generateMindMap(
      sessionId,
      topic,
      maxDepth,
    );
    data.when(
      success: (success) {
        emit(SuccessMindMap(success.data!));
      },
      failure: (failure) {
        emit(FailureMindMap(message: failure.apiErrorModel.message));
      },
    );
  }
}
