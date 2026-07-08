// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'profile_state.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// dart format off
T _$identity<T>(T value) => value;
/// @nodoc
mixin _$ProfileState<T> {





@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is ProfileState<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'ProfileState<$T>()';
}


}

/// @nodoc
class $ProfileStateCopyWith<T,$Res>  {
$ProfileStateCopyWith(ProfileState<T> _, $Res Function(ProfileState<T>) __);
}


/// Adds pattern-matching-related methods to [ProfileState].
extension ProfileStatePatterns<T> on ProfileState<T> {
/// A variant of `map` that fallback to returning `orElse`.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case _:
///     return orElse();
/// }
/// ```

@optionalTypeArgs TResult maybeMap<TResult extends Object?>({TResult Function( _Initial<T> value)?  initial,TResult Function( LoadingMyProfile<T> value)?  loadingMyProfile,TResult Function( SuccessMyProfile<T> value)?  successMyProfile,TResult Function( FailureMyProfile<T> value)?  failureMyProfile,TResult Function( LoadingGetUserStatistics<T> value)?  loadingGetUserStatistics,TResult Function( SuccessGetUserStatistics<T> value)?  successGetUserStatistics,TResult Function( FailureGetUserStatistics<T> value)?  failureGetUserStatistics,TResult Function( LoadingLogout<T> value)?  loadingLogout,TResult Function( SuccessLogout<T> value)?  successLogout,TResult Function( FailureLogout<T> value)?  failureLogout,TResult Function( LoadingUpdateProfile<T> value)?  loadingUpdateProfile,TResult Function( SuccesUpdateProfile<T> value)?  successUpdateProfile,TResult Function( FailureUpdateProfile<T> value)?  failureUpdateProfile,required TResult orElse(),}){
final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial(_that);case LoadingMyProfile() when loadingMyProfile != null:
return loadingMyProfile(_that);case SuccessMyProfile() when successMyProfile != null:
return successMyProfile(_that);case FailureMyProfile() when failureMyProfile != null:
return failureMyProfile(_that);case LoadingGetUserStatistics() when loadingGetUserStatistics != null:
return loadingGetUserStatistics(_that);case SuccessGetUserStatistics() when successGetUserStatistics != null:
return successGetUserStatistics(_that);case FailureGetUserStatistics() when failureGetUserStatistics != null:
return failureGetUserStatistics(_that);case LoadingLogout() when loadingLogout != null:
return loadingLogout(_that);case SuccessLogout() when successLogout != null:
return successLogout(_that);case FailureLogout() when failureLogout != null:
return failureLogout(_that);case LoadingUpdateProfile() when loadingUpdateProfile != null:
return loadingUpdateProfile(_that);case SuccesUpdateProfile() when successUpdateProfile != null:
return successUpdateProfile(_that);case FailureUpdateProfile() when failureUpdateProfile != null:
return failureUpdateProfile(_that);case _:
  return orElse();

}
}
/// A `switch`-like method, using callbacks.
///
/// Callbacks receives the raw object, upcasted.
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case final Subclass2 value:
///     return ...;
/// }
/// ```

