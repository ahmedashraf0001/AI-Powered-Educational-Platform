import 'package:freezed_annotation/freezed_annotation.dart';
part 'register_state.freezed.dart';

@freezed
class RegisterState<T> with _$RegisterState<T>{
  factory RegisterState.initial() = _Initial;
  factory RegisterState.loading() = Loading;
  factory RegisterState.success(T data) = Success<T>;
  factory RegisterState.failure({String? message}) = Failure;
}

