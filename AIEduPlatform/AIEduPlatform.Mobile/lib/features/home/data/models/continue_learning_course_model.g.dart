// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'continue_learning_course_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

ContinueLearningCourseModel _$ContinueLearningCourseModelFromJson(
  Map<String, dynamic> json,
) => ContinueLearningCourseModel(
  coursesList: (json['data'] as List<dynamic>?)
      ?.map(
        (e) => ContinueLearningDataModel.fromJson(e as Map<String, dynamic>),
      )
      .toList(),
);

Map<String, dynamic> _$ContinueLearningCourseModelToJson(
  ContinueLearningCourseModel instance,
) => <String, dynamic>{'data': instance.coursesList};

ContinueLearningDataModel _$ContinueLearningDataModelFromJson(
  Map<String, dynamic> json,
) => ContinueLearningDataModel(
  courseId: json['courseId'] as String?,
  courseTitle: json['courseTitle'] as String?,
  progressPercentage: (json['progressPercentage'] as num?)?.toInt(),
  lastMaterialId: json['lastMaterialId'] as String?,
  lastMaterialTitle: json['lastMaterialTitle'] as String?,
  resumePosition: (json['resumePosition'] as num?)?.toInt(),
);

Map<String, dynamic> _$ContinueLearningDataModelToJson(
  ContinueLearningDataModel instance,
) => <String, dynamic>{
  'courseId': instance.courseId,
  'courseTitle': instance.courseTitle,
  'progressPercentage': instance.progressPercentage,
  'lastMaterialId': instance.lastMaterialId,
  'lastMaterialTitle': instance.lastMaterialTitle,
  'resumePosition': instance.resumePosition,
};