@optionalTypeArgs TResult map<TResult extends Object?>({required TResult Function( _Initial<T> value)  initial,required TResult Function( LoadingMyProfile<T> value)  loadingMyProfile,required TResult Function( SuccessMyProfile<T> value)  successMyProfile,required TResult Function( FailureMyProfile<T> value)  failureMyProfile,required TResult Function( LoadingGetUserStatistics<T> value)  loadingGetUserStatistics,required TResult Function( SuccessGetUserStatistics<T> value)  successGetUserStatistics,required TResult Function( FailureGetUserStatistics<T> value)  failureGetUserStatistics,required TResult Function( LoadingLogout<T> value)  loadingLogout,required TResult Function( SuccessLogout<T> value)  successLogout,required TResult Function( FailureLogout<T> value)  failureLogout,required TResult Function( LoadingUpdateProfile<T> value)  loadingUpdateProfile,required TResult Function( SuccesUpdateProfile<T> value)  successUpdateProfile,required TResult Function( FailureUpdateProfile<T> value)  failureUpdateProfile,}){
final _that = this;
switch (_that) {
case _Initial():
return initial(_that);case LoadingMyProfile():
return loadingMyProfile(_that);case SuccessMyProfile():
return successMyProfile(_that);case FailureMyProfile():
return failureMyProfile(_that);case LoadingGetUserStatistics():
return loadingGetUserStatistics(_that);case SuccessGetUserStatistics():
return successGetUserStatistics(_that);case FailureGetUserStatistics():
return failureGetUserStatistics(_that);case LoadingLogout():
return loadingLogout(_that);case SuccessLogout():
return successLogout(_that);case FailureLogout():
return failureLogout(_that);case LoadingUpdateProfile():
return loadingUpdateProfile(_that);case SuccesUpdateProfile():
return successUpdateProfile(_that);case FailureUpdateProfile():
return failureUpdateProfile(_that);case _:
  throw StateError('Unexpected subclass');

}
}
/// A variant of `map` that fallback to returning `null`.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case _:
///     return null;
/// }
/// ```

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>({TResult? Function( _Initial<T> value)?  initial,TResult? Function( LoadingMyProfile<T> value)?  loadingMyProfile,TResult? Function( SuccessMyProfile<T> value)?  successMyProfile,TResult? Function( FailureMyProfile<T> value)?  failureMyProfile,TResult? Function( LoadingGetUserStatistics<T> value)?  loadingGetUserStatistics,TResult? Function( SuccessGetUserStatistics<T> value)?  successGetUserStatistics,TResult? Function( FailureGetUserStatistics<T> value)?  failureGetUserStatistics,TResult? Function( LoadingLogout<T> value)?  loadingLogout,TResult? Function( SuccessLogout<T> value)?  successLogout,TResult? Function( FailureLogout<T> value)?  failureLogout,TResult? Function( LoadingUpdateProfile<T> value)?  loadingUpdateProfile,TResult? Function( SuccesUpdateProfile<T> value)?  successUpdateProfile,TResult? Function( FailureUpdateProfile<T> value)?  failureUpdateProfile,}){
final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial(_that);case LoadingMyProfile() when loadingMyProfile != null:
return loadingMyProfile(_that);case SuccessMyProfile() when successMyProfile != null:
return successMyProfile(_that);case FailureMyProfile() when failureMyProfile != null:
return failureMyProfile(_that);case LoadingGetUserStatistics() when loadingGetUserStatistics != null:
return loadingGetUserStatistics(_that);case SuccessGetUserStatistics() when successGetUserStatistics != null:
return successGetUserStatistics(_that);case FailureGetUserStatistics() when failureGetUserStatistics != null:
return failureGetUserStatistics(_that);case LoadingLogout() when loadingLogout != null:
return loadingLogout(_that);case SuccessLogout() when successLogout != null:
return successLogout(_that);case FailureLogout() when failureLogout != null:
return failureLogout(_that);case LoadingUpdateProfile() when loadingUpdateProfile != null:
return loadingUpdateProfile(_that);case SuccesUpdateProfile() when successUpdateProfile != null:
return successUpdateProfile(_that);case FailureUpdateProfile() when failureUpdateProfile != null:
return failureUpdateProfile(_that);case _:
  return null;

}
}
/// A variant of `when` that fallback to an `orElse` callback.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case _:
///     return orElse();
/// }
/// ```

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>({TResult Function()?  initial,TResult Function()?  loadingMyProfile,TResult Function( MyProfileData profileData)?  successMyProfile,TResult Function( String? message)?  failureMyProfile,TResult Function()?  loadingGetUserStatistics,TResult Function( UserStatisticsData userStatistics)?  successGetUserStatistics,TResult Function( String? message)?  failureGetUserStatistics,TResult Function()?  loadingLogout,TResult Function( T data)?  successLogout,TResult Function( String? message)?  failureLogout,TResult Function()?  loadingUpdateProfile,TResult Function( String? message)?  successUpdateProfile,TResult Function( String? message)?  failureUpdateProfile,required TResult orElse(),}) {final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial();case LoadingMyProfile() when loadingMyProfile != null:
return loadingMyProfile();case SuccessMyProfile() when successMyProfile != null:
return successMyProfile(_that.profileData);case FailureMyProfile() when failureMyProfile != null:
return failureMyProfile(_that.message);case LoadingGetUserStatistics() when loadingGetUserStatistics != null:
return loadingGetUserStatistics();case SuccessGetUserStatistics() when successGetUserStatistics != null:
return successGetUserStatistics(_that.userStatistics);case FailureGetUserStatistics() when failureGetUserStatistics != null:
return failureGetUserStatistics(_that.message);case LoadingLogout() when loadingLogout != null:
return loadingLogout();case SuccessLogout() when successLogout != null:
return successLogout(_that.data);case FailureLogout() when failureLogout != null:
return failureLogout(_that.message);case LoadingUpdateProfile() when loadingUpdateProfile != null:
return loadingUpdateProfile();case SuccesUpdateProfile() when successUpdateProfile != null:
return successUpdateProfile(_that.message);case FailureUpdateProfile() when failureUpdateProfile != null:
return failureUpdateProfile(_that.message);case _:
  return orElse();

}
}
/// A `switch`-like method, using callbacks.
///
/// As opposed to `map`, this offers destructuring.
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case Subclass2(:final field2):
///     return ...;
/// }
/// ```

