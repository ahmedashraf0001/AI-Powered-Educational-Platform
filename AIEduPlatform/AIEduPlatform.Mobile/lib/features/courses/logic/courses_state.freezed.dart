// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'courses_state.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// dart format off
T _$identity<T>(T value) => value;
/// @nodoc
mixin _$CoursesState<T> {





@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is CoursesState<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CoursesState<$T>()';
}


}

/// @nodoc
class $CoursesStateCopyWith<T,$Res>  {
$CoursesStateCopyWith(CoursesState<T> _, $Res Function(CoursesState<T>) __);
}


/// Adds pattern-matching-related methods to [CoursesState].
extension CoursesStatePatterns<T> on CoursesState<T> {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>({TResult Function( _Initial<T> value)?  initial,TResult Function( LoadingGetAllCourses<T> value)?  loadingGetAllCourses,TResult Function( SuccessGetAllCourses<T> value)?  successGetAllCourses,TResult Function( FailureGetAllCourses<T> value)?  failureGetAllCourses,TResult Function( LoadingAddCourseToCart<T> value)?  loadingAddCourseToCart,TResult Function( SuccessAddCourseToCart<T> value)?  successAddCourseToCart,TResult Function( FailureAddCourseToCart<T> value)?  failureAddCourseToCart,TResult Function( LoadingCourseLectures<T> value)?  loadingCourseLectures,TResult Function( SuccessCourseLectures<T> value)?  successCourseLectures,TResult Function( FailureCourseLectures<T> value)?  failureCourseLectures,TResult Function( LoadingStartSession<T> value)?  loadingStartSession,TResult Function( SuccessStartSession<T> value)?  successStartSession,TResult Function( FailureStartSession<T> value)?  failureStartSession,required TResult orElse(),}){
final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial(_that);case LoadingGetAllCourses() when loadingGetAllCourses != null:
return loadingGetAllCourses(_that);case SuccessGetAllCourses() when successGetAllCourses != null:
return successGetAllCourses(_that);case FailureGetAllCourses() when failureGetAllCourses != null:
return failureGetAllCourses(_that);case LoadingAddCourseToCart() when loadingAddCourseToCart != null:
return loadingAddCourseToCart(_that);case SuccessAddCourseToCart() when successAddCourseToCart != null:
return successAddCourseToCart(_that);case FailureAddCourseToCart() when failureAddCourseToCart != null:
return failureAddCourseToCart(_that);case LoadingCourseLectures() when loadingCourseLectures != null:
return loadingCourseLectures(_that);case SuccessCourseLectures() when successCourseLectures != null:
return successCourseLectures(_that);case FailureCourseLectures() when failureCourseLectures != null:
return failureCourseLectures(_that);case LoadingStartSession() when loadingStartSession != null:
return loadingStartSession(_that);case SuccessStartSession() when successStartSession != null:
return successStartSession(_that);case FailureStartSession() when failureStartSession != null:
return failureStartSession(_that);case _:
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

@optionalTypeArgs TResult map<TResult extends Object?>({required TResult Function( _Initial<T> value)  initial,required TResult Function( LoadingGetAllCourses<T> value)  loadingGetAllCourses,required TResult Function( SuccessGetAllCourses<T> value)  successGetAllCourses,required TResult Function( FailureGetAllCourses<T> value)  failureGetAllCourses,required TResult Function( LoadingAddCourseToCart<T> value)  loadingAddCourseToCart,required TResult Function( SuccessAddCourseToCart<T> value)  successAddCourseToCart,required TResult Function( FailureAddCourseToCart<T> value)  failureAddCourseToCart,required TResult Function( LoadingCourseLectures<T> value)  loadingCourseLectures,required TResult Function( SuccessCourseLectures<T> value)  successCourseLectures,required TResult Function( FailureCourseLectures<T> value)  failureCourseLectures,required TResult Function( LoadingStartSession<T> value)  loadingStartSession,required TResult Function( SuccessStartSession<T> value)  successStartSession,required TResult Function( FailureStartSession<T> value)  failureStartSession,}){
final _that = this;
switch (_that) {
case _Initial():
return initial(_that);case LoadingGetAllCourses():
return loadingGetAllCourses(_that);case SuccessGetAllCourses():
return successGetAllCourses(_that);case FailureGetAllCourses():
return failureGetAllCourses(_that);case LoadingAddCourseToCart():
return loadingAddCourseToCart(_that);case SuccessAddCourseToCart():
return successAddCourseToCart(_that);case FailureAddCourseToCart():
return failureAddCourseToCart(_that);case LoadingCourseLectures():
return loadingCourseLectures(_that);case SuccessCourseLectures():
return successCourseLectures(_that);case FailureCourseLectures():
return failureCourseLectures(_that);case LoadingStartSession():
return loadingStartSession(_that);case SuccessStartSession():
return successStartSession(_that);case FailureStartSession():
return failureStartSession(_that);case _:
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>({TResult? Function( _Initial<T> value)?  initial,TResult? Function( LoadingGetAllCourses<T> value)?  loadingGetAllCourses,TResult? Function( SuccessGetAllCourses<T> value)?  successGetAllCourses,TResult? Function( FailureGetAllCourses<T> value)?  failureGetAllCourses,TResult? Function( LoadingAddCourseToCart<T> value)?  loadingAddCourseToCart,TResult? Function( SuccessAddCourseToCart<T> value)?  successAddCourseToCart,TResult? Function( FailureAddCourseToCart<T> value)?  failureAddCourseToCart,TResult? Function( LoadingCourseLectures<T> value)?  loadingCourseLectures,TResult? Function( SuccessCourseLectures<T> value)?  successCourseLectures,TResult? Function( FailureCourseLectures<T> value)?  failureCourseLectures,TResult? Function( LoadingStartSession<T> value)?  loadingStartSession,TResult? Function( SuccessStartSession<T> value)?  successStartSession,TResult? Function( FailureStartSession<T> value)?  failureStartSession,}){
final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial(_that);case LoadingGetAllCourses() when loadingGetAllCourses != null:
return loadingGetAllCourses(_that);case SuccessGetAllCourses() when successGetAllCourses != null:
return successGetAllCourses(_that);case FailureGetAllCourses() when failureGetAllCourses != null:
return failureGetAllCourses(_that);case LoadingAddCourseToCart() when loadingAddCourseToCart != null:
return loadingAddCourseToCart(_that);case SuccessAddCourseToCart() when successAddCourseToCart != null:
return successAddCourseToCart(_that);case FailureAddCourseToCart() when failureAddCourseToCart != null:
return failureAddCourseToCart(_that);case LoadingCourseLectures() when loadingCourseLectures != null:
return loadingCourseLectures(_that);case SuccessCourseLectures() when successCourseLectures != null:
return successCourseLectures(_that);case FailureCourseLectures() when failureCourseLectures != null:
return failureCourseLectures(_that);case LoadingStartSession() when loadingStartSession != null:
return loadingStartSession(_that);case SuccessStartSession() when successStartSession != null:
return successStartSession(_that);case FailureStartSession() when failureStartSession != null:
return failureStartSession(_that);case _:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>({TResult Function()?  initial,TResult Function()?  loadingGetAllCourses,TResult Function( List<AllCoursesItemModel> coursesData)?  successGetAllCourses,TResult Function( String? message)?  failureGetAllCourses,TResult Function()?  loadingAddCourseToCart,TResult Function( AddCourseToCartResponseModel responseModel)?  successAddCourseToCart,TResult Function( String? message)?  failureAddCourseToCart,TResult Function()?  loadingCourseLectures,TResult Function( List<CourseLectureMaterials> courseLecturerMaterials)?  successCourseLectures,TResult Function( String? message)?  failureCourseLectures,TResult Function()?  loadingStartSession,TResult Function( StartSessionResponseModel dataModel)?  successStartSession,TResult Function( String? message)?  failureStartSession,required TResult orElse(),}) {final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial();case LoadingGetAllCourses() when loadingGetAllCourses != null:
return loadingGetAllCourses();case SuccessGetAllCourses() when successGetAllCourses != null:
return successGetAllCourses(_that.coursesData);case FailureGetAllCourses() when failureGetAllCourses != null:
return failureGetAllCourses(_that.message);case LoadingAddCourseToCart() when loadingAddCourseToCart != null:
return loadingAddCourseToCart();case SuccessAddCourseToCart() when successAddCourseToCart != null:
return successAddCourseToCart(_that.responseModel);case FailureAddCourseToCart() when failureAddCourseToCart != null:
return failureAddCourseToCart(_that.message);case LoadingCourseLectures() when loadingCourseLectures != null:
return loadingCourseLectures();case SuccessCourseLectures() when successCourseLectures != null:
return successCourseLectures(_that.courseLecturerMaterials);case FailureCourseLectures() when failureCourseLectures != null:
return failureCourseLectures(_that.message);case LoadingStartSession() when loadingStartSession != null:
return loadingStartSession();case SuccessStartSession() when successStartSession != null:
return successStartSession(_that.dataModel);case FailureStartSession() when failureStartSession != null:
return failureStartSession(_that.message);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>({required TResult Function()  initial,required TResult Function()  loadingGetAllCourses,required TResult Function( List<AllCoursesItemModel> coursesData)  successGetAllCourses,required TResult Function( String? message)  failureGetAllCourses,required TResult Function()  loadingAddCourseToCart,required TResult Function( AddCourseToCartResponseModel responseModel)  successAddCourseToCart,required TResult Function( String? message)  failureAddCourseToCart,required TResult Function()  loadingCourseLectures,required TResult Function( List<CourseLectureMaterials> courseLecturerMaterials)  successCourseLectures,required TResult Function( String? message)  failureCourseLectures,required TResult Function()  loadingStartSession,required TResult Function( StartSessionResponseModel dataModel)  successStartSession,required TResult Function( String? message)  failureStartSession,}) {final _that = this;
switch (_that) {
case _Initial():
return initial();case LoadingGetAllCourses():
return loadingGetAllCourses();case SuccessGetAllCourses():
return successGetAllCourses(_that.coursesData);case FailureGetAllCourses():
return failureGetAllCourses(_that.message);case LoadingAddCourseToCart():
return loadingAddCourseToCart();case SuccessAddCourseToCart():
return successAddCourseToCart(_that.responseModel);case FailureAddCourseToCart():
return failureAddCourseToCart(_that.message);case LoadingCourseLectures():
return loadingCourseLectures();case SuccessCourseLectures():
return successCourseLectures(_that.courseLecturerMaterials);case FailureCourseLectures():
return failureCourseLectures(_that.message);case LoadingStartSession():
return loadingStartSession();case SuccessStartSession():
return successStartSession(_that.dataModel);case FailureStartSession():
return failureStartSession(_that.message);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>({TResult? Function()?  initial,TResult? Function()?  loadingGetAllCourses,TResult? Function( List<AllCoursesItemModel> coursesData)?  successGetAllCourses,TResult? Function( String? message)?  failureGetAllCourses,TResult? Function()?  loadingAddCourseToCart,TResult? Function( AddCourseToCartResponseModel responseModel)?  successAddCourseToCart,TResult? Function( String? message)?  failureAddCourseToCart,TResult? Function()?  loadingCourseLectures,TResult? Function( List<CourseLectureMaterials> courseLecturerMaterials)?  successCourseLectures,TResult? Function( String? message)?  failureCourseLectures,TResult? Function()?  loadingStartSession,TResult? Function( StartSessionResponseModel dataModel)?  successStartSession,TResult? Function( String? message)?  failureStartSession,}) {final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial();case LoadingGetAllCourses() when loadingGetAllCourses != null:
return loadingGetAllCourses();case SuccessGetAllCourses() when successGetAllCourses != null:
return successGetAllCourses(_that.coursesData);case FailureGetAllCourses() when failureGetAllCourses != null:
return failureGetAllCourses(_that.message);case LoadingAddCourseToCart() when loadingAddCourseToCart != null:
return loadingAddCourseToCart();case SuccessAddCourseToCart() when successAddCourseToCart != null:
return successAddCourseToCart(_that.responseModel);case FailureAddCourseToCart() when failureAddCourseToCart != null:
return failureAddCourseToCart(_that.message);case LoadingCourseLectures() when loadingCourseLectures != null:
return loadingCourseLectures();case SuccessCourseLectures() when successCourseLectures != null:
return successCourseLectures(_that.courseLecturerMaterials);case FailureCourseLectures() when failureCourseLectures != null:
return failureCourseLectures(_that.message);case LoadingStartSession() when loadingStartSession != null:
return loadingStartSession();case SuccessStartSession() when successStartSession != null:
return successStartSession(_that.dataModel);case FailureStartSession() when failureStartSession != null:
return failureStartSession(_that.message);case _:
  return null;

}
}

}

/// @nodoc


class _Initial<T> implements CoursesState<T> {
  const _Initial();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is _Initial<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CoursesState<$T>.initial()';
}


}




/// @nodoc


class LoadingGetAllCourses<T> implements CoursesState<T> {
  const LoadingGetAllCourses();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingGetAllCourses<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CoursesState<$T>.loadingGetAllCourses()';
}


}




/// @nodoc


class SuccessGetAllCourses<T> implements CoursesState<T> {
  const SuccessGetAllCourses(final  List<AllCoursesItemModel> coursesData): _coursesData = coursesData;
  

 final  List<AllCoursesItemModel> _coursesData;
 List<AllCoursesItemModel> get coursesData {
  if (_coursesData is EqualUnmodifiableListView) return _coursesData;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_coursesData);
}


/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessGetAllCoursesCopyWith<T, SuccessGetAllCourses<T>> get copyWith => _$SuccessGetAllCoursesCopyWithImpl<T, SuccessGetAllCourses<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessGetAllCourses<T>&&const DeepCollectionEquality().equals(other._coursesData, _coursesData));
}


@override
int get hashCode => Object.hash(runtimeType,const DeepCollectionEquality().hash(_coursesData));

@override
String toString() {
  return 'CoursesState<$T>.successGetAllCourses(coursesData: $coursesData)';
}


}

/// @nodoc
abstract mixin class $SuccessGetAllCoursesCopyWith<T,$Res> implements $CoursesStateCopyWith<T, $Res> {
  factory $SuccessGetAllCoursesCopyWith(SuccessGetAllCourses<T> value, $Res Function(SuccessGetAllCourses<T>) _then) = _$SuccessGetAllCoursesCopyWithImpl;
@useResult
$Res call({
 List<AllCoursesItemModel> coursesData
});




}
/// @nodoc
class _$SuccessGetAllCoursesCopyWithImpl<T,$Res>
    implements $SuccessGetAllCoursesCopyWith<T, $Res> {
  _$SuccessGetAllCoursesCopyWithImpl(this._self, this._then);

  final SuccessGetAllCourses<T> _self;
  final $Res Function(SuccessGetAllCourses<T>) _then;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? coursesData = null,}) {
  return _then(SuccessGetAllCourses<T>(
null == coursesData ? _self._coursesData : coursesData // ignore: cast_nullable_to_non_nullable
as List<AllCoursesItemModel>,
  ));
}


}

