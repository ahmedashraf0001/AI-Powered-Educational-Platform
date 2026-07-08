import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/features/cart/logic/cart_cubit.dart';
import 'package:graduation_app/features/cart/logic/cart_state.dart';
import 'package:graduation_app/features/cart/screens/widgets/checkout_button_bloc_listener.dart';
import 'package:graduation_app/features/cart/screens/widgets/empty_cart_widget.dart';
import '../../../../core/helpers/space_helper.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';
import 'cart_items_list_view.dart';

class MyCartBlocBuilder extends StatelessWidget {
  const MyCartBlocBuilder({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<CartCubit, CartState>(
      buildWhen: (previous, current) =>
          current is LoadingGetMyCart ||
          current is SuccessGetMyCart ||
          current is FailureGetMyCart,
      builder: (context, state) {
        if (state is LoadingGetMyCart) {
          return Center(child: CircularProgressIndicator());
        } else if (state is SuccessGetMyCart) {
          return state.cartData.myCartItems?.length == 0
              ? EmptyCartWidget()
              : Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.start,
                  children: [
                    Text(
                      'YOUR SELECTION (${state.cartData.itemCount ?? 0})',
                      style: TextStyles.font16.copyWith(
                        color: ColorsManager.darkGray,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    VerticalSpace(height: 16),
                    Flexible(
                      child: CartItemsListView(
                        myCartItemsList: state.cartData.myCartItems ?? [],
                      ),
                    ),
                    VerticalSpace(height: 20),
                    Container(
                      padding: EdgeInsets.symmetric(
                        horizontal: 16.w,
                        vertical: 16.h,
                      ),
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(16.r),
                        border: Border.all(color: ColorsManager.gray, width: 1),
                        boxShadow: [
                          BoxShadow(
                            color: Colors.black.withValues(alpha: 0.09),
                            blurRadius: 12,
                            offset: Offset(0, 8),
                            spreadRadius: 1,
                          ),
                        ],
                        color: context.colors.surface,
                      ),
                      child: Column(
                        spacing: 12.h,
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Text(
                                'Subtotal',
                                style: TextStyles.font16.copyWith(
                                  color: context.colors.onSurface,
                                  fontWeight: FontWeight.w500,
                                ),
                              ),
                              Text(
                                '\$${state.cartData.subtotal}',
                                style: TextStyles.font16.copyWith(
                                  color: context.colors.onSurface,
                                  fontWeight: FontWeight.w500,
                                ),
                              ),
                            ],
                          ),
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Text(
                                'Discount',
                                style: TextStyles.font16.copyWith(
                                  color: context.colors.onSurface,
                                  fontWeight: FontWeight.w500,
                                ),
                              ),
                              Text(
                                '-\$0',
                                style: TextStyles.font16.copyWith(
                                  color: ColorsManager.red,
                                  fontWeight: FontWeight.w500,
                                ),
                              ),
                            ],
                          ),
                          Divider(),
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Text(
                                'Total Price',
                                style: TextStyles.font20.copyWith(
                                  color: context.colors.onSurface,
                                ),
                              ),
                              Text(
                                '\$${state.cartData.subtotal}',
                                style: TextStyles.font20.copyWith(
                                  color: ColorsManager.mainBlue,
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                    VerticalSpace(height: 15),
                    CheckoutButtonBlocListener(),
                  ],
                );
        }
        if (state is FailureGetMyCart) {
          return Center(child: Text(state.message ?? 'error'));
        } else {
          return SizedBox.shrink();
        }
      },
    );
  }
}