@optionalTypeArgs TResult when<TResult extends Object?>({required TResult Function()  initial,required TResult Function()  loadingMyProfile,required TResult Function( MyProfileData profileData)  successMyProfile,required TResult Function( String? message)  failureMyProfile,required TResult Function()  loadingGetUserStatistics,required TResult Function( UserStatisticsData userStatistics)  successGetUserStatistics,required TResult Function( String? message)  failureGetUserStatistics,required TResult Function()  loadingLogout,required TResult Function( T data)  successLogout,required TResult Function( String? message)  failureLogout,required TResult Function()  loadingUpdateProfile,required TResult Function( String? message)  successUpdateProfile,required TResult Function( String? message)  failureUpdateProfile,}) {final _that = this;
switch (_that) {
case _Initial():
return initial();case LoadingMyProfile():
return loadingMyProfile();case SuccessMyProfile():
return successMyProfile(_that.profileData);case FailureMyProfile():
return failureMyProfile(_that.message);case LoadingGetUserStatistics():
return loadingGetUserStatistics();case SuccessGetUserStatistics():
return successGetUserStatistics(_that.userStatistics);case FailureGetUserStatistics():
return failureGetUserStatistics(_that.message);case LoadingLogout():
return loadingLogout();case SuccessLogout():
return successLogout(_that.data);case FailureLogout():
return failureLogout(_that.message);case LoadingUpdateProfile():
return loadingUpdateProfile();case SuccesUpdateProfile():
return successUpdateProfile(_that.message);case FailureUpdateProfile():
return failureUpdateProfile(_that.message);case _:
  throw StateError('Unexpected subclass');

}
}
/// A variant of `when` that fallback to returning `null`
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case _:
///     return null;
/// }
/// ```

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>({TResult? Function()?  initial,TResult? Function()?  loadingMyProfile,TResult? Function( MyProfileData profileData)?  successMyProfile,TResult? Function( String? message)?  failureMyProfile,TResult? Function()?  loadingGetUserStatistics,TResult? Function( UserStatisticsData userStatistics)?  successGetUserStatistics,TResult? Function( String? message)?  failureGetUserStatistics,TResult? Function()?  loadingLogout,TResult? Function( T data)?  successLogout,TResult? Function( String? message)?  failureLogout,TResult? Function()?  loadingUpdateProfile,TResult? Function( String? message)?  successUpdateProfile,TResult? Function( String? message)?  failureUpdateProfile,}) {final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial();case LoadingMyProfile() when loadingMyProfile != null:
return loadingMyProfile();case SuccessMyProfile() when successMyProfile != null:
return successMyProfile(_that.profileData);case FailureMyProfile() when failureMyProfile != null:
return failureMyProfile(_that.message);case LoadingGetUserStatistics() when loadingGetUserStatistics != null:
return loadingGetUserStatistics();case SuccessGetUserStatistics() when successGetUserStatistics != null:
return successGetUserStatistics(_that.userStatistics);case FailureGetUserStatistics() when failureGetUserStatistics != null:
return failureGetUserStatistics(_that.message);case LoadingLogout() when loadingLogout != null:
return loadingLogout();case SuccessLogout() when successLogout != null:
return successLogout(_that.data);case FailureLogout() when failureLogout != null:
return failureLogout(_that.message);case LoadingUpdateProfile() when loadingUpdateProfile != null:
return loadingUpdateProfile();case SuccesUpdateProfile() when successUpdateProfile != null:
return successUpdateProfile(_that.message);case FailureUpdateProfile() when failureUpdateProfile != null:
return failureUpdateProfile(_that.message);case _:
  return null;

}
}

}

