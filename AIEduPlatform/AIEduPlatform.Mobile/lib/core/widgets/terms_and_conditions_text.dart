import 'package:flutter/material.dart';

import '../theming/colors.dart';
import '../theming/styles.dart';

class TermsAndConditionsText extends StatelessWidget {
  const TermsAndConditionsText({
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    return RichText(
        textAlign: TextAlign.center,
        text: TextSpan(
            text:'By logging, you agree to our ',style: TextStyles.font12.copyWith(color: ColorsManager.lightGray),
            children:  <TextSpan>[
              TextSpan(text:'Terms & Conditions ',style: TextStyles.font12.copyWith(color: ColorsManager.darkBlue)),
              TextSpan(text:'and\n',style: TextStyles.font12.copyWith(color: ColorsManager.lightGray)),
              TextSpan(text:'PrivacyPolicy.',style: TextStyles.font12.copyWith(color: ColorsManager.darkBlue)),
            ]

        )
    );
  }
}
