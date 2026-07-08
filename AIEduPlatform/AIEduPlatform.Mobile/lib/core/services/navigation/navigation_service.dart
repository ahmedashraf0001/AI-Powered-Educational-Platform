import 'package:flutter/material.dart';

class NavigationService {
  static final NavigationService _instance = NavigationService._internal();

  factory NavigationService() {
    return _instance;
  }

  NavigationService._internal();

  static NavigationService get instance => _instance;

  final GlobalKey<NavigatorState> navigatorKey = GlobalKey<NavigatorState>();

  Future<dynamic> navigateTo(Widget page) {
    return navigatorKey.currentState!.push(
      MaterialPageRoute(builder: (context) => page),
    );
  }

  Future<dynamic> navigateToNamed(String routeName, {Object? arguments}) {
    return navigatorKey.currentState!.pushNamed(
      routeName,
      arguments: arguments,
    );
  }

  Future<dynamic> navigateToAndReplace(Widget page) {
    return navigatorKey.currentState!.pushReplacement(
      MaterialPageRoute(builder: (context) => page),
    );
  }

  Future<dynamic> navigateToAndRemoveUntil(Widget page) {
    return navigatorKey.currentState!.pushAndRemoveUntil(
      MaterialPageRoute(builder: (context) => page),
      (route) => false,
    );
  }

  Future<dynamic> navigateRootAndRemoveUntil(
    BuildContext context,
    Widget page,
  ) {
    return Navigator.of(context, rootNavigator: true).pushAndRemoveUntil(
      MaterialPageRoute(builder: (context) => page),
      (route) => false,
    );
  }

  void goBack<T extends Object?>([T? result]) {
    navigatorKey.currentState!.pop(result);
  }

  void goBackToFirst() {
    return navigatorKey.currentState!.popUntil((route) => route.isFirst);
  }

  void goBackTo(String routeName) {
    return navigatorKey.currentState!.popUntil(
      (route) => route.settings.name == routeName,
    );
  }
}
