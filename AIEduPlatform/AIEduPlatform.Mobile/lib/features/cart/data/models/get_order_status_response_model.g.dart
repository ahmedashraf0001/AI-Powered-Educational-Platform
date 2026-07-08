// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'get_order_status_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

GetOrderStatusResponseModel _$GetOrderStatusResponseModelFromJson(
  Map<String, dynamic> json,
) => GetOrderStatusResponseModel(
  orderStatusData: OrderStatusData.fromJson(
    json['data'] as Map<String, dynamic>,
  ),
  message: json['message'] as String?,
);

Map<String, dynamic> _$GetOrderStatusResponseModelToJson(
  GetOrderStatusResponseModel instance,
) => <String, dynamic>{
  'data': instance.orderStatusData,
  'message': instance.message,
};

OrderStatusData _$OrderStatusDataFromJson(Map<String, dynamic> json) =>
    OrderStatusData(
      orderId: json['orderId'] as String?,
      status: json['status'] as String?,
      totalAmount: (json['totalAmount'] as num?)?.toDouble(),
      currency: json['currency'] as String?,
      enrolledCourses: (json['enrolledCourses'] as List<dynamic>?)
          ?.map((e) => EnrolledCoursesModel.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$OrderStatusDataToJson(OrderStatusData instance) =>
    <String, dynamic>{
      'orderId': instance.orderId,
      'status': instance.status,
      'totalAmount': instance.totalAmount,
      'currency': instance.currency,
      'enrolledCourses': instance.enrolledCourses,
    };

EnrolledCoursesModel _$EnrolledCoursesModelFromJson(
  Map<String, dynamic> json,
) => EnrolledCoursesModel(
  courseId: json['courseId'] as String?,
  courseTitle: json['courseTitle'] as String?,
  price: (json['price'] as num?)?.toDouble(),
);

Map<String, dynamic> _$EnrolledCoursesModelToJson(
  EnrolledCoursesModel instance,
) => <String, dynamic>{
  'courseId': instance.courseId,
  'courseTitle': instance.courseTitle,
  'price': instance.price,
};
