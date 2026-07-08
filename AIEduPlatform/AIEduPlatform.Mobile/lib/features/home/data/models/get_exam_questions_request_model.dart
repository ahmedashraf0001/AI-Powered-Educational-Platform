import 'package:json_annotation/json_annotation.dart';
part 'get_exam_questions_request_model.g.dart';

@JsonSerializable()
class GetExamQuestionsRequestModel {
  final String? examId;

  GetExamQuestionsRequestModel({required this.examId});
  Map<String, dynamic> toJson() => _$GetExamQuestionsRequestModelToJson(this);
}
