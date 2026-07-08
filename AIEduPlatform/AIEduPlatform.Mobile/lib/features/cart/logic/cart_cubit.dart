import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/features/cart/data/repo/cart_repo.dart';

import '../../../core/di/dependency_injection.dart';
import '../../../core/services/stripe/stripe_service.dart';
import 'cart_state.dart';

class CartCubit extends Cubit<CartState> {
  final CartRepo cartRepo;
  CartCubit(this.cartRepo) : super(CartState.initial());

  getMyCart() async {
    emit(LoadingGetMyCart());
    final data = await cartRepo.getMyCart();
    return data.when(
      success: (success) {
        emit(SuccessGetMyCart(success.myCartData));
      },
      failure: (failure) {
        emit(
          FailureGetMyCart(message: failure.apiErrorModel.message ?? 'error'),
        );
      },
    );
  }

  deleteCourseFromCart(String cartItemId) async {
    emit(LoadingRemoveCourseFromCart());
    final data = await cartRepo.deleteCourseFromCart(cartItemId);
    return data.when(
      success: (success) async {
        emit(
          SuccessRemoveCourseFromCart(message: success.message ?? 'deleted.'),
        );
        await getMyCart();
      },
      failure: (failure) {
        emit(
          FailureRemoveCourseFromCart(
            message: failure.apiErrorModel.message ?? 'error',
          ),
        );
      },
    );
  }

  clearMyCart() async {
    emit(LoadingClearCart());
    final data = await cartRepo.clearMyCart();
    return data.when(
      success: (success) async {
        emit(SuccessClearCart(message: success));
        await getMyCart();
      },
      failure: (failure) {
        emit(
          FailureRemoveCourseFromCart(
            message: failure.apiErrorModel.message ?? 'error',
          ),
        );
      },
    );
  }

  Future<void> startCheckout() async {
    emit(LoadingStartCheckout());

    final result = await cartRepo.startCheckout();

    result.when(
      success: (checkoutData) async {
        try {
          if (checkoutData.requiresPayment ?? false) {
            final stripeService = getIt<StripeService>();

            await stripeService.initStripe(checkoutData.publishableKey!);

            await stripeService.makePayment(checkoutData.clientSecret!);
          }
          emit(SuccessStartCheckout(checkoutData));

          await getOrderStatus(checkoutData.orderId ?? 'null');
        } catch (e) {
          emit(FailureStartCheckout(message: e.toString()));
        }
      },

      failure: (failure) {
        emit(FailureStartCheckout(message: failure.apiErrorModel.message));
      },
    );
  }

  Future<void> getOrderStatus(String orderId, {int maxRetries = 5}) async {
    emit(LoadingOrderStatus());

    for (int i = 0; i < maxRetries; i++) {
      final data = await cartRepo.getOrderStatus(orderId);

      final result = data.when(
        success: (orderStatus) {
          final status = orderStatus.orderStatusData.status;
          if (status == 'Paid') {
            // Paid
            emit(SuccessOrderStatus(orderStatus.orderStatusData));
            return true;
          }
          if (status == 'Failed') {
            // Failed
            emit(FailureOrderStatus(message: 'Payment failed'));
            return true;
          }
          return false; // still Pending, retry
        },
        failure: (failure) {
          emit(FailureOrderStatus(message: failure.apiErrorModel.message));
          return true;
        },
      );

      if (result) return;
      await Future.delayed(Duration(seconds: 2));
    }

    emit(FailureOrderStatus(message: 'Order confirmation timed out'));
  }

  Future getMyCourses() async {
    emit(LoadingGetMyCourses());
    final data = await cartRepo.getMyCourses();
    return data.when(
      success: (myCoursesReponseModel) {
        emit(SuccessGetMyCourses(myCoursesReponseModel));
      },
      failure: (failure) {
        emit(FailureGetMyCourses(message: failure.apiErrorModel.message));
      },
    );
  }
}
