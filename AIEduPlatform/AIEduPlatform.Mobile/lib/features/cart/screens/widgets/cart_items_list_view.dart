import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/cart/data/models/get_my_cart_response_model.dart';

import 'cart_item_card.dart';

class CartItemsListView extends StatelessWidget {
  final List<MyCartItems> myCartItemsList;
  const CartItemsListView({super.key, required this.myCartItemsList});

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      itemBuilder: (context, index) {
        return Padding(
          padding: EdgeInsets.only(bottom: 16.h),
          child: CartItemCard(cartItems: myCartItemsList[index]),
        );
      },
      itemCount: myCartItemsList.length,
    );
  }
}
