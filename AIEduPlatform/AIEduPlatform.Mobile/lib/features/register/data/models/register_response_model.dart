import 'package:freezed_annotation/freezed_annotation.dart';

part 'register_response_model.g.dart';

@JsonSerializable()
class RegisterResponseModel {
  final String? data;
  final String? message;

  RegisterResponseModel(this.data, this.message);

  factory RegisterResponseModel.fromJson(Map<String, dynamic> json) =>
      _$RegisterResponseModelFromJson(json);
}
