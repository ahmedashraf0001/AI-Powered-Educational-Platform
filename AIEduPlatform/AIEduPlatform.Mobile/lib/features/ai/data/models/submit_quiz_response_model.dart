import 'package:json_annotation/json_annotation.dart';
part 'submit_quiz_response_model.g.dart';

@JsonSerializable()
class SubmitQuizResponseModel {
  final SubmitQuizResponseData? data;

  SubmitQuizResponseModel({required this.data});

  factory SubmitQuizResponseModel.fromJson(Map<String, dynamic> json) =>
      _$SubmitQuizResponseModelFromJson(json);
}

@JsonSerializable()
class SubmitQuizResponseData {
  final String? quizId;
  final double? score;
  final int? totalQuestions;
  final int? correctCount;
  @JsonKey(name: 'results')
  final List<QuizResultsModel>? resultsList;

  SubmitQuizResponseData({
    required this.quizId,
    required this.score,
    required this.totalQuestions,
    required this.correctCount,
    required this.resultsList,
  });

  factory SubmitQuizResponseData.fromJson(Map<String, dynamic> json) =>
      _$SubmitQuizResponseDataFromJson(json);
}

@JsonSerializable()
class QuizResultsModel {
  final int? questionIndex;
  final String? studentAnswer;
  final String? correctAnswer;
  final bool? isCorrect;
  final String? explanation;
  final double? aiScore;
  final String? aiFeedback;

  QuizResultsModel({
    required this.questionIndex,
    required this.studentAnswer,
    required this.correctAnswer,
    required this.isCorrect,
    required this.explanation,
    required this.aiScore,
    required this.aiFeedback,
  });

  factory QuizResultsModel.fromJson(Map<String, dynamic> json) =>
      _$QuizResultsModelFromJson(json);
}
