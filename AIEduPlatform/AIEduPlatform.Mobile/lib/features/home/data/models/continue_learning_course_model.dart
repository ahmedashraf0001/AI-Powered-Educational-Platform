
import 'package:freezed_annotation/freezed_annotation.dart';
part 'continue_learning_course_model.g.dart';

@JsonSerializable()
class ContinueLearningCourseModel{
  @JsonKey(name: 'data')
  final List<ContinueLearningDataModel>? coursesList;

  ContinueLearningCourseModel({required this.coursesList});
  factory ContinueLearningCourseModel.fromJson(Map<String,dynamic>json)=>_$ContinueLearningCourseModelFromJson(json);
}



@JsonSerializable()
class ContinueLearningDataModel{
  final String? courseId;
  final String? courseTitle;
  final int? progressPercentage;
  final String? lastMaterialId;
  final String? lastMaterialTitle;
  final int? resumePosition;

  ContinueLearningDataModel({required this.courseId, required this.courseTitle, required this.progressPercentage, required this.lastMaterialId, required this.lastMaterialTitle, required this.resumePosition});

  factory ContinueLearningDataModel.fromJson(Map<String,dynamic>json)=>_$ContinueLearningDataModelFromJson(json);

}