import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/features/cart/logic/cart_cubit.dart';
import 'package:graduation_app/features/courses/logic/courses_cubit.dart';
import 'package:graduation_app/features/courses/logic/courses_state.dart';
import '../../../../core/services/navigation/navigation_service.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';

class AddCourseToCartBlocListener extends StatelessWidget {
  const AddCourseToCartBlocListener({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocListener<CoursesCubit, CoursesState>(
      listenWhen: (previous, current) =>
          current is SuccessAddCourseToCart ||
          current is FailureAddCourseToCart,
      listener: (context, state) {
        state.whenOrNull(
          successAddCourseToCart: (success) {
            context.read<CartCubit>().getMyCart();
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(
                  success.message ?? 'Added.',
                  style: TextStyles.font15.copyWith(
                    fontWeight: FontWeight.w600,
                    color: ColorsManager.darkBlue,
                  ),
                ),
                backgroundColor: ColorsManager.customGreen,
              ),
            );
          },
          failureAddCourseToCart: (error) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(
                  error ?? 'error.',
                  style: TextStyles.font15.copyWith(
                    fontWeight: FontWeight.w600,
                    color: ColorsManager.darkBlue,
                  ),
                ),
                backgroundColor: ColorsManager.lightRed,
              ),
            );
          },
        );
      },
      child: const SizedBox.shrink(),
    );
  }
}

void setupErrorState(BuildContext context, String error) {
  NavigationService.instance.goBack();
  showDialog(
    context: context,
    builder: (context) => AlertDialog(
      icon: const Icon(Icons.error, color: Colors.red, size: 32),
      content: Text(
        error,
        style: TextStyles.font15.copyWith(
          color: ColorsManager.darkBlue,
          fontWeight: FontWeight.w500,
        ),
      ),
      actions: [
        TextButton(
          onPressed: () {
            NavigationService.instance.goBack();
          },
          child: Text(
            'Got it',
            style: TextStyles.font14.copyWith(
              color: ColorsManager.darkBlue,
              fontWeight: FontWeight.w600,
            ),
          ),
        ),
      ],
    ),
  );
}
