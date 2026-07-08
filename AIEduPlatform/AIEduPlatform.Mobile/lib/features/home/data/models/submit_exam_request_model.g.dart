// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'submit_exam_request_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SubmitExamRequestModel _$SubmitExamRequestModelFromJson(
  Map<String, dynamic> json,
) => SubmitExamRequestModel(
  answers: (json['answers'] as Map<String, dynamic>?)?.map(
    (k, e) => MapEntry(k, e as String),
  ),
);

Map<String, dynamic> _$SubmitExamRequestModelToJson(
  SubmitExamRequestModel instance,
) => <String, dynamic>{'answers': instance.answers};
