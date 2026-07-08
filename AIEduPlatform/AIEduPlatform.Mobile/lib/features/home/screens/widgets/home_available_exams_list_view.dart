import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/home/data/models/get_availble_exams_response_model.dart';
import 'package:graduation_app/features/home/screens/widgets/home_available_exams_card.dart';

class HomeAvailableExamsListView extends StatelessWidget {
  final List<AvailableExamsItemModel> examsList;
  const HomeAvailableExamsListView({super.key, required this.examsList});

  @override
  Widget build(BuildContext context) {
    return examsList.isEmpty
        ? Padding(
            padding: EdgeInsets.only(top: 16.h),
            child: SizedBox(
              child: Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.quiz_outlined, size: 48.sp, color: Colors.grey),
                    SizedBox(height: 12.h),
                    Text(
                      'No Exams yet',
                      style: TextStyle(
                        fontSize: 16.sp,
                        fontWeight: FontWeight.w600,
                        color: Colors.grey[700],
                      ),
                    ),
                    SizedBox(height: 6.h),
                    Text(
                      'Start learning now and explore available exams',
                      textAlign: TextAlign.center,
                      style: TextStyle(fontSize: 13.sp, color: Colors.grey),
                    ),
                  ],
                ),
              ),
            ),
          )
        : ListView.builder(
            shrinkWrap: true,
            physics: NeverScrollableScrollPhysics(),
            itemBuilder: (context, index) {
              return Padding(
                padding: EdgeInsets.only(bottom: 12.h),
                child: HomeAvailableExamCard(examModel: examsList[index]),
              );
            },
            itemCount: examsList.length,
          );
  }
}
