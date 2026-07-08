import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../networking/api_constants.dart';

//secured flutter storage

class SecureStorageHelper {
  final storage = FlutterSecureStorage(
    aOptions: AndroidOptions(encryptedSharedPreferences: true),
  );

  Future saveToken(String token) async {
    await storage.write(key: ApiKeys.token, value: token);
  }

  Future<void> saveRefreshToken(String token) async {
    await storage.write(key: ApiKeys.refreshToken, value: token);
  }

  Future<String?> getToken({required String key}) async {
    return await storage.read(key: key);
  }

  Future removeToken() async {
    await storage.delete(key: ApiKeys.token);
  }

  Future<void> removeRefreshToken() async {
    await storage.delete(key: ApiKeys.refreshToken);
  }

  Future<void> clearAllTokens() async {
    await storage.delete(key: ApiKeys.token);
    await storage.delete(key: ApiKeys.refreshToken);
  }
}
