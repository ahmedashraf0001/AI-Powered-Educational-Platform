import 'package:dio/dio.dart';
import 'package:graduation_app/core/networking/api_service.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/features/login/screens/login_screen.dart';
import 'package:pretty_dio_logger/pretty_dio_logger.dart';

import '../di/dependency_injection.dart';
import '../helpers/secure_storage_helper.dart';
import '../models/refresh_token_model.dart';
import 'api_constants.dart';
import 'dart:async';

class DioFactory {
  DioFactory._();

  static Dio? _dio;

  static Dio getDio() {
    if (_dio == null) {
      _dio = Dio(
        BaseOptions(
          baseUrl: ApiConstants.apiBaseUrl,
          connectTimeout: const Duration(seconds: 30),
          receiveTimeout: const Duration(seconds: 30),
          headers: {'Accept': 'application/json'},
        ),
      );

      _dio!.interceptors.addAll([
        // 1. StreamGuard MUST come first to hijack stream setups before anyone else touches them
        StreamGuardInterceptor(),
        // 2. Auth handling
        AuthInterceptor(),
        // 3. Logger comes last so standard requests are caught safely
        PrettyDioLogger(
          requestBody: true,
          requestHeader: true,
          responseHeader: true,
          responseBody: false,
        ),
      ]);
    }

    return _dio!;
  }

  static void resetDio() {
    _dio = null;
  }
}

/// -------------------------------------------------------------
/// Stream Guard Interceptor
/// Immediately short-circuits to avoid logger stream locks
/// -------------------------------------------------------------
class StreamGuardInterceptor extends Interceptor {
  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    // If it's a stream, we completely bypass downstream loggers for the request phase
    if (options.extra['skipLogging'] == true ||
        options.responseType == ResponseType.stream) {
      return handler.next(options);
    }
    return handler.next(options);
  }

  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) {
    // Short-circuit response before PrettyDioLogger tries to read/lock the stream object
    if (response.requestOptions.extra['skipLogging'] == true ||
        response.requestOptions.responseType == ResponseType.stream) {
      return handler.resolve(response);
    }
    return handler.next(response);
  }
}

/// --------------------
/// Auth Interceptor
/// --------------------
class AuthInterceptor extends Interceptor {
  bool _isRefreshing = false;
  Completer<String?>? _refreshCompleter;

  @override
  Future<void> onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final token = await getIt<SecureStorageHelper>().getToken(
      key: ApiKeys.token,
    );

    if (token != null && token.isNotEmpty) {
      options.headers['Authorization'] = 'Bearer $token';
    }

    return handler.next(options);
  }

  @override
  Future<void> onError(
    DioException err,
    ErrorInterceptorHandler handler,
  ) async {
    if (err.requestOptions.extra['skipLogging'] == true) {
      return handler.next(err);
    }

    if (err.response?.statusCode == 401) {
      String? newAccessToken;

      if (_isRefreshing) {
        // A refresh is already in progress — wait for it instead of failing.
        newAccessToken = await _refreshCompleter?.future;
      } else {
        _isRefreshing = true;
        _refreshCompleter = Completer<String?>();
        newAccessToken = await refreshToken();
        _refreshCompleter!.complete(newAccessToken);
        _isRefreshing = false;
      }

      if (newAccessToken != null) {
        DioFactory._dio!.options.headers['Authorization'] =
            'Bearer $newAccessToken';

        final requestOptions = err.requestOptions;
        requestOptions.headers['Authorization'] = 'Bearer $newAccessToken';

        // Unchanged: preserves responseType (incl. stream) on retry
        final options = Options(
          method: requestOptions.method,
          headers: requestOptions.headers,
          extra: requestOptions.extra,
          responseType: requestOptions.responseType,
        );

        final response = await DioFactory._dio!.request(
          requestOptions.path,
          data: requestOptions.data,
          queryParameters: requestOptions.queryParameters,
          options: options,
        );

        return handler.resolve(response);
      }
    }

    return handler.next(err);
  }

  Future<String?> refreshToken() async {
    try {
      final refreshTokenValue = await getIt<SecureStorageHelper>().getToken(
        key: ApiKeys.refreshToken,
      );
      final accessTokenValue = await getIt<SecureStorageHelper>().getToken(
        key: ApiKeys.token,
      );

      if (refreshTokenValue == null || accessTokenValue == null) return null;

      final response = await getIt<ApiService>().refreshToken(
        RefreshTokenRequest(
          accessToken: accessTokenValue,
          refreshToken: refreshTokenValue,
        ),
      );

      final newAccessToken = response.accessToken;
      final newRefreshToken = response.refreshToken;

      if (newAccessToken == null) return null;

      await getIt<SecureStorageHelper>().saveToken(newAccessToken);

      // Save the rotated refresh token too, otherwise the old one
      // becomes invalid and the next refresh attempt fails.
      if (newRefreshToken != null && newRefreshToken.isNotEmpty) {
        await getIt<SecureStorageHelper>().saveRefreshToken(newRefreshToken);
      }

      return newAccessToken;
    } catch (exception) {
      await getIt<SecureStorageHelper>().clearAllTokens();
      DioFactory.resetDio();
      NavigationService.instance.navigateToAndRemoveUntil(LoginScreen());
      return null;
    }
  }
}
