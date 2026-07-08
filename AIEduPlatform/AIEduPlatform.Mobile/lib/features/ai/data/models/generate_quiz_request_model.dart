import 'package:freezed_annotation/freezed_annotation.dart';

part 'generate_quiz_request_model.g.dart';

@JsonSerializable()
class GenerateQuizRequestModel {
  final String? topic;
  final int? numberOfQuestions;
  final String? difficulty;
  final List<String>? questionTypes;

  GenerateQuizRequestModel({
    required this.topic,
    required this.numberOfQuestions,
    required this.difficulty,
    required this.questionTypes,
  });

  Map<String, dynamic> toJson() => _$GenerateQuizRequestModelToJson(this);
}
