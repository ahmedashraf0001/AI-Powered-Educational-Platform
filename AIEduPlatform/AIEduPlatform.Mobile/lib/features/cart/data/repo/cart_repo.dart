
import 'package:graduation_app/core/networking/api_error_handler.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/core/networking/api_service.dart';
import 'package:graduation_app/features/cart/data/models/get_my_cart_response_model.dart';
import 'package:graduation_app/features/cart/data/models/get_order_status_response_model.dart';
import 'package:graduation_app/features/cart/data/models/my_courses_response_model.dart';
import 'package:graduation_app/features/cart/data/models/remove_course_from_cart_response_model.dart';

import '../models/checkout_response_model.dart';

class CartRepo{
  final ApiService apiService;

  CartRepo({required this.apiService});

  Future<ApiResult<GetMyCartResponseModel>>getMyCart()async{
    try {
      final response = await apiService.getMyCart();
      return ApiResult.success(response);
    }  catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<RemoveCourseFromCartResponseModel>>deleteCourseFromCart(String courseId)async{
    try {
      final response = await apiService.removeCourseFromCart(courseId);
      return ApiResult.success(response);
    }  catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<String ?>> clearMyCart()async{
    try {
      final response = await apiService.clearMyCart();
      return ApiResult.success(response.message ?? 'deleted.');
    }  catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<CheckoutResponseData>> startCheckout()async{
    try {
      final response = await apiService.startCheckout();
      return ApiResult.success(response.checkoutResponseData);
    }  catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }
  
  Future<ApiResult<GetOrderStatusResponseModel>> getOrderStatus(String orderId)async{
    try {
      final response = await apiService.getOrderStatus(orderId);
      return ApiResult.success(response);
    }  catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }


  Future<ApiResult<MyCoursesResponseModel>> getMyCourses()async{
    try {
      final response = await apiService.getMyCourses();
      return ApiResult.success(response);
    }  catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  }