/// @nodoc


class FailureGetAllCourses<T> implements CoursesState<T> {
  const FailureGetAllCourses({this.message});
  

 final  String? message;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureGetAllCoursesCopyWith<T, FailureGetAllCourses<T>> get copyWith => _$FailureGetAllCoursesCopyWithImpl<T, FailureGetAllCourses<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureGetAllCourses<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'CoursesState<$T>.failureGetAllCourses(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureGetAllCoursesCopyWith<T,$Res> implements $CoursesStateCopyWith<T, $Res> {
  factory $FailureGetAllCoursesCopyWith(FailureGetAllCourses<T> value, $Res Function(FailureGetAllCourses<T>) _then) = _$FailureGetAllCoursesCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureGetAllCoursesCopyWithImpl<T,$Res>
    implements $FailureGetAllCoursesCopyWith<T, $Res> {
  _$FailureGetAllCoursesCopyWithImpl(this._self, this._then);

  final FailureGetAllCourses<T> _self;
  final $Res Function(FailureGetAllCourses<T>) _then;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureGetAllCourses<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingAddCourseToCart<T> implements CoursesState<T> {
  const LoadingAddCourseToCart();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingAddCourseToCart<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CoursesState<$T>.loadingAddCourseToCart()';
}


}




/// @nodoc


class SuccessAddCourseToCart<T> implements CoursesState<T> {
  const SuccessAddCourseToCart(this.responseModel);
  

