// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'submit_quiz_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SubmitQuizResponseModel _$SubmitQuizResponseModelFromJson(
  Map<String, dynamic> json,
) => SubmitQuizResponseModel(
  data: json['data'] == null
      ? null
      : SubmitQuizResponseData.fromJson(json['data'] as Map<String, dynamic>),
);

Map<String, dynamic> _$SubmitQuizResponseModelToJson(
  SubmitQuizResponseModel instance,
) => <String, dynamic>{'data': instance.data};

SubmitQuizResponseData _$SubmitQuizResponseDataFromJson(
  Map<String, dynamic> json,
) => SubmitQuizResponseData(
  quizId: json['quizId'] as String?,
  score: (json['score'] as num?)?.toDouble(),
  totalQuestions: (json['totalQuestions'] as num?)?.toInt(),
  correctCount: (json['correctCount'] as num?)?.toInt(),
  resultsList: (json['results'] as List<dynamic>?)
      ?.map((e) => QuizResultsModel.fromJson(e as Map<String, dynamic>))
      .toList(),
);

Map<String, dynamic> _$SubmitQuizResponseDataToJson(
  SubmitQuizResponseData instance,
) => <String, dynamic>{
  'quizId': instance.quizId,
  'score': instance.score,
  'totalQuestions': instance.totalQuestions,
  'correctCount': instance.correctCount,
  'results': instance.resultsList,
};

QuizResultsModel _$QuizResultsModelFromJson(Map<String, dynamic> json) =>
    QuizResultsModel(
      questionIndex: (json['questionIndex'] as num?)?.toInt(),
      studentAnswer: json['studentAnswer'] as String?,
      correctAnswer: json['correctAnswer'] as String?,
      isCorrect: json['isCorrect'] as bool?,
      explanation: json['explanation'] as String?,
      aiScore: (json['aiScore'] as num?)?.toDouble(),
      aiFeedback: json['aiFeedback'] as String?,
    );

Map<String, dynamic> _$QuizResultsModelToJson(QuizResultsModel instance) =>
    <String, dynamic>{
      'questionIndex': instance.questionIndex,
      'studentAnswer': instance.studentAnswer,
      'correctAnswer': instance.correctAnswer,
      'isCorrect': instance.isCorrect,
      'explanation': instance.explanation,
      'aiScore': instance.aiScore,
      'aiFeedback': instance.aiFeedback,
    };
