import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/cubit/theme_cubit.dart';
import 'package:graduation_app/core/di/dependency_injection.dart';
import 'core/helpers/secure_storage_helper.dart';
import 'core/helpers/shared_pref_helper.dart';
import 'core/networking/api_constants.dart';
import 'learnify_app.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await ScreenUtil.ensureScreenSize();
  await setupGetIt();
  await SharedPrefHelper().init();
  final String? token = await getIt<SecureStorageHelper>().getToken(
    key: ApiKeys.token,
  );

  await getIt<ThemeCubit>().loadTheme();
  runApp(LearnifyApp(token: token));
}
