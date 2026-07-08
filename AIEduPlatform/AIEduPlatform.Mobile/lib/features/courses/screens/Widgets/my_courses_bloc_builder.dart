import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/features/cart/logic/cart_cubit.dart';
import 'package:graduation_app/features/cart/logic/cart_state.dart';
import 'my_courses_list_view.dart';

class MyCoursesBlocBuilder extends StatelessWidget {
  const MyCoursesBlocBuilder({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<CartCubit, CartState>(
        buildWhen: (previous,current)=> current is SuccessGetMyCourses || current is FailureGetMyCourses || current is LoadingGetMyCourses,
        builder: (context, state) {
          if (state is FailureGetMyCourses){
            return Center(child: Text(state.message ?? 'error'),);
          }

          else if (state is SuccessGetMyCourses){
            return MyCoursesListView(coursesData: state.myCoursesReponseModel.coursesData,);

          }
          else if (state is LoadingGetMyCourses) {
            return const Center(
              child: CircularProgressIndicator(),
            );
          }

          else{
            return const Placeholder();
          }
        }
    );
  }
}
