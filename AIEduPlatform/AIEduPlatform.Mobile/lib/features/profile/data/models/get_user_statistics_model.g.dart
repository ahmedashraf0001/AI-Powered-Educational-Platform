// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'get_user_statistics_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

GetUserStatisticsModel _$GetUserStatisticsModelFromJson(
  Map<String, dynamic> json,
) => GetUserStatisticsModel(
  statisticsData: UserStatisticsData.fromJson(
    json['data'] as Map<String, dynamic>,
  ),
);

Map<String, dynamic> _$GetUserStatisticsModelToJson(
  GetUserStatisticsModel instance,
) => <String, dynamic>{'data': instance.statisticsData};

UserStatisticsData _$UserStatisticsDataFromJson(Map<String, dynamic> json) =>
    UserStatisticsData(
      coursesEnrolled: (json['coursesEnrolled'] as num?)?.toInt(),
      coursesCompleted: (json['coursesCompleted'] as num?)?.toInt(),
      coursesTaught: (json['coursesTaught'] as num?)?.toInt(),
      totalStudySessions: (json['totalStudySessions'] as num?)?.toInt(),
      examsTaken: (json['examsTaken'] as num?)?.toInt(),
      averageExamScore: (json['averageExamScore'] as num?)?.toInt(),
      flashcardsCreated: (json['flashcardsCreated'] as num?)?.toInt(),
      quizzesTaken: (json['quizzesTaken'] as num?)?.toInt(),
    );

Map<String, dynamic> _$UserStatisticsDataToJson(UserStatisticsData instance) =>
    <String, dynamic>{
      'coursesEnrolled': instance.coursesEnrolled,
      'coursesCompleted': instance.coursesCompleted,
      'coursesTaught': instance.coursesTaught,
      'totalStudySessions': instance.totalStudySessions,
      'examsTaken': instance.examsTaken,
      'averageExamScore': instance.averageExamScore,
      'flashcardsCreated': instance.flashcardsCreated,
      'quizzesTaken': instance.quizzesTaken,
    };
