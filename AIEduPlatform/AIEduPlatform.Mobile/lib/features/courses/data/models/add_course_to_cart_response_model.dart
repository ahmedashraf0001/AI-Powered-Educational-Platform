import 'package:freezed_annotation/freezed_annotation.dart';
part 'add_course_to_cart_response_model.g.dart';
@JsonSerializable()
class AddCourseToCartResponseModel {
  @JsonKey(name: 'data')
  final AddCourseToCartData? addCourseToCartData;
  @JsonKey(name: 'message')
  final String ? message;


  AddCourseToCartResponseModel(this.message, {required this.addCourseToCartData});

  factory AddCourseToCartResponseModel.fromJson(Map<String,dynamic>json)=>_$AddCourseToCartResponseModelFromJson(json);
}

@JsonSerializable()
class AddCourseToCartData {
  final String? cartId;
  @JsonKey(name: 'items')
  final List<AddCourseToCartItems>? courseToCartItems;
  final int? itemCount;
  final double? subtotal;
  final String? currency;

  AddCourseToCartData({required this.cartId, required this.courseToCartItems, required this.itemCount, required this.subtotal, required this.currency});

  factory AddCourseToCartData.fromJson(Map<String,dynamic>json)=>_$AddCourseToCartDataFromJson(json);


}

@JsonSerializable()
class AddCourseToCartItems {
  final String? cartItemId;
  final String? courseId;
  final String? courseTitle;
  final String? courseThumbnailUrl;
  final String? teacherName;
  final int? originalPrice;

  AddCourseToCartItems({required this.cartItemId, required this.courseId, required this.courseTitle, required this.courseThumbnailUrl, required this.teacherName, required this.originalPrice});

  factory AddCourseToCartItems.fromJson(Map<String,dynamic>json)=>_$AddCourseToCartItemsFromJson(json);


}