
import 'package:freezed_annotation/freezed_annotation.dart';

part 'checkout_response_model.g.dart';

@JsonSerializable()
class CheckoutResponseModel{
  @JsonKey(name: 'data')
  final CheckoutResponseData checkoutResponseData;
  final String? message;

  CheckoutResponseModel({required this.checkoutResponseData, required this.message});
  factory CheckoutResponseModel.fromJson(Map<String,dynamic>json)=>_$CheckoutResponseModelFromJson(json);
}


@JsonSerializable()
class CheckoutResponseData{
  final String? orderId;
  final String? clientSecret;
  final String? paymentIntentId;
  final String? publishableKey;
  final bool? requiresPayment;
  final double? totalAmount;
  final String? currency;
  @JsonKey(name: 'items')
  final List<CheckoutResponseItems>? checkoutItems;

  CheckoutResponseData({required this.orderId, required this.clientSecret, required this.paymentIntentId, required this.publishableKey, required this.requiresPayment, required this.totalAmount, required this.currency, required this.checkoutItems});

  factory CheckoutResponseData.fromJson(Map<String,dynamic>json)=>_$CheckoutResponseDataFromJson(json);

}

@JsonSerializable()
class CheckoutResponseItems{
  final String? courseId;
  final String? courseTitle;
  final double? price;

  CheckoutResponseItems({required this.courseId, required this.courseTitle, required this.price});

  factory CheckoutResponseItems.fromJson(Map<String,dynamic>json)=>_$CheckoutResponseItemsFromJson(json);


}