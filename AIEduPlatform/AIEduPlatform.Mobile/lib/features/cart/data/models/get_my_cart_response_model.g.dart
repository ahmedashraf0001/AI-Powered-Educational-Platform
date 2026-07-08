// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'get_my_cart_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

GetMyCartResponseModel _$GetMyCartResponseModelFromJson(
  Map<String, dynamic> json,
) => GetMyCartResponseModel(
  myCartData: MyCartData.fromJson(json['data'] as Map<String, dynamic>),
);

Map<String, dynamic> _$GetMyCartResponseModelToJson(
  GetMyCartResponseModel instance,
) => <String, dynamic>{'data': instance.myCartData};

MyCartData _$MyCartDataFromJson(Map<String, dynamic> json) => MyCartData(
  cartId: json['cartId'] as String?,
  myCartItems: (json['items'] as List<dynamic>?)
      ?.map((e) => MyCartItems.fromJson(e as Map<String, dynamic>))
      .toList(),
  itemCount: (json['itemCount'] as num?)?.toInt(),
  subtotal: (json['subtotal'] as num?)?.toDouble(),
  currency: json['currency'] as String?,
);

Map<String, dynamic> _$MyCartDataToJson(MyCartData instance) =>
    <String, dynamic>{
      'cartId': instance.cartId,
      'items': instance.myCartItems,
      'itemCount': instance.itemCount,
      'subtotal': instance.subtotal,
      'currency': instance.currency,
    };

MyCartItems _$MyCartItemsFromJson(Map<String, dynamic> json) => MyCartItems(
  cartItemId: json['cartItemId'] as String?,
  courseId: json['courseId'] as String?,
  courseTitle: json['courseTitle'] as String?,
  courseThumbnailUrl: json['courseThumbnailUrl'] as String?,
  teacherName: json['teacherName'] as String?,
  originalPrice: (json['originalPrice'] as num?)?.toInt(),
);

Map<String, dynamic> _$MyCartItemsToJson(MyCartItems instance) =>
    <String, dynamic>{
      'cartItemId': instance.cartItemId,
      'courseId': instance.courseId,
      'courseTitle': instance.courseTitle,
      'courseThumbnailUrl': instance.courseThumbnailUrl,
      'teacherName': instance.teacherName,
      'originalPrice': instance.originalPrice,
    };
