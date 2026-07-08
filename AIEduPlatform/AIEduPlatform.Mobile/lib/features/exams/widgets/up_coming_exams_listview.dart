import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/exams/widgets/up_coming_exam_card.dart';

class UpComingExamsListView extends StatelessWidget {
  const UpComingExamsListView({
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 205.h,
      child: ListView.builder(
        itemBuilder: (context,index){
          return Padding(
            padding:  EdgeInsets.only(right: 16.w),
            child: UpComingExamCard(),
          );
        },
        itemCount: 4,
        scrollDirection: Axis.horizontal,
      ),
    );
  }
}
