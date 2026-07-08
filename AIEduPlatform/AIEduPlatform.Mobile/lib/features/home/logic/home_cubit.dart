import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/features/home/data/repo/home_repo.dart';

import 'home_state.dart';

class HomeCubit extends Cubit<HomeState> {
  final HomeRepo homeRepo;

  HomeCubit(this.homeRepo) : super(HomeState.initial());

  Future getContinueLearningCourses() async {
    emit(HomeState.loadingContinueLearning());
    final data = await homeRepo.getContinueLearningCourses();
    return data.when(
      success: (data) {
        emit(HomeState.successContinueLearning(data.coursesList ?? []));
      },
      failure: (errorMessage) {
        emit(
          HomeState.failureContinueLearning(
            message: errorMessage.apiErrorModel.message,
          ),
        );
      },
    );
  }

  getUpComingExams(String courseId, int page, int pageSize) async {
    emit(HomeState.loadingUpComingExams());
    final data = await homeRepo.getUpComingExams(courseId, page, pageSize);
    return data.when(
      success: (data) {
        emit(HomeState.successUpComingExams(data));
      },
      failure: (errorMessage) {
        emit(
          HomeState.failureUpComingExams(
            message: errorMessage.apiErrorModel.message,
          ),
        );
      },
    );
  }

  Future getAvailableExams(int page, int pageSize) async {
    emit(HomeState.loadingGetAvailableExams());
    final data = await homeRepo.getAvailableExams(page, pageSize);
    return data.when(
      success: (response) {
        emit(HomeState.successGetAvailableExams(response.data!));
      },
      failure: (errorMessage) {
        emit(
          HomeState.failureGetAvailableExams(
            message: errorMessage.apiErrorModel.message,
          ),
        );
      },
    );
  }

  Future getExamQuestions(String examId) async {
    emit(LoadingExamQuestions());
    final data = await homeRepo.getExamQuestions(examId);
    return data.when(
      success: (success) {
        emit(SuccessExamQuestions(success.data ?? []));
      },
      failure: (failure) {
        emit(FailureExamQuestions(message: failure.apiErrorModel.message));
      },
    );
  }

  Future<void> submitExam(String examId, Map<String, String> answers) async {
    emit(LoadingSubmitExam());
    final data = await homeRepo.submitExam(examId, answers);
    await data.when(
      success: (success) async {
        final submissionId =
            success.data?.submissionId; // was: success.submissionId
        emit(SuccessSubmitExam(submissionId));
        if (submissionId != null) {
          await getSubmissionDetails(submissionId);
        }
      },
      failure: (failure) async {
        emit(FailureSubmitExam(message: failure.apiErrorModel.message));
      },
    );
  }

  Future<void> getSubmissionDetails(String submissionId) async {
    emit(LoadingSubmissionDetails());
    final data = await homeRepo.getSubmissionDetails(submissionId);
    data.when(
      success: (success) {
        emit(SuccessSubmissionDetails(success));
      },
      failure: (failure) {
        emit(FailureSubmissionDetails(message: failure.apiErrorModel.message));
      },
    );
  }

  Future<void> getStudentSubmissions({int page = 1, int pageSize = 10}) async {
    emit(LoadingStudentSubmissions());
    final data = await homeRepo.getStudentSubmissions(page, pageSize);
    data.when(
      success: (success) {
        emit(SuccessStudentSubmissions(success));
      },
      failure: (failure) {
        emit(FailureStudentSubmissions(message: failure.apiErrorModel.message));
      },
    );
  }
}
