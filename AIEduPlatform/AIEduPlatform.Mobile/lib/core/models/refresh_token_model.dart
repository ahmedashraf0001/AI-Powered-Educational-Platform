import 'package:json_annotation/json_annotation.dart';
part 'refresh_token_model.g.dart';

@JsonSerializable()
class RefreshTokenRequest {
  final String accessToken;
  final String refreshToken;

  RefreshTokenRequest({required this.accessToken, required this.refreshToken});

  Map<String, dynamic> toJson() => _$RefreshTokenRequestToJson(this);
}
