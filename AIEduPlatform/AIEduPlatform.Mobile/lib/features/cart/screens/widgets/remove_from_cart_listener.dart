import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/features/cart/logic/cart_cubit.dart';
import 'package:graduation_app/features/cart/logic/cart_state.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';

class RemoveFromCartListener extends StatelessWidget {
  const RemoveFromCartListener({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocListener<CartCubit, CartState>(
      listenWhen: (previous,current)=> current is SuccessRemoveCourseFromCart || current is FailureRemoveCourseFromCart,
      listener: (context, state) {
        state.whenOrNull(
            // loadingRemoveCourseFromCart: (){
            //   showDialog(
            //     context: context,
            //     builder: (context)=>Center(child: CircularProgressIndicator(color: ColorsManager.mainBlue,)),
            //   );
            // },
            successRemoveCourseFromCart: (success){
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text(success ?? 'Added.',style: TextStyles.font15.copyWith(fontWeight: FontWeight.w600,color: ColorsManager.darkBlue),),
                  backgroundColor: ColorsManager.customGreen,
                ),
              );
            },
            failureRemoveCourseFromCart: (error){
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text(error ?? 'Removed.',style: TextStyles.font15.copyWith(fontWeight: FontWeight.w600,color: ColorsManager.darkBlue),),
                ),
              );
            }
        );
      },
      child: const SizedBox.shrink(),
    );
  }
}


