import 'package:flutter/material.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/features/main/screens/widgets/cart_icon_with_badge.dart';
import 'package:persistent_bottom_nav_bar/persistent_bottom_nav_bar.dart';
import '../../../core/theming/colors.dart';
import '../../cart/screens/cart_screen.dart';
import '../../courses/screens/courses_catalog_screen.dart';
import '../../home/screens/exam_submissions_screen.dart';
import '../../home/screens/home_screen.dart';
import '../../profile/screens/profile_screen.dart';

class MainScreen extends StatefulWidget {
  const MainScreen({super.key});

  @override
  State<MainScreen> createState() => _MainScreenState();
}

class _MainScreenState extends State<MainScreen> {
  late PersistentTabController _controller;

  @override
  void initState() {
    super.initState();
    _controller = PersistentTabController(initialIndex: 0);
  }

  /// Screens for each tab
  List<Widget> _buildScreens() {
    return [
      const HomeScreen(),
      const CoursesCatalogScreen(),
      const CartScreen(),
      const ExamSubmissionsScreen(),
      const ProfileScreen(),
    ];
  }

  /// Bottom Navigation Bar Items
  List<PersistentBottomNavBarItem> _navBarsItems() {
    return [
      PersistentBottomNavBarItem(
        icon: const Icon(Icons.home_outlined),
        inactiveIcon: const Icon(
          Icons.home_outlined,
          color: ColorsManager.lightGray,
        ),
        activeColorPrimary: ColorsManager.mainBlue,
        inactiveColorPrimary: ColorsManager.lightGray,
        activeColorSecondary: Colors.white,
        title: 'Home',
        textStyle: const TextStyle(fontWeight: FontWeight.bold),
      ),
      PersistentBottomNavBarItem(
        icon: const Icon(Icons.menu_book_outlined),
        inactiveIcon: const Icon(
          Icons.menu_book_outlined,
          color: ColorsManager.lightGray,
        ),
        activeColorPrimary: ColorsManager.mainBlue,
        inactiveColorPrimary: ColorsManager.lightGray,
        activeColorSecondary: Colors.white,
        title: 'Courses',
        textStyle: const TextStyle(fontWeight: FontWeight.bold),
      ),
      PersistentBottomNavBarItem(
        icon: const CartIconWithBadge(),
        inactiveIcon: const CartIconWithBadge(),
        activeColorPrimary: ColorsManager.mainBlue,
        inactiveColorPrimary: ColorsManager.lightGray,
        activeColorSecondary: Colors.white,
        title: 'Cart',
        textStyle: const TextStyle(fontWeight: FontWeight.bold),
      ),
      PersistentBottomNavBarItem(
        icon: const Icon(Icons.checklist_outlined),
        inactiveIcon: const Icon(
          Icons.checklist_outlined,
          color: ColorsManager.lightGray,
        ),
        activeColorPrimary: ColorsManager.mainBlue,
        inactiveColorPrimary: ColorsManager.lightGray,
        activeColorSecondary: Colors.white,
        title: 'Submissions',
        textStyle: const TextStyle(fontWeight: FontWeight.bold),
      ),
      PersistentBottomNavBarItem(
        icon: const Icon(Icons.person_outlined),
        inactiveIcon: const Icon(
          Icons.person_outlined,
          color: ColorsManager.lightGray,
        ),
        activeColorPrimary: ColorsManager.mainBlue,
        inactiveColorPrimary: ColorsManager.lightGray,
        activeColorSecondary: Colors.white,
        title: 'Profile',
        textStyle: const TextStyle(fontWeight: FontWeight.bold),
      ),
    ];
  }

  Future<bool> _onWillPop() async {
    if (_controller.index != 0) {
      _controller.index = 0;
      return false;
    } else {
      return true;
    }
  }

  @override
  Widget build(BuildContext context) {
    return WillPopScope(
      onWillPop: _onWillPop,
      child: PersistentTabView(
        context,
        controller: _controller,
        screens: _buildScreens(),
        items: _navBarsItems(),
        backgroundColor: context.colors.surface,
        confineToSafeArea: true,
        handleAndroidBackButtonPress: true,
        resizeToAvoidBottomInset: true,
        stateManagement: true,
        decoration: NavBarDecoration(
          borderRadius: BorderRadius.circular(0),
          colorBehindNavBar: Colors.white,
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.08),
              blurRadius: 10,
              offset: const Offset(0, -2),
            ),
          ],
        ),
        navBarStyle: NavBarStyle.style10,
      ),
    );
  }
}
