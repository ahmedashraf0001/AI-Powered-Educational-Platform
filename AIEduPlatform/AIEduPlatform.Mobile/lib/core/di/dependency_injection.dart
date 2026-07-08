import 'package:dio/dio.dart';

import 'package:get_it/get_it.dart';
import 'package:graduation_app/core/cubit/theme_cubit.dart';
import 'package:graduation_app/core/helpers/shared_pref_helper.dart';
import 'package:graduation_app/features/ai/data/networking/chat_api_service.dart';
import 'package:graduation_app/features/ai/data/repo/chat_repo.dart';
import 'package:graduation_app/features/ai/data/repo/ai_services_repo.dart';
import 'package:graduation_app/features/ai/logic/chat_cubit/chat_cubit.dart';
import 'package:graduation_app/features/ai/logic/ai_services_cubit/cubit/ai_services_cubit.dart';
import 'package:graduation_app/features/courses/data/repo/courses_repo.dart';
import 'package:graduation_app/features/home/data/repo/home_repo.dart';
import 'package:graduation_app/features/register/data/repo/register_repo.dart';
import 'package:graduation_app/features/register/logic/register_cubit.dart';
import '../../features/cart/data/repo/cart_repo.dart';
import '../../features/cart/logic/cart_cubit.dart';
import '../../features/courses/logic/courses_cubit.dart';
import '../../features/home/logic/home_cubit.dart';
import '../../features/login/data/repo/login_repo.dart';
import '../../features/login/logic/login_cubit.dart';
import '../../features/profile/data/repo/profile_repo.dart';
import '../../features/profile/logic/profile_cubit.dart';
import '../helpers/secure_storage_helper.dart';
import '../networking/api_service.dart';
import '../networking/dio_factory.dart';
import '../services/stripe/stripe_service.dart';

final getIt = GetIt.instance;

Future<void> setupGetIt() async {
  Dio dio = DioFactory.getDio();

  //ApiService
  getIt.registerLazySingleton<ApiService>(() => ApiService(dio));

  // Ai ApiService
  getIt.registerLazySingleton<ChatApiService>(() => ChatApiService());

  getIt.registerLazySingleton<LoginRepo>(
    () => LoginRepo(apiService: getIt<ApiService>()),
  );

  getIt.registerFactory<LoginCubit>(() => LoginCubit(getIt<LoginRepo>()));

  //register
  getIt.registerLazySingleton<RegisterRepo>(
    () => RegisterRepo(apiService: getIt<ApiService>()),
  );
  getIt.registerFactory<RegisterCubit>(
    () => RegisterCubit(getIt<RegisterRepo>()),
  );

  //secure storage
  getIt.registerLazySingleton<SecureStorageHelper>(() => SecureStorageHelper());
  getIt.registerLazySingleton<SharedPrefHelper>(() => SharedPrefHelper());

  //home
  getIt.registerLazySingleton<HomeRepo>(
    () => HomeRepo(apiService: getIt<ApiService>()),
  );

  getIt.registerFactory<HomeCubit>(() => HomeCubit(getIt<HomeRepo>()));

  // courses
  getIt.registerLazySingleton<CoursesRepo>(
    () => CoursesRepo(apiService: getIt<ApiService>()),
  );
  getIt.registerFactory<CoursesCubit>(() => CoursesCubit(getIt<CoursesRepo>()));

  //profile
  getIt.registerLazySingleton<ProfileRepo>(
    () => ProfileRepo(apiService: getIt<ApiService>()),
  );
  getIt.registerFactory<ProfileCubit>(() => ProfileCubit(getIt<ProfileRepo>()));

  //cart
  getIt.registerLazySingleton<CartRepo>(
    () => CartRepo(apiService: getIt<ApiService>()),
  );
  getIt.registerFactory<CartCubit>(() => CartCubit(getIt<CartRepo>()));

  // stripe
  getIt.registerLazySingleton<StripeService>(() => StripeService());

  // chat repo
  getIt.registerLazySingleton<ChatRepo>(
    () => ChatRepo(chatApiService: getIt<ChatApiService>()),
  );

  //chat cubit
  getIt.registerFactory<ChatCubit>(() => ChatCubit(getIt<ChatRepo>()));

  //flashcards
  getIt.registerLazySingleton<AiServicesRepo>(
    () => AiServicesRepo(apiService: getIt<ApiService>()),
  );

  getIt.registerFactory<AiServicesCubit>(
    () => AiServicesCubit(getIt<AiServicesRepo>()),
  );

  getIt.registerLazySingleton<ThemeCubit>(() => ThemeCubit());

  /*  getIt.registerLazySingleton<StudentSignalRService>(
  () => StudentSignalRService(),
); */
}
