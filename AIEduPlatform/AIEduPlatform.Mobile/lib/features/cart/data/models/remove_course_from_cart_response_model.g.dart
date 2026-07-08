// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'remove_course_from_cart_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

RemoveCourseFromCartResponseModel _$RemoveCourseFromCartResponseModelFromJson(
  Map<String, dynamic> json,
) => RemoveCourseFromCartResponseModel(
  json['message'] as String?,
  removeCourseFromCartData: RemoveCourseFromCartData.fromJson(
    json['data'] as Map<String, dynamic>,
  ),
);

Map<String, dynamic> _$RemoveCourseFromCartResponseModelToJson(
  RemoveCourseFromCartResponseModel instance,
) => <String, dynamic>{
  'data': instance.removeCourseFromCartData,
  'message': instance.message,
};

RemoveCourseFromCartData _$RemoveCourseFromCartDataFromJson(
  Map<String, dynamic> json,
) => RemoveCourseFromCartData(
  cartId: json['cartId'] as String?,
  itemCount: (json['itemCount'] as num?)?.toInt(),
  subtotal: (json['subtotal'] as num?)?.toDouble(),
  removeCourseItems: (json['items'] as List<dynamic>?)
      ?.map(
        (e) => RemoveCourseFromCartItems.fromJson(e as Map<String, dynamic>),
      )
      .toList(),
);

Map<String, dynamic> _$RemoveCourseFromCartDataToJson(
  RemoveCourseFromCartData instance,
) => <String, dynamic>{
  'cartId': instance.cartId,
  'itemCount': instance.itemCount,
  'subtotal': instance.subtotal,
  'items': instance.removeCourseItems,
};

RemoveCourseFromCartItems _$RemoveCourseFromCartItemsFromJson(
  Map<String, dynamic> json,
) => RemoveCourseFromCartItems(
  cartItemId: json['cartItemId'] as String?,
  courseId: json['courseId'] as String?,
  courseTitle: json['courseTitle'] as String?,
  courseThumbnailUrl: json['courseThumbnailUrl'] as String?,
  teacherName: json['teacherName'] as String?,
  originalPrice: (json['originalPrice'] as num?)?.toInt(),
);

Map<String, dynamic> _$RemoveCourseFromCartItemsToJson(
  RemoveCourseFromCartItems instance,
) => <String, dynamic>{
  'cartItemId': instance.cartItemId,
  'courseId': instance.courseId,
  'courseTitle': instance.courseTitle,
  'courseThumbnailUrl': instance.courseThumbnailUrl,
  'teacherName': instance.teacherName,
  'originalPrice': instance.originalPrice,
};
