import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:graduation_app/features/home/data/models/get_exam_questions_response_model.dart';
part 'submit_exam_response_model.g.dart';

@JsonSerializable()
class SubmitExamResponseModel {
  final SubmitExamDataModel? data;

  SubmitExamResponseModel({required this.data});

  factory SubmitExamResponseModel.fromJson(Map<String, dynamic> json) =>
      _$SubmitExamResponseModelFromJson(json);
}

@JsonSerializable()
class SubmitExamDataModel {
  final String? submissionId;

  SubmitExamDataModel({required this.submissionId});

  factory SubmitExamDataModel.fromJson(Map<String, dynamic> json) =>
      _$SubmitExamDataModelFromJson(json);
}

@JsonSerializable()
class SubmissionDetailResponseModel {
  final SubmissionDetailDataModel? data;

  SubmissionDetailResponseModel({required this.data});

  factory SubmissionDetailResponseModel.fromJson(Map<String, dynamic> json) =>
      _$SubmissionDetailResponseModelFromJson(json);
}

@JsonSerializable()
class SubmissionDetailDataModel {
  final String? id;
  final String? examId;
  final String? studentId;
  final String? examTitle;
  final String? courseName;
  final String? studentName;
  final List<SubmissionAnswerModel>? answers;
  final DateTime? submittedAt;
  final GradeModel? grade; // null = not graded yet

  factory SubmissionDetailDataModel.fromJson(Map<String, dynamic> json) =>
      _$SubmissionDetailDataModelFromJson(json);

  SubmissionDetailDataModel({
    required this.id,
    required this.examId,
    required this.studentId,
    required this.examTitle,
    required this.courseName,
    required this.studentName,
    required this.answers,
    required this.submittedAt,
    required this.grade,
  });
}

@JsonSerializable()
class SubmissionAnswerModel {
  final String? questionId;
  final String? questionText;
  final String? questionType;
  final String? answer;
  final String? correctAnswer;
  @OptionsConverter()
  final List<String>? options;
  final int? points;
  final int? order;

  factory SubmissionAnswerModel.fromJson(Map<String, dynamic> json) =>
      _$SubmissionAnswerModelFromJson(json);

  SubmissionAnswerModel({
    required this.questionId,
    required this.questionText,
    required this.questionType,
    required this.answer,
    required this.correctAnswer,
    required this.options,
    required this.points,
    required this.order,
  });
}

@JsonSerializable()
class GradeModel {
  final String? id;
  final String? submissionId;
  final double? score;
  final String? feedback;
  final bool? isAiGraded;
  final bool? isApproved;
  final String? examTitle;
  final String? courseTitle;

  factory GradeModel.fromJson(Map<String, dynamic> json) =>
      _$GradeModelFromJson(json);

  GradeModel({
    required this.id,
    required this.submissionId,
    required this.score,
    required this.feedback,
    required this.isAiGraded,
    required this.isApproved,
    required this.examTitle,
    required this.courseTitle,
  });
}
