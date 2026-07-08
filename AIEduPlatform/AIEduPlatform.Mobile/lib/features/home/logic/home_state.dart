import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:graduation_app/features/home/data/models/get_availble_exams_response_model.dart';
import 'package:graduation_app/features/home/data/models/get_exam_questions_response_model.dart';
import 'package:graduation_app/features/home/data/models/get_student_submissions_response_model.dart';
import 'package:graduation_app/features/home/data/models/submit_exam_response_model.dart';

import '../data/models/continue_learning_course_model.dart';

part 'home_state.freezed.dart';

@freezed
class HomeState<T> with _$HomeState<T> {
  const factory HomeState.initial() = _Initial;
  const factory HomeState.loadingContinueLearning() = LoadingContinueLearning;
  const factory HomeState.successContinueLearning(
    List<ContinueLearningDataModel> courses,
  ) = SuccessContinueLearning;
  const factory HomeState.failureContinueLearning({String? message}) =
      FailureContinueLearning;

  const factory HomeState.loadingUpComingExams() = LoadingUpComingExams;
  const factory HomeState.successUpComingExams(T data) =
      SuccessUpComingExams<T>;
  const factory HomeState.failureUpComingExams({String? message}) =
      FailureUpComingExams;

  const factory HomeState.loadingGetAvailableExams() = LoadingGetAvailableExams;
  const factory HomeState.successGetAvailableExams(
    AvailableExamsDataModel availableExamsDataModel,
  ) = SuccessGetAvailableExams;
  const factory HomeState.failureGetAvailableExams({String? message}) =
      FailureGetAvailableExams;

  const factory HomeState.loadingExamQuestions() = LoadingExamQuestions;
  const factory HomeState.successExamQuestions(
    List<ExamQuestionDataModel>? questions,
  ) = SuccessExamQuestions;
  const factory HomeState.failureExamQuestions({String? message}) =
      FailureExamQuestions;

  const factory HomeState.loadingSubmitExam() = LoadingSubmitExam;
  const factory HomeState.successSubmitExams(String? submissionId) =
      SuccessSubmitExam;
  const factory HomeState.failureSubmitExam({String? message}) =
      FailureSubmitExam;

  const factory HomeState.loadingSubmissionDetails() = LoadingSubmissionDetails;
  const factory HomeState.successSubmissionDetails(
    SubmissionDetailResponseModel success,
  ) = SuccessSubmissionDetails;
  const factory HomeState.failureSubmissionDetails({String? message}) =
      FailureSubmissionDetails;

  const factory HomeState.loadingStudentSubmissions() =
      LoadingStudentSubmissions;
  const factory HomeState.successStudentSubmissions(
    GetStudentSubmissionsResponseModel success,
  ) = SuccessStudentSubmissions;
  const factory HomeState.failureStudentSubmissions({String? message}) =
      FailureStudentSubmissions;
}