/// @nodoc


class _Initial<T> implements ProfileState<T> {
  const _Initial();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is _Initial<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'ProfileState<$T>.initial()';
}


}




/// @nodoc


class LoadingMyProfile<T> implements ProfileState<T> {
  const LoadingMyProfile();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingMyProfile<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'ProfileState<$T>.loadingMyProfile()';
}


}




/// @nodoc


class SuccessMyProfile<T> implements ProfileState<T> {
  const SuccessMyProfile(this.profileData);
  

 final  MyProfileData profileData;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessMyProfileCopyWith<T, SuccessMyProfile<T>> get copyWith => _$SuccessMyProfileCopyWithImpl<T, SuccessMyProfile<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessMyProfile<T>&&(identical(other.profileData, profileData) || other.profileData == profileData));
}


@override
int get hashCode => Object.hash(runtimeType,profileData);

@override
String toString() {
  return 'ProfileState<$T>.successMyProfile(profileData: $profileData)';
}


}

/// @nodoc
abstract mixin class $SuccessMyProfileCopyWith<T,$Res> implements $ProfileStateCopyWith<T, $Res> {
  factory $SuccessMyProfileCopyWith(SuccessMyProfile<T> value, $Res Function(SuccessMyProfile<T>) _then) = _$SuccessMyProfileCopyWithImpl;
@useResult
$Res call({
 MyProfileData profileData
});




}
/// @nodoc
class _$SuccessMyProfileCopyWithImpl<T,$Res>
    implements $SuccessMyProfileCopyWith<T, $Res> {
  _$SuccessMyProfileCopyWithImpl(this._self, this._then);

  final SuccessMyProfile<T> _self;
  final $Res Function(SuccessMyProfile<T>) _then;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? profileData = null,}) {
  return _then(SuccessMyProfile<T>(
null == profileData ? _self.profileData : profileData // ignore: cast_nullable_to_non_nullable
as MyProfileData,
  ));
}


}

/// @nodoc


class FailureMyProfile<T> implements ProfileState<T> {
  const FailureMyProfile({this.message});
  

 final  String? message;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureMyProfileCopyWith<T, FailureMyProfile<T>> get copyWith => _$FailureMyProfileCopyWithImpl<T, FailureMyProfile<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureMyProfile<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'ProfileState<$T>.failureMyProfile(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureMyProfileCopyWith<T,$Res> implements $ProfileStateCopyWith<T, $Res> {
  factory $FailureMyProfileCopyWith(FailureMyProfile<T> value, $Res Function(FailureMyProfile<T>) _then) = _$FailureMyProfileCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureMyProfileCopyWithImpl<T,$Res>
    implements $FailureMyProfileCopyWith<T, $Res> {
  _$FailureMyProfileCopyWithImpl(this._self, this._then);

  final FailureMyProfile<T> _self;
  final $Res Function(FailureMyProfile<T>) _then;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureMyProfile<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingGetUserStatistics<T> implements ProfileState<T> {
  const LoadingGetUserStatistics();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingGetUserStatistics<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'ProfileState<$T>.loadingGetUserStatistics()';
}


}




/// @nodoc


class SuccessGetUserStatistics<T> implements ProfileState<T> {
  const SuccessGetUserStatistics(this.userStatistics);
  

