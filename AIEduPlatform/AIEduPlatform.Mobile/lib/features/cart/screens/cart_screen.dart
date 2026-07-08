import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/features/cart/logic/cart_cubit.dart';
import 'package:graduation_app/features/cart/screens/widgets/my_cart_bloc_builder.dart';
import 'package:graduation_app/features/cart/screens/widgets/remove_from_cart_listener.dart';

import '../../../core/theming/styles.dart';

class CartScreen extends StatefulWidget {
  const CartScreen({super.key});

  @override
  State<CartScreen> createState() => _CartScreenState();
}

class _CartScreenState extends State<CartScreen> {
  @override
  void initState() {
    super.initState();
    context.read<CartCubit>().getMyCart();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('My Cart', style: TextStyles.font20),
        centerTitle: true,
        actions: [
          IconButton(
            onPressed: () {
              context.read<CartCubit>().clearMyCart();
            },
            icon: Icon(
              Icons.delete_forever_outlined,
              size: 27.w,
              color: ColorsManager.red,
            ),
          ),
        ],
        actionsPadding: EdgeInsets.symmetric(horizontal: 16.w),
      ),
      body: Padding(
        padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 16.h),
        child: Column(
          children: [
            Flexible(child: MyCartBlocBuilder()),
            RemoveFromCartListener(),
          ],
        ),
      ),
    );
  }
}