 final  AddCourseToCartResponseModel responseModel;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessAddCourseToCartCopyWith<T, SuccessAddCourseToCart<T>> get copyWith => _$SuccessAddCourseToCartCopyWithImpl<T, SuccessAddCourseToCart<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessAddCourseToCart<T>&&(identical(other.responseModel, responseModel) || other.responseModel == responseModel));
}


@override
int get hashCode => Object.hash(runtimeType,responseModel);

@override
String toString() {
  return 'CoursesState<$T>.successAddCourseToCart(responseModel: $responseModel)';
}


}

/// @nodoc
abstract mixin class $SuccessAddCourseToCartCopyWith<T,$Res> implements $CoursesStateCopyWith<T, $Res> {
  factory $SuccessAddCourseToCartCopyWith(SuccessAddCourseToCart<T> value, $Res Function(SuccessAddCourseToCart<T>) _then) = _$SuccessAddCourseToCartCopyWithImpl;
@useResult
$Res call({
 AddCourseToCartResponseModel responseModel
});




}
/// @nodoc
class _$SuccessAddCourseToCartCopyWithImpl<T,$Res>
    implements $SuccessAddCourseToCartCopyWith<T, $Res> {
  _$SuccessAddCourseToCartCopyWithImpl(this._self, this._then);

  final SuccessAddCourseToCart<T> _self;
  final $Res Function(SuccessAddCourseToCart<T>) _then;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? responseModel = null,}) {
  return _then(SuccessAddCourseToCart<T>(
null == responseModel ? _self.responseModel : responseModel // ignore: cast_nullable_to_non_nullable
as AddCourseToCartResponseModel,
  ));
}


}

