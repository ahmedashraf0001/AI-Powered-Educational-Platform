import 'package:freezed_annotation/freezed_annotation.dart';
part 'add_course_to_cart_request_model.g.dart';

@JsonSerializable()
class AddCourseToCartRequestModel {
  final String courseId;

  AddCourseToCartRequestModel({required this.courseId});

  Map<String, dynamic> toJson() => _$AddCourseToCartRequestModelToJson(this);
}
