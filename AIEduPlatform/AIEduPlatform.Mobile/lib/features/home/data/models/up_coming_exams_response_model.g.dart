// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'up_coming_exams_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

UpComingExamsResponseModel _$UpComingExamsResponseModelFromJson(
  Map<String, dynamic> json,
) => UpComingExamsResponseModel(
  message: json['message'] as String,
  upComingExamData: UpcomingExamsDataModel.fromJson(
    json['upComingExamData'] as Map<String, dynamic>,
  ),
);

Map<String, dynamic> _$UpComingExamsResponseModelToJson(
  UpComingExamsResponseModel instance,
) => <String, dynamic>{
  'message': instance.message,
  'upComingExamData': instance.upComingExamData,
};

UpcomingExamsDataModel _$UpcomingExamsDataModelFromJson(
  Map<String, dynamic> json,
) => UpcomingExamsDataModel(
  items: (json['items'] as List<dynamic>)
      .map((e) => UpComingExamItemModel.fromJson(e as Map<String, dynamic>))
      .toList(),
  page: (json['page'] as num).toInt(),
  pageSize: (json['pageSize'] as num).toInt(),
  totalCount: (json['totalCount'] as num).toInt(),
  totalPages: (json['totalPages'] as num).toInt(),
  hasPrevious: json['hasPrevious'] as bool,
  hasNext: json['hasNext'] as bool,
);

Map<String, dynamic> _$UpcomingExamsDataModelToJson(
  UpcomingExamsDataModel instance,
) => <String, dynamic>{
  'items': instance.items,
  'page': instance.page,
  'pageSize': instance.pageSize,
  'totalCount': instance.totalCount,
  'totalPages': instance.totalPages,
  'hasPrevious': instance.hasPrevious,
  'hasNext': instance.hasNext,
};

UpComingExamItemModel _$UpComingExamItemModelFromJson(
  Map<String, dynamic> json,
) => UpComingExamItemModel(
  id: json['id'] as String,
  courseId: json['courseId'] as String,
  title: json['title'] as String,
  startTime: DateTime.parse(json['startTime'] as String),
  endTime: DateTime.parse(json['endTime'] as String),
  durationMinutes: (json['durationMinutes'] as num).toInt(),
  questionCount: (json['questionCount'] as num).toInt(),
);

Map<String, dynamic> _$UpComingExamItemModelToJson(
  UpComingExamItemModel instance,
) => <String, dynamic>{
  'id': instance.id,
  'courseId': instance.courseId,
  'title': instance.title,
  'startTime': instance.startTime.toIso8601String(),
  'endTime': instance.endTime.toIso8601String(),
  'durationMinutes': instance.durationMinutes,
  'questionCount': instance.questionCount,
};
