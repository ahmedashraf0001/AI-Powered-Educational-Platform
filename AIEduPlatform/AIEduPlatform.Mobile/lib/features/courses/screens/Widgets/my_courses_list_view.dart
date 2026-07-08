import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../../cart/data/models/my_courses_response_model.dart';
import 'my_courses_card.dart';

class MyCoursesListView extends StatelessWidget {
  final MyCoursesData coursesData;
  const MyCoursesListView({
    super.key, required this.coursesData,
  });

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      itemBuilder: (context,index){
        return Padding(
          padding:  EdgeInsets.only(bottom: 16.h),
          child: MyCoursesItemCard(coursesProgress: coursesData.courseProgressList![index],),
        );
      },
      itemCount: coursesData.courseProgressList?.length ?? 0,
    );
  }
}
