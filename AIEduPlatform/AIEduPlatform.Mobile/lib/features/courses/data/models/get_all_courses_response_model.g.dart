// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'get_all_courses_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

GetAllCoursesResponseModel _$GetAllCoursesResponseModelFromJson(
  Map<String, dynamic> json,
) => GetAllCoursesResponseModel(
  data: AllCoursesDataModel.fromJson(json['data'] as Map<String, dynamic>),
);

Map<String, dynamic> _$GetAllCoursesResponseModelToJson(
  GetAllCoursesResponseModel instance,
) => <String, dynamic>{'data': instance.data};

AllCoursesDataModel _$AllCoursesDataModelFromJson(Map<String, dynamic> json) =>
    AllCoursesDataModel(
      allCoursesItemModels: (json['items'] as List<dynamic>)
          .map((e) => AllCoursesItemModel.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$AllCoursesDataModelToJson(
  AllCoursesDataModel instance,
) => <String, dynamic>{'items': instance.allCoursesItemModels};

AllCoursesItemModel _$AllCoursesItemModelFromJson(Map<String, dynamic> json) =>
    AllCoursesItemModel(
      courseId: json['courseId'] as String?,
      title: json['title'] as String?,
      description: json['description'] as String?,
      teacherId: json['teacherId'] as String?,
      teacherName: json['teacherName'] as String?,
      isPublished: json['isPublished'] as bool?,
      lectureCount: (json['lectureCount'] as num?)?.toInt(),
      enrollmentCount: (json['enrollmentCount'] as num?)?.toInt(),
      isEnrolled: json['isEnrolled'] as bool?,
      averageRating: (json['averageRating'] as num?)?.toDouble(),
      reviewCount: (json['reviewCount'] as num?)?.toInt(),
      categoryId: json['categoryId'] as String?,
      categoryName: json['categoryName'] as String?,
      price: (json['price'] as num?)?.toDouble(),
      isFree: json['isFree'] as bool?,
      thumbnailUrl: json['thumbnailUrl'] as String?,
    );

Map<String, dynamic> _$AllCoursesItemModelToJson(
  AllCoursesItemModel instance,
) => <String, dynamic>{
  'courseId': instance.courseId,
  'title': instance.title,
  'description': instance.description,
  'teacherId': instance.teacherId,
  'teacherName': instance.teacherName,
  'isPublished': instance.isPublished,
  'lectureCount': instance.lectureCount,
  'enrollmentCount': instance.enrollmentCount,
  'isEnrolled': instance.isEnrolled,
  'averageRating': instance.averageRating,
  'reviewCount': instance.reviewCount,
  'categoryId': instance.categoryId,
  'categoryName': instance.categoryName,
  'price': instance.price,
  'isFree': instance.isFree,
  'thumbnailUrl': instance.thumbnailUrl,
};
