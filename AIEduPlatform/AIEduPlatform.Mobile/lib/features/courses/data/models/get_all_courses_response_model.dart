
import 'package:freezed_annotation/freezed_annotation.dart';
part 'get_all_courses_response_model.g.dart';

@JsonSerializable()
class GetAllCoursesResponseModel{
  @JsonKey(name: 'data')
  final  AllCoursesDataModel  data;

  GetAllCoursesResponseModel({required this.data});

  factory GetAllCoursesResponseModel.fromJson(Map<String,dynamic>json)=>_$GetAllCoursesResponseModelFromJson(json);
}


@JsonSerializable()
class AllCoursesDataModel{
  @JsonKey(name: 'items')
  final List<AllCoursesItemModel> allCoursesItemModels;

  AllCoursesDataModel({required this.allCoursesItemModels});

  factory AllCoursesDataModel.fromJson(Map<String,dynamic>json)=>_$AllCoursesDataModelFromJson(json);

}


@JsonSerializable()
class AllCoursesItemModel{
  final String? courseId;
  final String? title;
  final String? description;
  final String? teacherId;
  final String? teacherName;
  final bool? isPublished;
  final int? lectureCount;
  final int? enrollmentCount;
  final bool? isEnrolled;
  final double? averageRating;
  final int? reviewCount;
  final String? categoryId;
  final String? categoryName;
  final double? price;
  final bool? isFree;
  final String? thumbnailUrl;


  factory AllCoursesItemModel.fromJson(Map<String,dynamic>json)=>_$AllCoursesItemModelFromJson(json);

  AllCoursesItemModel({required this.courseId, required this.title, required this.description, required this.teacherId, required this.teacherName, required this.isPublished, required this.lectureCount, required this.enrollmentCount, required this.isEnrolled, required this.averageRating, required this.reviewCount, required this.categoryId, required this.categoryName, required this.price, required this.isFree, required this.thumbnailUrl});

}

