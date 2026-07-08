import 'dart:convert';

import 'package:json_annotation/json_annotation.dart';
part 'get_exam_questions_response_model.g.dart';

@JsonSerializable()
class GetExamQuestionsResponseModel {
  final List<ExamQuestionDataModel>? data;

  GetExamQuestionsResponseModel({required this.data});

  factory GetExamQuestionsResponseModel.fromJson(Map<String, dynamic> json) =>
      _$GetExamQuestionsResponseModelFromJson(json);
}

@JsonSerializable()
class ExamQuestionDataModel {
  final String? id;
  final String? examId;
  final String? type;
  final String? text;
  @OptionsConverter()
  final List<String>? options;
  final String? correctAnswer;
  final int? points;
  final int? order;

  ExamQuestionDataModel({
    required this.id,
    required this.examId,
    required this.type,
    required this.text,
    required this.options,
    required this.correctAnswer,
    required this.points,
    required this.order,
  });
  factory ExamQuestionDataModel.fromJson(Map<String, dynamic> json) =>
      _$ExamQuestionDataModelFromJson(json);
}

class OptionsConverter implements JsonConverter<List<String>?, dynamic> {
  const OptionsConverter();

  @override
  List<String>? fromJson(dynamic json) {
    if (json == null) return null;
    if (json is String) {
      return List<String>.from(jsonDecode(json));
    }
    if (json is List) {
      return List<String>.from(json);
    }
    return null;
  }

  @override
  dynamic toJson(List<String>? object) {
    if (object == null) return null;
    return jsonEncode(object);
  }
}
