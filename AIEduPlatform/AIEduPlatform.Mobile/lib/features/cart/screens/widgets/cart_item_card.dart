import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/features/cart/logic/cart_cubit.dart';

import '../../../../core/helpers/space_helper.dart';
import '../../../../core/networking/api_constants.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';
import '../../data/models/get_my_cart_response_model.dart';

class CartItemCard extends StatelessWidget {
  final MyCartItems cartItems;
  const CartItemCard({super.key, required this.cartItems});

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        Container(
          padding: EdgeInsets.symmetric(horizontal: 12.w, vertical: 12.h),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(12.r),
            border: Border.all(color: ColorsManager.gray, width: 1),
          ),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            spacing: 16.w,
            children: [
              SizedBox(
                width: 96.w,
                height: 96.h,
                child: CachedNetworkImage(
                  imageUrl:
                      '${ApiConstants.baseImageUrl}${cartItems.courseThumbnailUrl}',
                  fit: BoxFit.cover,
                  width: double.infinity,
                  height: double.infinity,
                  placeholder: (context, url) =>
                      Center(child: CircularProgressIndicator()),
                  errorWidget: (context, url, error) =>
                      Icon(Icons.error, color: ColorsManager.red),
                ),
              ),
              Flexible(
                child: Column(
                  spacing: 4.h,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      cartItems.courseTitle ?? '',
                      style: TextStyles.font18.copyWith(
                        color: context.colors.onSurface,
                      ),
                      overflow: TextOverflow.ellipsis,
                    ),
                    Text(
                      cartItems.teacherName ?? '',
                      style: TextStyles.font16.copyWith(
                        color: context.colors.onSurface.withValues(alpha: 0.8),
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    VerticalSpace(height: 6),
                    Text(
                      '\$${cartItems.originalPrice ?? 0}',
                      style: TextStyles.font22.copyWith(
                        color: ColorsManager.mainBlue,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
        Positioned(
          right: 0,
          top: 0,
          bottom: 0,
          child: Center(
            child: IconButton(
              onPressed: () {
                context.read<CartCubit>().deleteCourseFromCart(
                  cartItems.courseId ?? '0',
                );
              },
              icon: Icon(
                Icons.clear,
                color: context.colors.onSurface.withValues(alpha: 0.7),
                size: 27.w,
              ),
            ),
          ),
        ),
      ],
    );
  }
}
