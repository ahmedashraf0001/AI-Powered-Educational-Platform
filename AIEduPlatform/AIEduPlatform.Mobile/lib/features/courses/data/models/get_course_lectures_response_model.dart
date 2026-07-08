import 'package:freezed_annotation/freezed_annotation.dart';
part 'get_course_lectures_response_model.g.dart';

@JsonSerializable()
class GetCourseLecturesResponseModel{
  @JsonKey(name: 'data')
  final List<CourseLecturesData> courseLecturesDataList;

  GetCourseLecturesResponseModel({required this.courseLecturesDataList});
  factory GetCourseLecturesResponseModel.fromJson(Map<String,dynamic>json)=>_$GetCourseLecturesResponseModelFromJson(json);
}

@JsonSerializable()
class CourseLecturesData{
  final String? id;
  final String? courseId;
  final String? title;
  final String? description;
  @JsonKey(name: 'materials')
  final List<CourseLectureMaterials> courseLectureMaterials;

  CourseLecturesData({required this.id, required this.courseId, required this.title, required this.description, required this.courseLectureMaterials});
  factory CourseLecturesData.fromJson(Map<String,dynamic>json)=>_$CourseLecturesDataFromJson(json);

}

@JsonSerializable()
class CourseLectureMaterials{
  final String? id;
  final String? lectureId;
  final String? type;
  final String? title;
  final String? streamUrl;
  final bool? indexed;

  CourseLectureMaterials({required this.id, required this.lectureId, required this.type, required this.title, required this.streamUrl, required this.indexed});
  factory CourseLectureMaterials.fromJson(Map<String,dynamic>json)=>_$CourseLectureMaterialsFromJson(json);

}