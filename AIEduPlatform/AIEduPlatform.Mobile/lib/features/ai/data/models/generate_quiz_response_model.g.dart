// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'generate_quiz_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

GenerateQuizResponseModel _$GenerateQuizResponseModelFromJson(
  Map<String, dynamic> json,
) => GenerateQuizResponseModel(
  data: json['data'] == null
      ? null
      : QuizDataModel.fromJson(json['data'] as Map<String, dynamic>),
);

Map<String, dynamic> _$GenerateQuizResponseModelToJson(
  GenerateQuizResponseModel instance,
) => <String, dynamic>{'data': instance.data?.toJson()};

QuizDataModel _$QuizDataModelFromJson(Map<String, dynamic> json) =>
    QuizDataModel(
      items: (json['items'] as List<dynamic>?)
          ?.map((e) => QuizItemModel.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$QuizDataModelToJson(QuizDataModel instance) =>
    <String, dynamic>{'items': instance.items?.map((e) => e.toJson()).toList()};

QuizItemModel _$QuizItemModelFromJson(Map<String, dynamic> json) =>
    QuizItemModel(
      id: json['id'] as String?,
      topic: json['topic'] as String?,
      difficulty: json['difficulty'] as String?,
      questions: const QuestionsConverter().fromJson(json['questions']),
      studentAnswers: const StudentAnswersConverter().fromJson(
        json['studentAnswers'],
      ),
      score: (json['score'] as num?)?.toInt(),
      createdAt: json['createdAt'] == null
          ? null
          : DateTime.parse(json['createdAt'] as String),
    );

Map<String, dynamic> _$QuizItemModelToJson(QuizItemModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'topic': instance.topic,
      'difficulty': instance.difficulty,
      'questions': const QuestionsConverter().toJson(instance.questions),
      'studentAnswers': const StudentAnswersConverter().toJson(
        instance.studentAnswers,
      ),
      'score': instance.score,
      'createdAt': instance.createdAt?.toIso8601String(),
    };

QuizQuestionModel _$QuizQuestionModelFromJson(Map<String, dynamic> json) =>
    QuizQuestionModel(
      options: (json['options'] as List<dynamic>?)
          ?.map((e) => e as String)
          .toList(),
      difficulty: json['difficulty'] as String?,
      explanation: json['explanation'] as String?,
      sourceTitle: json['sourceTitle'] as String?,
      questionText: json['questionText'] as String?,
      questionType: json['questionType'] as String?,
      correctAnswer: json['correctAnswer'] as String?,
      sourceLocation: json['sourceLocation'] as String?,
      suggestedPoints: (json['suggestedPoints'] as num?)?.toInt(),
    );

Map<String, dynamic> _$QuizQuestionModelToJson(QuizQuestionModel instance) =>
    <String, dynamic>{
      'options': instance.options,
      'difficulty': instance.difficulty,
      'explanation': instance.explanation,
      'sourceTitle': instance.sourceTitle,
      'questionText': instance.questionText,
      'questionType': instance.questionType,
      'correctAnswer': instance.correctAnswer,
      'sourceLocation': instance.sourceLocation,
      'suggestedPoints': instance.suggestedPoints,
    };
