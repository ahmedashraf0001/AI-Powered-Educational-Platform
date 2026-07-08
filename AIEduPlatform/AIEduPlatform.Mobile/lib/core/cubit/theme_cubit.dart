import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/core/di/dependency_injection.dart';
import 'package:graduation_app/core/helpers/shared_pref_helper.dart';

class ThemeCubit extends Cubit<ThemeMode> {
  ThemeCubit() : super(ThemeMode.light);

  Future<void> loadTheme() async {
    final isDark =
        await getIt<SharedPrefHelper>().getData(key: 'isDark') ?? false;

    emit(isDark ? ThemeMode.dark : ThemeMode.light);
  }

  Future<void> toggleTheme() async {
    final newMode = state == ThemeMode.light ? ThemeMode.dark : ThemeMode.light;

    emit(newMode);

    await getIt<SharedPrefHelper>().put(
      key: 'isDark',
      value: newMode == ThemeMode.dark,
    );
  }
}
