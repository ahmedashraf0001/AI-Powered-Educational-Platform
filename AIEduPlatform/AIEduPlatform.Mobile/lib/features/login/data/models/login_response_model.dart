import 'package:json_annotation/json_annotation.dart';
part 'login_response_model.g.dart';

@JsonSerializable()
class LoginResponseModel{
  final String? message;
  final DataModel? data;

  LoginResponseModel(this.message, this.data);

  factory LoginResponseModel.fromJson(Map<String,dynamic>json)=>_$LoginResponseModelFromJson(json);


}

@JsonSerializable()
class DataModel{
  final String? accessToken;
  final String? refreshToken;

  DataModel(this.accessToken, this.refreshToken);
  factory DataModel.fromJson(Map<String,dynamic>json)=>_$DataModelFromJson(json);
}