import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

extension DateFormatting on String {
  String get examMonth =>
      DateFormat('MMM').format(DateTime.parse(this)).toUpperCase();

  String get examDay => DateFormat('dd').format(DateTime.parse(this));

  String get examTime =>
      DateFormat('hh:mm a').format(DateTime.parse(this).toLocal());
}

extension ThemeX on BuildContext {
  ColorScheme get colors => Theme.of(this).colorScheme;
}