 final  UserStatisticsData userStatistics;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessGetUserStatisticsCopyWith<T, SuccessGetUserStatistics<T>> get copyWith => _$SuccessGetUserStatisticsCopyWithImpl<T, SuccessGetUserStatistics<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessGetUserStatistics<T>&&(identical(other.userStatistics, userStatistics) || other.userStatistics == userStatistics));
}


@override
int get hashCode => Object.hash(runtimeType,userStatistics);

@override
String toString() {
  return 'ProfileState<$T>.successGetUserStatistics(userStatistics: $userStatistics)';
}


}

/// @nodoc
abstract mixin class $SuccessGetUserStatisticsCopyWith<T,$Res> implements $ProfileStateCopyWith<T, $Res> {
  factory $SuccessGetUserStatisticsCopyWith(SuccessGetUserStatistics<T> value, $Res Function(SuccessGetUserStatistics<T>) _then) = _$SuccessGetUserStatisticsCopyWithImpl;
@useResult
$Res call({
 UserStatisticsData userStatistics
});




}
/// @nodoc
class _$SuccessGetUserStatisticsCopyWithImpl<T,$Res>
    implements $SuccessGetUserStatisticsCopyWith<T, $Res> {
  _$SuccessGetUserStatisticsCopyWithImpl(this._self, this._then);

  final SuccessGetUserStatistics<T> _self;
  final $Res Function(SuccessGetUserStatistics<T>) _then;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? userStatistics = null,}) {
  return _then(SuccessGetUserStatistics<T>(
null == userStatistics ? _self.userStatistics : userStatistics // ignore: cast_nullable_to_non_nullable
as UserStatisticsData,
  ));
}


}

/// @nodoc


class FailureGetUserStatistics<T> implements ProfileState<T> {
  const FailureGetUserStatistics({this.message});
  

 final  String? message;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureGetUserStatisticsCopyWith<T, FailureGetUserStatistics<T>> get copyWith => _$FailureGetUserStatisticsCopyWithImpl<T, FailureGetUserStatistics<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureGetUserStatistics<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'ProfileState<$T>.failureGetUserStatistics(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureGetUserStatisticsCopyWith<T,$Res> implements $ProfileStateCopyWith<T, $Res> {
  factory $FailureGetUserStatisticsCopyWith(FailureGetUserStatistics<T> value, $Res Function(FailureGetUserStatistics<T>) _then) = _$FailureGetUserStatisticsCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureGetUserStatisticsCopyWithImpl<T,$Res>
    implements $FailureGetUserStatisticsCopyWith<T, $Res> {
  _$FailureGetUserStatisticsCopyWithImpl(this._self, this._then);

  final FailureGetUserStatistics<T> _self;
  final $Res Function(FailureGetUserStatistics<T>) _then;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureGetUserStatistics<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingLogout<T> implements ProfileState<T> {
  const LoadingLogout();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingLogout<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'ProfileState<$T>.loadingLogout()';
}


}




/// @nodoc


class SuccessLogout<T> implements ProfileState<T> {
  const SuccessLogout(this.data);
  

 final  T data;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessLogoutCopyWith<T, SuccessLogout<T>> get copyWith => _$SuccessLogoutCopyWithImpl<T, SuccessLogout<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessLogout<T>&&const DeepCollectionEquality().equals(other.data, data));
}


@override
int get hashCode => Object.hash(runtimeType,const DeepCollectionEquality().hash(data));

@override
String toString() {
  return 'ProfileState<$T>.successLogout(data: $data)';
}


}

