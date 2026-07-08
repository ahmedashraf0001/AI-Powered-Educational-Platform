import 'package:json_annotation/json_annotation.dart';
part 'submit_exam_request_model.g.dart';

@JsonSerializable()
class SubmitExamRequestModel {
  final Map<String, String>? answers;

  SubmitExamRequestModel({required this.answers});

  Map<String, dynamic> toJson() => _$SubmitExamRequestModelToJson(this);
}
