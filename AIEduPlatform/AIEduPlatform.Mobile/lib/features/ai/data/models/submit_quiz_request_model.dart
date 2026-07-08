import 'package:json_annotation/json_annotation.dart';
part 'submit_quiz_request_model.g.dart';

@JsonSerializable()
class SubmitQuizRequestModel {
  final Map<String, String>? answers;

  SubmitQuizRequestModel({required this.answers});

  Map<String, dynamic> toJson() => _$SubmitQuizRequestModelToJson(this);
}
