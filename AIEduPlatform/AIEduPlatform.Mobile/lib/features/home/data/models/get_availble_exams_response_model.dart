import 'package:freezed_annotation/freezed_annotation.dart';
part 'get_availble_exams_response_model.g.dart';

@JsonSerializable()
class GetAvailbleExamsResponseModel {
  final AvailableExamsDataModel? data;

  GetAvailbleExamsResponseModel({required this.data});

  factory GetAvailbleExamsResponseModel.fromJson(Map<String, dynamic> json) =>
      _$GetAvailbleExamsResponseModelFromJson(json);
}

@JsonSerializable()
class AvailableExamsDataModel {
  @JsonKey(name: 'items')
  final List<AvailableExamsItemModel>? examsItemsList;
  final int? page;
  final int? pageSize;
  final int? totalCount;
  final int? totalPages;
  final bool? hasPrevious;
  final bool? hasNext;

  AvailableExamsDataModel({
    required this.examsItemsList,
    required this.page,
    required this.pageSize,
    required this.totalCount,
    required this.totalPages,
    required this.hasPrevious,
    required this.hasNext,
  });

  factory AvailableExamsDataModel.fromJson(Map<String, dynamic> json) =>
      _$AvailableExamsDataModelFromJson(json);
}

@JsonSerializable()
class AvailableExamsItemModel {
  final String? id;
  final String? courseId;
  final String? title;
  final String? startTime;
  final String? endTime;
  final int? durationMinutes;
  final int? questionCount;
  final bool? hasSubmitted;

  AvailableExamsItemModel({
    required this.id,
    required this.courseId,
    required this.title,
    required this.startTime,
    required this.endTime,
    required this.durationMinutes,
    required this.questionCount,
    required this.hasSubmitted,
  });

  factory AvailableExamsItemModel.fromJson(Map<String, dynamic> json) =>
      _$AvailableExamsItemModelFromJson(json);
}
