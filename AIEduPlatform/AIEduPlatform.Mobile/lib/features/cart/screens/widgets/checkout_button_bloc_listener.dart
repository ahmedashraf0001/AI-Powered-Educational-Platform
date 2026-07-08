import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/features/cart/logic/cart_cubit.dart';
import 'package:graduation_app/features/cart/logic/cart_state.dart';
import 'package:graduation_app/features/courses/screens/my_courses_screen.dart';
import 'package:graduation_app/features/main/screens/main_screen.dart';

import 'checkout_button_bloc_builder.dart';

class CheckoutButtonBlocListener extends StatelessWidget {
  const CheckoutButtonBlocListener({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocListener<CartCubit, CartState>(
      listener: (context, state) {
        state.whenOrNull(
          successStartCheckout: (_) {
            // do nothing, polling is in progress
          },

          successOrderStatus: (_) {
            NavigationService.instance.navigateToAndRemoveUntil(
              const MainScreen(),
            );
          },

          failureStartCheckout: (error) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('payemnt failed'),
                backgroundColor: Colors.red,
              ),
            );
          },

          failureOrderStatus: (error) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('payemnt failed'),
                backgroundColor: ColorsManager.red,
              ),
            );
          },
        );
      },
      child: CheckoutButtonBlocBuilder(),
    );
  }
}