/// @nodoc


class FailureAddCourseToCart<T> implements CoursesState<T> {
  const FailureAddCourseToCart({this.message});
  

 final  String? message;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureAddCourseToCartCopyWith<T, FailureAddCourseToCart<T>> get copyWith => _$FailureAddCourseToCartCopyWithImpl<T, FailureAddCourseToCart<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureAddCourseToCart<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'CoursesState<$T>.failureAddCourseToCart(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureAddCourseToCartCopyWith<T,$Res> implements $CoursesStateCopyWith<T, $Res> {
  factory $FailureAddCourseToCartCopyWith(FailureAddCourseToCart<T> value, $Res Function(FailureAddCourseToCart<T>) _then) = _$FailureAddCourseToCartCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureAddCourseToCartCopyWithImpl<T,$Res>
    implements $FailureAddCourseToCartCopyWith<T, $Res> {
  _$FailureAddCourseToCartCopyWithImpl(this._self, this._then);

  final FailureAddCourseToCart<T> _self;
  final $Res Function(FailureAddCourseToCart<T>) _then;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureAddCourseToCart<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingCourseLectures<T> implements CoursesState<T> {
  const LoadingCourseLectures();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingCourseLectures<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CoursesState<$T>.loadingCourseLectures()';
}


}




/// @nodoc


class SuccessCourseLectures<T> implements CoursesState<T> {
  const SuccessCourseLectures(final  List<CourseLectureMaterials> courseLecturerMaterials): _courseLecturerMaterials = courseLecturerMaterials;
  

