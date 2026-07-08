import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/cubit/theme_cubit.dart';
import 'package:graduation_app/core/di/dependency_injection.dart';
import 'package:graduation_app/core/theming/app_theme.dart';
import 'package:graduation_app/features/cart/logic/cart_cubit.dart';
import 'package:graduation_app/features/login/screens/login_screen.dart';
import 'package:graduation_app/features/main/screens/main_screen.dart';
import 'core/services/navigation/navigation_service.dart';

class LearnifyApp extends StatelessWidget {
  final String? token;

  const LearnifyApp({super.key, this.token});

  @override
  Widget build(BuildContext context) {
    return ScreenUtilInit(
      designSize: const Size(375, 812),
      minTextAdapt: true,
      splitScreenMode: true,
      child: BlocBuilder<ThemeCubit, ThemeMode>(
        bloc: getIt<ThemeCubit>(),

        builder: (context, themeMode) {
          return BlocProvider(
            create: (_) => getIt<CartCubit>(),
            child: MaterialApp(
              useInheritedMediaQuery: true,
              theme: AppThemes.light,
              darkTheme: AppThemes.dark,
              themeMode: themeMode,

              navigatorKey: NavigationService.instance.navigatorKey,
              debugShowCheckedModeBanner: false,

              home: token == null || token!.isEmpty
                  ? LoginScreen()
                  : MainScreen(),
            ),
          );
        },
      ),
    );
  }
}
