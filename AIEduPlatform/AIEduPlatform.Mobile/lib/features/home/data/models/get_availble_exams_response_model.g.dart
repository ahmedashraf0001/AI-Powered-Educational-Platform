// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'get_availble_exams_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

GetAvailbleExamsResponseModel _$GetAvailbleExamsResponseModelFromJson(
  Map<String, dynamic> json,
) => GetAvailbleExamsResponseModel(
  data: json['data'] == null
      ? null
      : AvailableExamsDataModel.fromJson(json['data'] as Map<String, dynamic>),
);

Map<String, dynamic> _$GetAvailbleExamsResponseModelToJson(
  GetAvailbleExamsResponseModel instance,
) => <String, dynamic>{'data': instance.data};

AvailableExamsDataModel _$AvailableExamsDataModelFromJson(
  Map<String, dynamic> json,
) => AvailableExamsDataModel(
  examsItemsList: (json['items'] as List<dynamic>?)
      ?.map((e) => AvailableExamsItemModel.fromJson(e as Map<String, dynamic>))
      .toList(),
  page: (json['page'] as num?)?.toInt(),
  pageSize: (json['pageSize'] as num?)?.toInt(),
  totalCount: (json['totalCount'] as num?)?.toInt(),
  totalPages: (json['totalPages'] as num?)?.toInt(),
  hasPrevious: json['hasPrevious'] as bool?,
  hasNext: json['hasNext'] as bool?,
);

Map<String, dynamic> _$AvailableExamsDataModelToJson(
  AvailableExamsDataModel instance,
) => <String, dynamic>{
  'items': instance.examsItemsList,
  'page': instance.page,
  'pageSize': instance.pageSize,
  'totalCount': instance.totalCount,
  'totalPages': instance.totalPages,
  'hasPrevious': instance.hasPrevious,
  'hasNext': instance.hasNext,
};

AvailableExamsItemModel _$AvailableExamsItemModelFromJson(
  Map<String, dynamic> json,
) => AvailableExamsItemModel(
  id: json['id'] as String?,
  courseId: json['courseId'] as String?,
  title: json['title'] as String?,
  startTime: json['startTime'] as String?,
  endTime: json['endTime'] as String?,
  durationMinutes: (json['durationMinutes'] as num?)?.toInt(),
  questionCount: (json['questionCount'] as num?)?.toInt(),
  hasSubmitted: json['hasSubmitted'] as bool?,
);

Map<String, dynamic> _$AvailableExamsItemModelToJson(
  AvailableExamsItemModel instance,
) => <String, dynamic>{
  'id': instance.id,
  'courseId': instance.courseId,
  'title': instance.title,
  'startTime': instance.startTime,
  'endTime': instance.endTime,
  'durationMinutes': instance.durationMinutes,
  'questionCount': instance.questionCount,
  'hasSubmitted': instance.hasSubmitted,
};
