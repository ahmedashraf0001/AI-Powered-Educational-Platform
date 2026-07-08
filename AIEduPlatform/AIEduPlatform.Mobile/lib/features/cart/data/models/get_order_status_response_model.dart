import 'package:freezed_annotation/freezed_annotation.dart';

part 'get_order_status_response_model.g.dart';
@JsonSerializable()
class GetOrderStatusResponseModel{
  @JsonKey(name: 'data')
  final OrderStatusData orderStatusData;
  final String? message;

  GetOrderStatusResponseModel({required this.orderStatusData, required this.message});

  factory GetOrderStatusResponseModel.fromJson(Map<String,dynamic>json)=>_$GetOrderStatusResponseModelFromJson(json);
}

@JsonSerializable()
class OrderStatusData{
  final String? orderId;
  final String? status;
  final double? totalAmount;
  final String? currency;
  @JsonKey(name: 'enrolledCourses')
  final List<EnrolledCoursesModel>? enrolledCourses;

  OrderStatusData({required this.orderId, required this.status, required this.totalAmount, required this.currency, required this.enrolledCourses});

  factory OrderStatusData.fromJson(Map<String,dynamic>json)=>_$OrderStatusDataFromJson(json);

}

@JsonSerializable()
class EnrolledCoursesModel{
  final String? courseId;
  final String? courseTitle;
  final double? price;

  EnrolledCoursesModel({required this.courseId, required this.courseTitle, required this.price});

  factory EnrolledCoursesModel.fromJson(Map<String,dynamic>json)=>_$EnrolledCoursesModelFromJson(json);

}
