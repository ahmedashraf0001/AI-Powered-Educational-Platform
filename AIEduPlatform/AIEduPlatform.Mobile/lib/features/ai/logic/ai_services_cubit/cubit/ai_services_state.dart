import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:graduation_app/features/ai/data/models/flash_cards_response_model.dart';
import 'package:graduation_app/features/ai/data/models/generate_mind_map_response_model.dart';
import 'package:graduation_app/features/ai/data/models/generate_quiz_response_model.dart';
import 'package:graduation_app/features/ai/data/models/submit_quiz_response_model.dart';
import 'package:graduation_app/features/ai/data/models/summary_topic_response_model.dart';

part 'ai_services_state.freezed.dart';

@freezed
class AiServicesState with _$AiServicesState {
  //flash cards
  const factory AiServicesState.initial() = _Initial;
  const factory AiServicesState.loadingFlashCards() = LoadingFlashCards;
  const factory AiServicesState.successFlashCards(
    List<FlashCardModel> dataList,
  ) = SuccessFlashCards;
  const factory AiServicesState.failureFlashCards({String? message}) =
      FailureFlashCards;

  // summary topic
  const factory AiServicesState.loadingSummaryTopic() = LoadingSummaryTopic;
  const factory AiServicesState.successSummaryTopic(
    SummaryDataModel dataModel,
  ) = SuccessSummaryTopic;
  const factory AiServicesState.failureSummaryTopic({String? message}) =
      FailureSummaryTopic;

  //quiz
  const factory AiServicesState.loadingGenerateQuiz() = LoadingGenerateQuiz;
  const factory AiServicesState.successGenerateQuiz(QuizDataModel quizData) =
      SuccessGenerateQuiz;
  const factory AiServicesState.failureGenerateQuiz({String? message}) =
      FailureGenerateQuiz;

  //submitQuiz
  const factory AiServicesState.loadingSubmitQuiz() = LoadingSubmitQuiz;
  const factory AiServicesState.successSubmitQuiz(
    SubmitQuizResponseData submitQuizData,
  ) = SuccessSubmitQuiz;
  const factory AiServicesState.failureSubmitQuiz({String? message}) =
      FailureSubmitQuiz;

  //generate mind map
  const factory AiServicesState.loadingMindMap() = LoadingMindMap;
  const factory AiServicesState.successMindMap(MindMapDataModel mindMapData) =
      SuccessMindMap;
  const factory AiServicesState.failureMindMap({String? message}) =
      FailureMindMap;
}
