import 'package:freezed_annotation/freezed_annotation.dart';
part 'get_student_submissions_response_model.g.dart';

@JsonSerializable()
class GetStudentSubmissionsResponseModel {
  final PagedSubmissionsDataModel? data;

  GetStudentSubmissionsResponseModel({required this.data});

  factory GetStudentSubmissionsResponseModel.fromJson(
    Map<String, dynamic> json,
  ) => _$GetStudentSubmissionsResponseModelFromJson(json);
}

@JsonSerializable()
class PagedSubmissionsDataModel {
  final List<SubmissionDataModel>? items;
  final int? page;
  final int? pageSize;
  final int? totalCount;
  final int? totalPages;
  final bool? hasPrevious;
  final bool? hasNext;

  PagedSubmissionsDataModel({
    required this.items,
    required this.page,
    required this.pageSize,
    required this.totalCount,
    required this.totalPages,
    required this.hasPrevious,
    required this.hasNext,
  });

  factory PagedSubmissionsDataModel.fromJson(Map<String, dynamic> json) =>
      _$PagedSubmissionsDataModelFromJson(json);
}

@JsonSerializable()
class SubmissionDataModel {
  final String? id;
  final String? examId;
  final String? studentId;
  final String? examTitle;
  final String? courseName;
  final String? studentName;
  final DateTime? submittedAt;
  final bool? isGraded;
  final double? score; // null = not graded yet

  SubmissionDataModel({
    required this.id,
    required this.examId,
    required this.studentId,
    required this.examTitle,
    required this.courseName,
    required this.studentName,
    required this.submittedAt,
    required this.isGraded,
    required this.score,
  });

  factory SubmissionDataModel.fromJson(Map<String, dynamic> json) =>
      _$SubmissionDataModelFromJson(json);
}
