import 'package:freezed_annotation/freezed_annotation.dart';
part 'up_coming_exams_response_model.g.dart';

@JsonSerializable()
class UpComingExamsResponseModel {
  final String message;
  final UpcomingExamsDataModel upComingExamData;

  UpComingExamsResponseModel({
    required this.message,
    required this.upComingExamData,
  });

  factory UpComingExamsResponseModel.fromJson(Map<String, dynamic> json) =>
      _$UpComingExamsResponseModelFromJson(json);
}

@JsonSerializable()
class UpcomingExamsDataModel {
  final List<UpComingExamItemModel> items;

  final int page;

  final int pageSize;

  final int totalCount;

  final int totalPages;

  final bool hasPrevious;

  final bool hasNext;

  UpcomingExamsDataModel({
    required this.items,
    required this.page,
    required this.pageSize,
    required this.totalCount,
    required this.totalPages,
    required this.hasPrevious,
    required this.hasNext,
  });

  factory UpcomingExamsDataModel.fromJson(Map<String, dynamic> json) =>
      _$UpcomingExamsDataModelFromJson(json);
}

@JsonSerializable()
class UpComingExamItemModel {
  final String id;

  final String courseId;

  final String title;

  final DateTime startTime;

  final DateTime endTime;

  final int durationMinutes;

  final int questionCount;

  UpComingExamItemModel({
    required this.id,
    required this.courseId,
    required this.title,
    required this.startTime,
    required this.endTime,
    required this.durationMinutes,
    required this.questionCount,
  });

  factory UpComingExamItemModel.fromJson(Map<String, dynamic> json) =>
      _$UpComingExamItemModelFromJson(json);
}
