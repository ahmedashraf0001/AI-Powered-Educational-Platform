// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'submit_quiz_request_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SubmitQuizRequestModel _$SubmitQuizRequestModelFromJson(
  Map<String, dynamic> json,
) => SubmitQuizRequestModel(
  answers: (json['answers'] as Map<String, dynamic>?)?.map(
    (k, e) => MapEntry(k, e as String),
  ),
);

Map<String, dynamic> _$SubmitQuizRequestModelToJson(
  SubmitQuizRequestModel instance,
) => <String, dynamic>{'answers': instance.answers};
