import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:graduation_app/features/cart/data/models/checkout_response_model.dart';
import 'package:graduation_app/features/cart/data/models/get_my_cart_response_model.dart';
import 'package:graduation_app/features/cart/data/models/get_order_status_response_model.dart';
import 'package:graduation_app/features/cart/data/models/my_courses_response_model.dart';

part 'cart_state.freezed.dart';

@freezed
class CartState<T> with _$CartState<T> {
  const factory CartState.initial() = _Initial;

  const factory CartState.loadingGetMyCart() = LoadingGetMyCart;

  const factory CartState.successGetMyCart(MyCartData cartData) =
      SuccessGetMyCart;

  const factory CartState.failureGetMyCart({String? message}) =
      FailureGetMyCart;

  //
  const factory CartState.loadingRemoveCourseFromCart() =
      LoadingRemoveCourseFromCart;

  const factory CartState.successRemoveCourseFromCart({String? message}) =
      SuccessRemoveCourseFromCart;

  const factory CartState.failureRemoveCourseFromCart({String? message}) =
      FailureRemoveCourseFromCart;

  //
  const factory CartState.loadingClearCart() = LoadingClearCart;

  const factory CartState.successClearCart({String? message}) =
      SuccessClearCart;

  const factory CartState.failureClearCart({String? message}) =
      FailureClearCart;

  //
  const factory CartState.loadingStartCheckout() = LoadingStartCheckout;

  const factory CartState.successStartCheckout(
    CheckoutResponseData checkoutData,
  ) = SuccessStartCheckout;

  const factory CartState.failureStartCheckout({String? message}) =
      FailureStartCheckout;

  //
  const factory CartState.loadingOrderStatus() = LoadingOrderStatus;

  const factory CartState.successOrderStatus(OrderStatusData orderStatusData) =
      SuccessOrderStatus;

  const factory CartState.failureOrderStatus({String? message}) =
      FailureOrderStatus;

  const factory CartState.loadingGetMyCourses() = LoadingGetMyCourses;

  const factory CartState.successGetMyCourses(
    MyCoursesResponseModel myCoursesReponseModel,
  ) = SuccessGetMyCourses;

  const factory CartState.failureGetMyCourses({String? message}) =
      FailureGetMyCourses;
}
