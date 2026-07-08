// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'checkout_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CheckoutResponseModel _$CheckoutResponseModelFromJson(
  Map<String, dynamic> json,
) => CheckoutResponseModel(
  checkoutResponseData: CheckoutResponseData.fromJson(
    json['data'] as Map<String, dynamic>,
  ),
  message: json['message'] as String?,
);

Map<String, dynamic> _$CheckoutResponseModelToJson(
  CheckoutResponseModel instance,
) => <String, dynamic>{
  'data': instance.checkoutResponseData,
  'message': instance.message,
};

CheckoutResponseData _$CheckoutResponseDataFromJson(
  Map<String, dynamic> json,
) => CheckoutResponseData(
  orderId: json['orderId'] as String?,
  clientSecret: json['clientSecret'] as String?,
  paymentIntentId: json['paymentIntentId'] as String?,
  publishableKey: json['publishableKey'] as String?,
  requiresPayment: json['requiresPayment'] as bool?,
  totalAmount: (json['totalAmount'] as num?)?.toDouble(),
  currency: json['currency'] as String?,
  checkoutItems: (json['items'] as List<dynamic>?)
      ?.map((e) => CheckoutResponseItems.fromJson(e as Map<String, dynamic>))
      .toList(),
);

Map<String, dynamic> _$CheckoutResponseDataToJson(
  CheckoutResponseData instance,
) => <String, dynamic>{
  'orderId': instance.orderId,
  'clientSecret': instance.clientSecret,
  'paymentIntentId': instance.paymentIntentId,
  'publishableKey': instance.publishableKey,
  'requiresPayment': instance.requiresPayment,
  'totalAmount': instance.totalAmount,
  'currency': instance.currency,
  'items': instance.checkoutItems,
};

CheckoutResponseItems _$CheckoutResponseItemsFromJson(
  Map<String, dynamic> json,
) => CheckoutResponseItems(
  courseId: json['courseId'] as String?,
  courseTitle: json['courseTitle'] as String?,
  price: (json['price'] as num?)?.toDouble(),
);

Map<String, dynamic> _$CheckoutResponseItemsToJson(
  CheckoutResponseItems instance,
) => <String, dynamic>{
  'courseId': instance.courseId,
  'courseTitle': instance.courseTitle,
  'price': instance.price,
};