 final  List<CourseLectureMaterials> _courseLecturerMaterials;
 List<CourseLectureMaterials> get courseLecturerMaterials {
  if (_courseLecturerMaterials is EqualUnmodifiableListView) return _courseLecturerMaterials;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_courseLecturerMaterials);
}


/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessCourseLecturesCopyWith<T, SuccessCourseLectures<T>> get copyWith => _$SuccessCourseLecturesCopyWithImpl<T, SuccessCourseLectures<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessCourseLectures<T>&&const DeepCollectionEquality().equals(other._courseLecturerMaterials, _courseLecturerMaterials));
}


@override
int get hashCode => Object.hash(runtimeType,const DeepCollectionEquality().hash(_courseLecturerMaterials));

@override
String toString() {
  return 'CoursesState<$T>.successCourseLectures(courseLecturerMaterials: $courseLecturerMaterials)';
}


}

/// @nodoc
abstract mixin class $SuccessCourseLecturesCopyWith<T,$Res> implements $CoursesStateCopyWith<T, $Res> {
  factory $SuccessCourseLecturesCopyWith(SuccessCourseLectures<T> value, $Res Function(SuccessCourseLectures<T>) _then) = _$SuccessCourseLecturesCopyWithImpl;
@useResult
$Res call({
 List<CourseLectureMaterials> courseLecturerMaterials
});




}
/// @nodoc
class _$SuccessCourseLecturesCopyWithImpl<T,$Res>
    implements $SuccessCourseLecturesCopyWith<T, $Res> {
  _$SuccessCourseLecturesCopyWithImpl(this._self, this._then);

  final SuccessCourseLectures<T> _self;
  final $Res Function(SuccessCourseLectures<T>) _then;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? courseLecturerMaterials = null,}) {
  return _then(SuccessCourseLectures<T>(
null == courseLecturerMaterials ? _self._courseLecturerMaterials : courseLecturerMaterials // ignore: cast_nullable_to_non_nullable
as List<CourseLectureMaterials>,
  ));
}


}