/// @nodoc
abstract mixin class $SuccessLogoutCopyWith<T,$Res> implements $ProfileStateCopyWith<T, $Res> {
  factory $SuccessLogoutCopyWith(SuccessLogout<T> value, $Res Function(SuccessLogout<T>) _then) = _$SuccessLogoutCopyWithImpl;
@useResult
$Res call({
 T data
});




}
/// @nodoc
class _$SuccessLogoutCopyWithImpl<T,$Res>
    implements $SuccessLogoutCopyWith<T, $Res> {
  _$SuccessLogoutCopyWithImpl(this._self, this._then);

  final SuccessLogout<T> _self;
  final $Res Function(SuccessLogout<T>) _then;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? data = freezed,}) {
  return _then(SuccessLogout<T>(
freezed == data ? _self.data : data // ignore: cast_nullable_to_non_nullable
as T,
  ));
}


}

/// @nodoc


class FailureLogout<T> implements ProfileState<T> {
  const FailureLogout({this.message});
  

 final  String? message;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureLogoutCopyWith<T, FailureLogout<T>> get copyWith => _$FailureLogoutCopyWithImpl<T, FailureLogout<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureLogout<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'ProfileState<$T>.failureLogout(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureLogoutCopyWith<T,$Res> implements $ProfileStateCopyWith<T, $Res> {
  factory $FailureLogoutCopyWith(FailureLogout<T> value, $Res Function(FailureLogout<T>) _then) = _$FailureLogoutCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureLogoutCopyWithImpl<T,$Res>
    implements $FailureLogoutCopyWith<T, $Res> {
  _$FailureLogoutCopyWithImpl(this._self, this._then);

  final FailureLogout<T> _self;
  final $Res Function(FailureLogout<T>) _then;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureLogout<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingUpdateProfile<T> implements ProfileState<T> {
  const LoadingUpdateProfile();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingUpdateProfile<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'ProfileState<$T>.loadingUpdateProfile()';
}


}




/// @nodoc


class SuccesUpdateProfile<T> implements ProfileState<T> {
  const SuccesUpdateProfile(this.message);
  

 final  String? message;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccesUpdateProfileCopyWith<T, SuccesUpdateProfile<T>> get copyWith => _$SuccesUpdateProfileCopyWithImpl<T, SuccesUpdateProfile<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccesUpdateProfile<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'ProfileState<$T>.successUpdateProfile(message: $message)';
}


}

/// @nodoc
abstract mixin class $SuccesUpdateProfileCopyWith<T,$Res> implements $ProfileStateCopyWith<T, $Res> {
  factory $SuccesUpdateProfileCopyWith(SuccesUpdateProfile<T> value, $Res Function(SuccesUpdateProfile<T>) _then) = _$SuccesUpdateProfileCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$SuccesUpdateProfileCopyWithImpl<T,$Res>
    implements $SuccesUpdateProfileCopyWith<T, $Res> {
  _$SuccesUpdateProfileCopyWithImpl(this._self, this._then);

  final SuccesUpdateProfile<T> _self;
  final $Res Function(SuccesUpdateProfile<T>) _then;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(SuccesUpdateProfile<T>(
freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class FailureUpdateProfile<T> implements ProfileState<T> {
  const FailureUpdateProfile({this.message});
  

 final  String? message;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureUpdateProfileCopyWith<T, FailureUpdateProfile<T>> get copyWith => _$FailureUpdateProfileCopyWithImpl<T, FailureUpdateProfile<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureUpdateProfile<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'ProfileState<$T>.failureUpdateProfile(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureUpdateProfileCopyWith<T,$Res> implements $ProfileStateCopyWith<T, $Res> {
  factory $FailureUpdateProfileCopyWith(FailureUpdateProfile<T> value, $Res Function(FailureUpdateProfile<T>) _then) = _$FailureUpdateProfileCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureUpdateProfileCopyWithImpl<T,$Res>
    implements $FailureUpdateProfileCopyWith<T, $Res> {
  _$FailureUpdateProfileCopyWithImpl(this._self, this._then);

  final FailureUpdateProfile<T> _self;
  final $Res Function(FailureUpdateProfile<T>) _then;

/// Create a copy of ProfileState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureUpdateProfile<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

// dart format on
