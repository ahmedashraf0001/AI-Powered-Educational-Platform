import 'dart:convert';

import 'package:json_annotation/json_annotation.dart';

part 'generate_quiz_response_model.g.dart';

@JsonSerializable(explicitToJson: true)
class GenerateQuizResponseModel {
  final QuizDataModel? data;

  GenerateQuizResponseModel({this.data});

  factory GenerateQuizResponseModel.fromJson(Map<String, dynamic> json) =>
      _$GenerateQuizResponseModelFromJson(json);

  Map<String, dynamic> toJson() => _$GenerateQuizResponseModelToJson(this);
}

@JsonSerializable(explicitToJson: true)
class QuizDataModel {
  final List<QuizItemModel>? items;

  QuizDataModel({this.items});

  factory QuizDataModel.fromJson(Map<String, dynamic> json) =>
      _$QuizDataModelFromJson(json);

  Map<String, dynamic> toJson() => _$QuizDataModelToJson(this);
}

@JsonSerializable(explicitToJson: true)
class QuizItemModel {
  final String? id;
  final String? topic;
  final String? difficulty;

  @QuestionsConverter()
  final List<QuizQuestionModel>? questions;

  @StudentAnswersConverter()
  final Map<String, dynamic>? studentAnswers;
  final int? score;
  final DateTime? createdAt;

  QuizItemModel({
    this.id,
    this.topic,
    this.difficulty,
    this.questions,
    this.studentAnswers,
    this.score,
    this.createdAt,
  });

  factory QuizItemModel.fromJson(Map<String, dynamic> json) =>
      _$QuizItemModelFromJson(json);

  Map<String, dynamic> toJson() => _$QuizItemModelToJson(this);
}

@JsonSerializable()
class QuizQuestionModel {
  final List<String>? options;
  final String? difficulty;
  final String? explanation;
  final String? sourceTitle;
  final String? questionText;
  final String? questionType;
  final String? correctAnswer;
  final String? sourceLocation;
  final int? suggestedPoints;

  QuizQuestionModel({
    this.options,
    this.difficulty,
    this.explanation,
    this.sourceTitle,
    this.questionText,
    this.questionType,
    this.correctAnswer,
    this.sourceLocation,
    this.suggestedPoints,
  });

  factory QuizQuestionModel.fromJson(Map<String, dynamic> json) =>
      _$QuizQuestionModelFromJson(json);

  Map<String, dynamic> toJson() => _$QuizQuestionModelToJson(this);
}

class QuestionsConverter
    implements JsonConverter<List<QuizQuestionModel>?, dynamic> {
  const QuestionsConverter();

  @override
  List<QuizQuestionModel>? fromJson(dynamic json) {
    if (json == null) return null;

    if (json is String) {
      final decoded = jsonDecode(json) as List;
      return decoded
          .map((e) => QuizQuestionModel.fromJson(e as Map<String, dynamic>))
          .toList();
    }

    if (json is List) {
      return json
          .map((e) => QuizQuestionModel.fromJson(e as Map<String, dynamic>))
          .toList();
    }

    return null;
  }

  @override
  dynamic toJson(List<QuizQuestionModel>? object) {
    if (object == null) return null;

    return jsonEncode(object.map((e) => e.toJson()).toList());
  }
}

class StudentAnswersConverter
    implements JsonConverter<Map<String, dynamic>?, dynamic> {
  const StudentAnswersConverter();

  @override
  Map<String, dynamic>? fromJson(dynamic json) {
    if (json == null) return null;
    if (json is String) {
      return jsonDecode(json) as Map<String, dynamic>;
    }
    if (json is Map<String, dynamic>) return json;
    return null;
  }

  @override
  dynamic toJson(Map<String, dynamic>? object) {
    if (object == null) return null;
    return jsonEncode(object);
  }
}
