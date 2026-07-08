import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/cart/logic/cart_cubit.dart';
import 'package:graduation_app/features/cart/logic/cart_state.dart';

class CartIconWithBadge extends StatelessWidget {
  const CartIconWithBadge({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<CartCubit, CartState>(
      builder: (context, state) {
        int count = 0;

        state.whenOrNull(
          successGetMyCart: (cart) {
            count = cart.itemCount ?? 0; // adjust to your model
          },
        );

        return Stack(
          clipBehavior: Clip.none,
          children: [
            const Icon(Icons.shopping_cart_outlined),

            if (count > 0)
              Positioned(
                right: -6,
                top: -4,
                child: Container(
                  padding: EdgeInsets.all(4.w),
                  constraints: BoxConstraints(minWidth: 18.w, minHeight: 18.h),
                  decoration: const BoxDecoration(
                    color: Colors.red,
                    shape: BoxShape.circle,
                  ),
                  child: Text(
                    count > 99 ? '99+' : '$count',
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 10.sp,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ),
          ],
        );
      },
    );
  }
}
