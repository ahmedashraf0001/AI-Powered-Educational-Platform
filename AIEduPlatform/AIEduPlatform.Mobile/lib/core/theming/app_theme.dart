import 'package:flutter/material.dart';
import 'package:graduation_app/core/theming/colors.dart';

class AppThemes {
  static ThemeData light = ThemeData(
    brightness: Brightness.light,
    scaffoldBackgroundColor: Colors.white,
    colorScheme: ColorScheme.light(primary: ColorsManager.mainBlue),
    appBarTheme: const AppBarTheme(
      backgroundColor: Colors.white,
      scrolledUnderElevation: 0,
    ),
  );

  static ThemeData dark = ThemeData(
    brightness: Brightness.dark,
    scaffoldBackgroundColor: const Color(0xFF121212),
    colorScheme: ColorScheme.dark(primary: ColorsManager.mainBlue),
    appBarTheme: const AppBarTheme(
      backgroundColor: Color(0xFF121212),
      scrolledUnderElevation: 0,
    ),
  );
}
