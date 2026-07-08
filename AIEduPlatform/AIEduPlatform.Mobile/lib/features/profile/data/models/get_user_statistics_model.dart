
import 'package:freezed_annotation/freezed_annotation.dart';
part 'get_user_statistics_model.g.dart';

@JsonSerializable()
class GetUserStatisticsModel{
  @JsonKey(name: 'data')
  final UserStatisticsData statisticsData;

  GetUserStatisticsModel({required this.statisticsData});
  factory GetUserStatisticsModel.fromJson(Map<String,dynamic>json)=>_$GetUserStatisticsModelFromJson(json);
}


@JsonSerializable()
class UserStatisticsData {
  final int ? coursesEnrolled;
  final int ? coursesCompleted;
  final int ? coursesTaught;
  final int ? totalStudySessions;
  final int ? examsTaken;
  final int ? averageExamScore;
  final int ? flashcardsCreated;
  final int ? quizzesTaken;

  UserStatisticsData({required this.coursesEnrolled, required this.coursesCompleted, required this.coursesTaught, required this.totalStudySessions, required this.examsTaken, required this.averageExamScore, required this.flashcardsCreated, required this.quizzesTaken});
  factory UserStatisticsData.fromJson(Map<String,dynamic>json)=>_$UserStatisticsDataFromJson(json);

}