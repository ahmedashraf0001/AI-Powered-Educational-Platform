
import 'package:freezed_annotation/freezed_annotation.dart';
part 'my_profile_model.g.dart';

@JsonSerializable()
class MyProfileModel{
  @JsonKey(name: 'data')
  final MyProfileData profileData;

  MyProfileModel({required this.profileData});
  factory MyProfileModel.fromJson(Map<String,dynamic> json)=>_$MyProfileModelFromJson(json);
}


@JsonSerializable()
class MyProfileData{
  final String? id;
  final String? email;
  final String? userName;
  final String? firstName;
  final String? lastName;
  final String? avatarUrl;
  final String? bio;

  factory MyProfileData.fromJson(Map<String,dynamic> json)=>_$MyProfileDataFromJson(json);

  MyProfileData(this.bio, {required this.id, required this.email, required this.userName, required this.firstName, required this.lastName, required this.avatarUrl});

}