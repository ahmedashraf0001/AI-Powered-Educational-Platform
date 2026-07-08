
import 'package:freezed_annotation/freezed_annotation.dart';
part 'remove_course_from_cart_response_model.g.dart';

@JsonSerializable()
class RemoveCourseFromCartResponseModel{
  @JsonKey(name: 'data')
  final RemoveCourseFromCartData removeCourseFromCartData;
  final String? message;

  RemoveCourseFromCartResponseModel(this.message, {required this.removeCourseFromCartData});
  factory RemoveCourseFromCartResponseModel.fromJson(Map<String,dynamic>json)=>_$RemoveCourseFromCartResponseModelFromJson(json);
}

@JsonSerializable()
class RemoveCourseFromCartData{
  final String? cartId;
  final int? itemCount;
  final double? subtotal;
  @JsonKey(name: 'items')
  final List<RemoveCourseFromCartItems>? removeCourseItems;

  RemoveCourseFromCartData({required this.cartId, required this.itemCount, required this.subtotal, required this.removeCourseItems});

  factory RemoveCourseFromCartData.fromJson(Map<String,dynamic>json)=>_$RemoveCourseFromCartDataFromJson(json);

}

@JsonSerializable()
class RemoveCourseFromCartItems{
  final String ? cartItemId;
  final String ? courseId;
  final String ? courseTitle;
  final String ? courseThumbnailUrl;
  final String ? teacherName;
  final int ? originalPrice;

  RemoveCourseFromCartItems({required this.cartItemId, required this.courseId, required this.courseTitle, required this.courseThumbnailUrl, required this.teacherName, required this.originalPrice});

  factory RemoveCourseFromCartItems.fromJson(Map<String,dynamic>json)=>_$RemoveCourseFromCartItemsFromJson(json);

}