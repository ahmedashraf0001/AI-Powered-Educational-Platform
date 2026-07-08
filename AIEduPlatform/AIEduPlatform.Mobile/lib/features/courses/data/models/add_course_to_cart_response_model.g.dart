// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'add_course_to_cart_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

AddCourseToCartResponseModel _$AddCourseToCartResponseModelFromJson(
  Map<String, dynamic> json,
) => AddCourseToCartResponseModel(
  json['message'] as String?,
  addCourseToCartData: json['data'] == null
      ? null
      : AddCourseToCartData.fromJson(json['data'] as Map<String, dynamic>),
);

Map<String, dynamic> _$AddCourseToCartResponseModelToJson(
  AddCourseToCartResponseModel instance,
) => <String, dynamic>{
  'data': instance.addCourseToCartData,
  'message': instance.message,
};

AddCourseToCartData _$AddCourseToCartDataFromJson(Map<String, dynamic> json) =>
    AddCourseToCartData(
      cartId: json['cartId'] as String?,
      courseToCartItems: (json['items'] as List<dynamic>?)
          ?.map((e) => AddCourseToCartItems.fromJson(e as Map<String, dynamic>))
          .toList(),
      itemCount: (json['itemCount'] as num?)?.toInt(),
      subtotal: (json['subtotal'] as num?)?.toDouble(),
      currency: json['currency'] as String?,
    );

Map<String, dynamic> _$AddCourseToCartDataToJson(
  AddCourseToCartData instance,
) => <String, dynamic>{
  'cartId': instance.cartId,
  'items': instance.courseToCartItems,
  'itemCount': instance.itemCount,
  'subtotal': instance.subtotal,
  'currency': instance.currency,
};

AddCourseToCartItems _$AddCourseToCartItemsFromJson(
  Map<String, dynamic> json,
) => AddCourseToCartItems(
  cartItemId: json['cartItemId'] as String?,
  courseId: json['courseId'] as String?,
  courseTitle: json['courseTitle'] as String?,
  courseThumbnailUrl: json['courseThumbnailUrl'] as String?,
  teacherName: json['teacherName'] as String?,
  originalPrice: (json['originalPrice'] as num?)?.toInt(),
);

Map<String, dynamic> _$AddCourseToCartItemsToJson(
  AddCourseToCartItems instance,
) => <String, dynamic>{
  'cartItemId': instance.cartItemId,
  'courseId': instance.courseId,
  'courseTitle': instance.courseTitle,
  'courseThumbnailUrl': instance.courseThumbnailUrl,
  'teacherName': instance.teacherName,
  'originalPrice': instance.originalPrice,
};
