import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/features/cart/logic/cart_cubit.dart';
import 'package:graduation_app/features/home/logic/home_cubit.dart';
import 'package:graduation_app/features/home/screens/widgets/available_exams_bloc_builder.dart';
import 'package:graduation_app/features/home/screens/widgets/continue_learning_bloc_builder.dart';
import 'package:graduation_app/features/home/screens/widgets/home_top_card.dart';
import 'package:graduation_app/features/home/screens/widgets/streak_bloc_builder.dart';
import '../../../core/di/dependency_injection.dart';
import '../../../core/theming/styles.dart';
import '../../courses/screens/my_courses_screen.dart';

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (_) => getIt<HomeCubit>(),
      child: const _HomeScreenBody(),
    );
  }
}

class _HomeScreenBody extends StatefulWidget {
  const _HomeScreenBody();

  @override
  State<_HomeScreenBody> createState() => _HomeScreenBodyState();
}

class _HomeScreenBodyState extends State<_HomeScreenBody> {
  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    await Future.wait([
      context.read<CartCubit>().getMyCourses(),
      context.read<HomeCubit>().getAvailableExams(1, 10),
    ]);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Welcome Back👋', style: TextStyles.font20),
        automaticallyImplyLeading: false,
      ),
      body: Padding(
        padding: EdgeInsets.symmetric(
          horizontal: 16.w,
          vertical: 12.h,
        ).copyWith(top: 20.h),
        child: SingleChildScrollView(
          child: RefreshIndicator(
            color: ColorsManager.mainBlue,
            onRefresh: () async {
              await context.read<CartCubit>().getMyCourses();
            },
            child: Column(
              children: [
                HomeTopCard(
                  iconName: 'assets/svgs/course.svg',
                  title: 'My Courses',
                  color: ColorsManager.mainBlue,
                  textColor: context.colors.surface,
                  onTap: () {
                    final cubit = context.read<CartCubit>();
                    NavigationService.instance.navigateTo(
                      BlocProvider.value(
                        value: cubit,
                        child: MyCoursesScreen(),
                      ),
                    );
                  },
                ),
                VerticalSpace(height: 20),
                ContinueLearningBlocBuilder(),
                VerticalSpace(height: 20),
                AvailableExamsBlocBuilder(),
                VerticalSpace(height: 20),
                StreakBlocBuilder(),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
