
import 'package:freezed_annotation/freezed_annotation.dart';
part 'get_my_cart_response_model.g.dart';

@JsonSerializable()
class GetMyCartResponseModel{
  @JsonKey(name: 'data')
  final MyCartData myCartData;

  GetMyCartResponseModel({required this.myCartData});
  factory GetMyCartResponseModel.fromJson(Map<String,dynamic>json)=>_$GetMyCartResponseModelFromJson(json);
}


@JsonSerializable()
class MyCartData{
  final String? cartId;
  @JsonKey(name: 'items')
  final List<MyCartItems>? myCartItems;
  final int? itemCount;
  final double? subtotal;
  final String? currency;

  MyCartData({required this.cartId, required this.myCartItems, required this.itemCount, required this.subtotal, required this.currency});
  factory MyCartData.fromJson(Map<String,dynamic>json)=>_$MyCartDataFromJson(json);

}

@JsonSerializable()
class MyCartItems{
  final String? cartItemId;
  final String? courseId;
  final String? courseTitle;
  final String? courseThumbnailUrl;
  final String? teacherName;
  final int? originalPrice;

  MyCartItems({required this.cartItemId, required this.courseId, required this.courseTitle, required this.courseThumbnailUrl, required this.teacherName, required this.originalPrice});
  factory MyCartItems.fromJson(Map<String,dynamic>json)=>_$MyCartItemsFromJson(json);

}