// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'generate_quiz_request_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

GenerateQuizRequestModel _$GenerateQuizRequestModelFromJson(
  Map<String, dynamic> json,
) => GenerateQuizRequestModel(
  topic: json['topic'] as String?,
  numberOfQuestions: (json['numberOfQuestions'] as num?)?.toInt(),
  difficulty: json['difficulty'] as String?,
  questionTypes: (json['questionTypes'] as List<dynamic>?)
      ?.map((e) => e as String)
      .toList(),
);

Map<String, dynamic> _$GenerateQuizRequestModelToJson(
  GenerateQuizRequestModel instance,
) => <String, dynamic>{
  'topic': instance.topic,
  'numberOfQuestions': instance.numberOfQuestions,
  'difficulty': instance.difficulty,
  'questionTypes': instance.questionTypes,
};
