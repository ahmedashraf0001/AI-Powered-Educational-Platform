import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/theming/colors.dart';

class EmptyCartWidget extends StatelessWidget {
  const EmptyCartWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: EdgeInsets.symmetric(horizontal: 32.w),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 130.w,
              height: 130.w,
              decoration: BoxDecoration(
                color: context.colors.onSecondary.withValues(alpha: 0.8),
                shape: BoxShape.circle,
              ),
              child: Center(
                child: Icon(
                  Icons.shopping_cart_outlined,
                  size: 64.sp,
                  color: ColorsManager.mainBlue,
                ),
              ),
            ),
            SizedBox(height: 28.h),
            Text(
              'Your cart is empty',
              style: TextStyle(
                fontSize: 20.sp,
                fontWeight: FontWeight.w600,
                color: context.colors.onSurface.withValues(alpha: 0.8),
              ),
            ),
            SizedBox(height: 10.h),
            Text(
              "Looks like you haven't added anything yet.\nStart exploring and find something you love!",
              textAlign: TextAlign.center,
              style: TextStyle(
                fontSize: 14.sp,
                height: 1.6,
                color: const Color(0xFF888780),
              ),
            ),
            SizedBox(height: 32.h),
          ],
        ),
      ),
    );
  }
}
