import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/theming/colors.dart';

import '../../../../core/widgets/custom_button.dart';
import '../../logic/cart_cubit.dart';
import '../../logic/cart_state.dart';

class CheckoutButtonBlocBuilder extends StatelessWidget {
  const CheckoutButtonBlocBuilder({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<CartCubit, CartState>(
      builder: (context, state) {
        final isLoading =
            state is LoadingStartCheckout || state is LoadingOrderStatus;

        return CustomButton(
          title: isLoading ? 'Loading...' : 'Proceed to Checkout',
          color: isLoading ? ColorsManager.darkBlue : ColorsManager.mainBlue,
          height: 56.h,
          onPressed: isLoading
              ? null
              : () {
                  context.read<CartCubit>().startCheckout();
                },
        );
      },
    );
  }
}
