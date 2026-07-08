// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'get_exam_questions_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

GetExamQuestionsResponseModel _$GetExamQuestionsResponseModelFromJson(
  Map<String, dynamic> json,
) => GetExamQuestionsResponseModel(
  data: (json['data'] as List<dynamic>?)
      ?.map((e) => ExamQuestionDataModel.fromJson(e as Map<String, dynamic>))
      .toList(),
);

Map<String, dynamic> _$GetExamQuestionsResponseModelToJson(
  GetExamQuestionsResponseModel instance,
) => <String, dynamic>{'data': instance.data};

ExamQuestionDataModel _$ExamQuestionDataModelFromJson(
  Map<String, dynamic> json,
) => ExamQuestionDataModel(
  id: json['id'] as String?,
  examId: json['examId'] as String?,
  type: json['type'] as String?,
  text: json['text'] as String?,
  options: const OptionsConverter().fromJson(json['options']),
  correctAnswer: json['correctAnswer'] as String?,
  points: (json['points'] as num?)?.toInt(),
  order: (json['order'] as num?)?.toInt(),
);

Map<String, dynamic> _$ExamQuestionDataModelToJson(
  ExamQuestionDataModel instance,
) => <String, dynamic>{
  'id': instance.id,
  'examId': instance.examId,
  'type': instance.type,
  'text': instance.text,
  'options': const OptionsConverter().toJson(instance.options),
  'correctAnswer': instance.correctAnswer,
  'points': instance.points,
  'order': instance.order,
};
