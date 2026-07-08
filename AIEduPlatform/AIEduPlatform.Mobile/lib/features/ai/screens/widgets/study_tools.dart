import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/ai/screens/widgets/study_tools_card.dart';

import '../../../../core/theming/colors.dart';

class StudyTools extends StatelessWidget {
  const StudyTools({
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      spacing: 16.h,
      children: [
        StudyToolsCard(
          title: 'Smart Flashcards',
          subTitle: 'Generate decks from notes',
          image: 'assets/svgs/flash_card.svg',
          color: ColorsManager.customOrange,
        ),
        StudyToolsCard(
          title: 'Practice Quiz',
          subTitle: 'Test your knowledge',
          image: 'assets/svgs/quiz.svg',
          color: ColorsManager.customGreen,
        ),
        StudyToolsCard(
          title: 'Summarize Lesson',
          subTitle: 'Long text to key points',
          image: 'assets/svgs/summarize.svg',
          color: ColorsManager.customPurple,
        ),

      ],
    );
  }
}
