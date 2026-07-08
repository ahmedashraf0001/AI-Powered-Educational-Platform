// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'get_course_lectures_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

GetCourseLecturesResponseModel _$GetCourseLecturesResponseModelFromJson(
  Map<String, dynamic> json,
) => GetCourseLecturesResponseModel(
  courseLecturesDataList: (json['data'] as List<dynamic>)
      .map((e) => CourseLecturesData.fromJson(e as Map<String, dynamic>))
      .toList(),
);

Map<String, dynamic> _$GetCourseLecturesResponseModelToJson(
  GetCourseLecturesResponseModel instance,
) => <String, dynamic>{'data': instance.courseLecturesDataList};

CourseLecturesData _$CourseLecturesDataFromJson(Map<String, dynamic> json) =>
    CourseLecturesData(
      id: json['id'] as String?,
      courseId: json['courseId'] as String?,
      title: json['title'] as String?,
      description: json['description'] as String?,
      courseLectureMaterials: (json['materials'] as List<dynamic>)
          .map(
            (e) => CourseLectureMaterials.fromJson(e as Map<String, dynamic>),
          )
          .toList(),
    );

Map<String, dynamic> _$CourseLecturesDataToJson(CourseLecturesData instance) =>
    <String, dynamic>{
      'id': instance.id,
      'courseId': instance.courseId,
      'title': instance.title,
      'description': instance.description,
      'materials': instance.courseLectureMaterials,
    };

CourseLectureMaterials _$CourseLectureMaterialsFromJson(
  Map<String, dynamic> json,
) => CourseLectureMaterials(
  id: json['id'] as String?,
  lectureId: json['lectureId'] as String?,
  type: json['type'] as String?,
  title: json['title'] as String?,
  streamUrl: json['streamUrl'] as String?,
  indexed: json['indexed'] as bool?,
);

Map<String, dynamic> _$CourseLectureMaterialsToJson(
  CourseLectureMaterials instance,
) => <String, dynamic>{
  'id': instance.id,
  'lectureId': instance.lectureId,
  'type': instance.type,
  'title': instance.title,
  'streamUrl': instance.streamUrl,
  'indexed': instance.indexed,
};