/// @nodoc


class FailureCourseLectures<T> implements CoursesState<T> {
  const FailureCourseLectures({this.message});
  

 final  String? message;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureCourseLecturesCopyWith<T, FailureCourseLectures<T>> get copyWith => _$FailureCourseLecturesCopyWithImpl<T, FailureCourseLectures<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureCourseLectures<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'CoursesState<$T>.failureCourseLectures(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureCourseLecturesCopyWith<T,$Res> implements $CoursesStateCopyWith<T, $Res> {
  factory $FailureCourseLecturesCopyWith(FailureCourseLectures<T> value, $Res Function(FailureCourseLectures<T>) _then) = _$FailureCourseLecturesCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureCourseLecturesCopyWithImpl<T,$Res>
    implements $FailureCourseLecturesCopyWith<T, $Res> {
  _$FailureCourseLecturesCopyWithImpl(this._self, this._then);

  final FailureCourseLectures<T> _self;
  final $Res Function(FailureCourseLectures<T>) _then;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureCourseLectures<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingStartSession<T> implements CoursesState<T> {
  const LoadingStartSession();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingStartSession<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CoursesState<$T>.loadingStartSession()';
}


}




/// @nodoc


class SuccessStartSession<T> implements CoursesState<T> {
  const SuccessStartSession(this.dataModel);
  

 final  StartSessionResponseModel dataModel;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessStartSessionCopyWith<T, SuccessStartSession<T>> get copyWith => _$SuccessStartSessionCopyWithImpl<T, SuccessStartSession<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessStartSession<T>&&(identical(other.dataModel, dataModel) || other.dataModel == dataModel));
}


@override
int get hashCode => Object.hash(runtimeType,dataModel);

@override
String toString() {
  return 'CoursesState<$T>.successStartSession(dataModel: $dataModel)';
}


}

/// @nodoc
abstract mixin class $SuccessStartSessionCopyWith<T,$Res> implements $CoursesStateCopyWith<T, $Res> {
  factory $SuccessStartSessionCopyWith(SuccessStartSession<T> value, $Res Function(SuccessStartSession<T>) _then) = _$SuccessStartSessionCopyWithImpl;
@useResult
$Res call({
 StartSessionResponseModel dataModel
});




}
/// @nodoc
class _$SuccessStartSessionCopyWithImpl<T,$Res>
    implements $SuccessStartSessionCopyWith<T, $Res> {
  _$SuccessStartSessionCopyWithImpl(this._self, this._then);

  final SuccessStartSession<T> _self;
  final $Res Function(SuccessStartSession<T>) _then;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? dataModel = null,}) {
  return _then(SuccessStartSession<T>(
null == dataModel ? _self.dataModel : dataModel // ignore: cast_nullable_to_non_nullable
as StartSessionResponseModel,
  ));
}


}

/// @nodoc


class FailureStartSession<T> implements CoursesState<T> {
  const FailureStartSession({this.message});
  

 final  String? message;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureStartSessionCopyWith<T, FailureStartSession<T>> get copyWith => _$FailureStartSessionCopyWithImpl<T, FailureStartSession<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureStartSession<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'CoursesState<$T>.failureStartSession(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureStartSessionCopyWith<T,$Res> implements $CoursesStateCopyWith<T, $Res> {
  factory $FailureStartSessionCopyWith(FailureStartSession<T> value, $Res Function(FailureStartSession<T>) _then) = _$FailureStartSessionCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureStartSessionCopyWithImpl<T,$Res>
    implements $FailureStartSessionCopyWith<T, $Res> {
  _$FailureStartSessionCopyWithImpl(this._self, this._then);

  final FailureStartSession<T> _self;
  final $Res Function(FailureStartSession<T>) _then;

/// Create a copy of CoursesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureStartSession<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

// dart format on
