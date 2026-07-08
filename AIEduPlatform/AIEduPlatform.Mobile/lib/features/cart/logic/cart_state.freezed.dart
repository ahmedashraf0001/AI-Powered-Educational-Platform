// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'cart_state.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// dart format off
T _$identity<T>(T value) => value;
/// @nodoc
mixin _$CartState<T> {





@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is CartState<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CartState<$T>()';
}


}

/// @nodoc
class $CartStateCopyWith<T,$Res>  {
$CartStateCopyWith(CartState<T> _, $Res Function(CartState<T>) __);
}


/// Adds pattern-matching-related methods to [CartState].
extension CartStatePatterns<T> on CartState<T> {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>({TResult Function( _Initial<T> value)?  initial,TResult Function( LoadingGetMyCart<T> value)?  loadingGetMyCart,TResult Function( SuccessGetMyCart<T> value)?  successGetMyCart,TResult Function( FailureGetMyCart<T> value)?  failureGetMyCart,TResult Function( LoadingRemoveCourseFromCart<T> value)?  loadingRemoveCourseFromCart,TResult Function( SuccessRemoveCourseFromCart<T> value)?  successRemoveCourseFromCart,TResult Function( FailureRemoveCourseFromCart<T> value)?  failureRemoveCourseFromCart,TResult Function( LoadingClearCart<T> value)?  loadingClearCart,TResult Function( SuccessClearCart<T> value)?  successClearCart,TResult Function( FailureClearCart<T> value)?  failureClearCart,TResult Function( LoadingStartCheckout<T> value)?  loadingStartCheckout,TResult Function( SuccessStartCheckout<T> value)?  successStartCheckout,TResult Function( FailureStartCheckout<T> value)?  failureStartCheckout,TResult Function( LoadingOrderStatus<T> value)?  loadingOrderStatus,TResult Function( SuccessOrderStatus<T> value)?  successOrderStatus,TResult Function( FailureOrderStatus<T> value)?  failureOrderStatus,TResult Function( LoadingGetMyCourses<T> value)?  loadingGetMyCourses,TResult Function( SuccessGetMyCourses<T> value)?  successGetMyCourses,TResult Function( FailureGetMyCourses<T> value)?  failureGetMyCourses,required TResult orElse(),}){
final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial(_that);case LoadingGetMyCart() when loadingGetMyCart != null:
return loadingGetMyCart(_that);case SuccessGetMyCart() when successGetMyCart != null:
return successGetMyCart(_that);case FailureGetMyCart() when failureGetMyCart != null:
return failureGetMyCart(_that);case LoadingRemoveCourseFromCart() when loadingRemoveCourseFromCart != null:
return loadingRemoveCourseFromCart(_that);case SuccessRemoveCourseFromCart() when successRemoveCourseFromCart != null:
return successRemoveCourseFromCart(_that);case FailureRemoveCourseFromCart() when failureRemoveCourseFromCart != null:
return failureRemoveCourseFromCart(_that);case LoadingClearCart() when loadingClearCart != null:
return loadingClearCart(_that);case SuccessClearCart() when successClearCart != null:
return successClearCart(_that);case FailureClearCart() when failureClearCart != null:
return failureClearCart(_that);case LoadingStartCheckout() when loadingStartCheckout != null:
return loadingStartCheckout(_that);case SuccessStartCheckout() when successStartCheckout != null:
return successStartCheckout(_that);case FailureStartCheckout() when failureStartCheckout != null:
return failureStartCheckout(_that);case LoadingOrderStatus() when loadingOrderStatus != null:
return loadingOrderStatus(_that);case SuccessOrderStatus() when successOrderStatus != null:
return successOrderStatus(_that);case FailureOrderStatus() when failureOrderStatus != null:
return failureOrderStatus(_that);case LoadingGetMyCourses() when loadingGetMyCourses != null:
return loadingGetMyCourses(_that);case SuccessGetMyCourses() when successGetMyCourses != null:
return successGetMyCourses(_that);case FailureGetMyCourses() when failureGetMyCourses != null:
return failureGetMyCourses(_that);case _:
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

@optionalTypeArgs TResult map<TResult extends Object?>({required TResult Function( _Initial<T> value)  initial,required TResult Function( LoadingGetMyCart<T> value)  loadingGetMyCart,required TResult Function( SuccessGetMyCart<T> value)  successGetMyCart,required TResult Function( FailureGetMyCart<T> value)  failureGetMyCart,required TResult Function( LoadingRemoveCourseFromCart<T> value)  loadingRemoveCourseFromCart,required TResult Function( SuccessRemoveCourseFromCart<T> value)  successRemoveCourseFromCart,required TResult Function( FailureRemoveCourseFromCart<T> value)  failureRemoveCourseFromCart,required TResult Function( LoadingClearCart<T> value)  loadingClearCart,required TResult Function( SuccessClearCart<T> value)  successClearCart,required TResult Function( FailureClearCart<T> value)  failureClearCart,required TResult Function( LoadingStartCheckout<T> value)  loadingStartCheckout,required TResult Function( SuccessStartCheckout<T> value)  successStartCheckout,required TResult Function( FailureStartCheckout<T> value)  failureStartCheckout,required TResult Function( LoadingOrderStatus<T> value)  loadingOrderStatus,required TResult Function( SuccessOrderStatus<T> value)  successOrderStatus,required TResult Function( FailureOrderStatus<T> value)  failureOrderStatus,required TResult Function( LoadingGetMyCourses<T> value)  loadingGetMyCourses,required TResult Function( SuccessGetMyCourses<T> value)  successGetMyCourses,required TResult Function( FailureGetMyCourses<T> value)  failureGetMyCourses,}){
final _that = this;
switch (_that) {
case _Initial():
return initial(_that);case LoadingGetMyCart():
return loadingGetMyCart(_that);case SuccessGetMyCart():
return successGetMyCart(_that);case FailureGetMyCart():
return failureGetMyCart(_that);case LoadingRemoveCourseFromCart():
return loadingRemoveCourseFromCart(_that);case SuccessRemoveCourseFromCart():
return successRemoveCourseFromCart(_that);case FailureRemoveCourseFromCart():
return failureRemoveCourseFromCart(_that);case LoadingClearCart():
return loadingClearCart(_that);case SuccessClearCart():
return successClearCart(_that);case FailureClearCart():
return failureClearCart(_that);case LoadingStartCheckout():
return loadingStartCheckout(_that);case SuccessStartCheckout():
return successStartCheckout(_that);case FailureStartCheckout():
return failureStartCheckout(_that);case LoadingOrderStatus():
return loadingOrderStatus(_that);case SuccessOrderStatus():
return successOrderStatus(_that);case FailureOrderStatus():
return failureOrderStatus(_that);case LoadingGetMyCourses():
return loadingGetMyCourses(_that);case SuccessGetMyCourses():
return successGetMyCourses(_that);case FailureGetMyCourses():
return failureGetMyCourses(_that);case _:
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>({TResult? Function( _Initial<T> value)?  initial,TResult? Function( LoadingGetMyCart<T> value)?  loadingGetMyCart,TResult? Function( SuccessGetMyCart<T> value)?  successGetMyCart,TResult? Function( FailureGetMyCart<T> value)?  failureGetMyCart,TResult? Function( LoadingRemoveCourseFromCart<T> value)?  loadingRemoveCourseFromCart,TResult? Function( SuccessRemoveCourseFromCart<T> value)?  successRemoveCourseFromCart,TResult? Function( FailureRemoveCourseFromCart<T> value)?  failureRemoveCourseFromCart,TResult? Function( LoadingClearCart<T> value)?  loadingClearCart,TResult? Function( SuccessClearCart<T> value)?  successClearCart,TResult? Function( FailureClearCart<T> value)?  failureClearCart,TResult? Function( LoadingStartCheckout<T> value)?  loadingStartCheckout,TResult? Function( SuccessStartCheckout<T> value)?  successStartCheckout,TResult? Function( FailureStartCheckout<T> value)?  failureStartCheckout,TResult? Function( LoadingOrderStatus<T> value)?  loadingOrderStatus,TResult? Function( SuccessOrderStatus<T> value)?  successOrderStatus,TResult? Function( FailureOrderStatus<T> value)?  failureOrderStatus,TResult? Function( LoadingGetMyCourses<T> value)?  loadingGetMyCourses,TResult? Function( SuccessGetMyCourses<T> value)?  successGetMyCourses,TResult? Function( FailureGetMyCourses<T> value)?  failureGetMyCourses,}){
final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial(_that);case LoadingGetMyCart() when loadingGetMyCart != null:
return loadingGetMyCart(_that);case SuccessGetMyCart() when successGetMyCart != null:
return successGetMyCart(_that);case FailureGetMyCart() when failureGetMyCart != null:
return failureGetMyCart(_that);case LoadingRemoveCourseFromCart() when loadingRemoveCourseFromCart != null:
return loadingRemoveCourseFromCart(_that);case SuccessRemoveCourseFromCart() when successRemoveCourseFromCart != null:
return successRemoveCourseFromCart(_that);case FailureRemoveCourseFromCart() when failureRemoveCourseFromCart != null:
return failureRemoveCourseFromCart(_that);case LoadingClearCart() when loadingClearCart != null:
return loadingClearCart(_that);case SuccessClearCart() when successClearCart != null:
return successClearCart(_that);case FailureClearCart() when failureClearCart != null:
return failureClearCart(_that);case LoadingStartCheckout() when loadingStartCheckout != null:
return loadingStartCheckout(_that);case SuccessStartCheckout() when successStartCheckout != null:
return successStartCheckout(_that);case FailureStartCheckout() when failureStartCheckout != null:
return failureStartCheckout(_that);case LoadingOrderStatus() when loadingOrderStatus != null:
return loadingOrderStatus(_that);case SuccessOrderStatus() when successOrderStatus != null:
return successOrderStatus(_that);case FailureOrderStatus() when failureOrderStatus != null:
return failureOrderStatus(_that);case LoadingGetMyCourses() when loadingGetMyCourses != null:
return loadingGetMyCourses(_that);case SuccessGetMyCourses() when successGetMyCourses != null:
return successGetMyCourses(_that);case FailureGetMyCourses() when failureGetMyCourses != null:
return failureGetMyCourses(_that);case _:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>({TResult Function()?  initial,TResult Function()?  loadingGetMyCart,TResult Function( MyCartData cartData)?  successGetMyCart,TResult Function( String? message)?  failureGetMyCart,TResult Function()?  loadingRemoveCourseFromCart,TResult Function( String? message)?  successRemoveCourseFromCart,TResult Function( String? message)?  failureRemoveCourseFromCart,TResult Function()?  loadingClearCart,TResult Function( String? message)?  successClearCart,TResult Function( String? message)?  failureClearCart,TResult Function()?  loadingStartCheckout,TResult Function( CheckoutResponseData checkoutData)?  successStartCheckout,TResult Function( String? message)?  failureStartCheckout,TResult Function()?  loadingOrderStatus,TResult Function( OrderStatusData orderStatusData)?  successOrderStatus,TResult Function( String? message)?  failureOrderStatus,TResult Function()?  loadingGetMyCourses,TResult Function( MyCoursesResponseModel myCoursesReponseModel)?  successGetMyCourses,TResult Function( String? message)?  failureGetMyCourses,required TResult orElse(),}) {final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial();case LoadingGetMyCart() when loadingGetMyCart != null:
return loadingGetMyCart();case SuccessGetMyCart() when successGetMyCart != null:
return successGetMyCart(_that.cartData);case FailureGetMyCart() when failureGetMyCart != null:
return failureGetMyCart(_that.message);case LoadingRemoveCourseFromCart() when loadingRemoveCourseFromCart != null:
return loadingRemoveCourseFromCart();case SuccessRemoveCourseFromCart() when successRemoveCourseFromCart != null:
return successRemoveCourseFromCart(_that.message);case FailureRemoveCourseFromCart() when failureRemoveCourseFromCart != null:
return failureRemoveCourseFromCart(_that.message);case LoadingClearCart() when loadingClearCart != null:
return loadingClearCart();case SuccessClearCart() when successClearCart != null:
return successClearCart(_that.message);case FailureClearCart() when failureClearCart != null:
return failureClearCart(_that.message);case LoadingStartCheckout() when loadingStartCheckout != null:
return loadingStartCheckout();case SuccessStartCheckout() when successStartCheckout != null:
return successStartCheckout(_that.checkoutData);case FailureStartCheckout() when failureStartCheckout != null:
return failureStartCheckout(_that.message);case LoadingOrderStatus() when loadingOrderStatus != null:
return loadingOrderStatus();case SuccessOrderStatus() when successOrderStatus != null:
return successOrderStatus(_that.orderStatusData);case FailureOrderStatus() when failureOrderStatus != null:
return failureOrderStatus(_that.message);case LoadingGetMyCourses() when loadingGetMyCourses != null:
return loadingGetMyCourses();case SuccessGetMyCourses() when successGetMyCourses != null:
return successGetMyCourses(_that.myCoursesReponseModel);case FailureGetMyCourses() when failureGetMyCourses != null:
return failureGetMyCourses(_that.message);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>({required TResult Function()  initial,required TResult Function()  loadingGetMyCart,required TResult Function( MyCartData cartData)  successGetMyCart,required TResult Function( String? message)  failureGetMyCart,required TResult Function()  loadingRemoveCourseFromCart,required TResult Function( String? message)  successRemoveCourseFromCart,required TResult Function( String? message)  failureRemoveCourseFromCart,required TResult Function()  loadingClearCart,required TResult Function( String? message)  successClearCart,required TResult Function( String? message)  failureClearCart,required TResult Function()  loadingStartCheckout,required TResult Function( CheckoutResponseData checkoutData)  successStartCheckout,required TResult Function( String? message)  failureStartCheckout,required TResult Function()  loadingOrderStatus,required TResult Function( OrderStatusData orderStatusData)  successOrderStatus,required TResult Function( String? message)  failureOrderStatus,required TResult Function()  loadingGetMyCourses,required TResult Function( MyCoursesResponseModel myCoursesReponseModel)  successGetMyCourses,required TResult Function( String? message)  failureGetMyCourses,}) {final _that = this;
switch (_that) {
case _Initial():
return initial();case LoadingGetMyCart():
return loadingGetMyCart();case SuccessGetMyCart():
return successGetMyCart(_that.cartData);case FailureGetMyCart():
return failureGetMyCart(_that.message);case LoadingRemoveCourseFromCart():
return loadingRemoveCourseFromCart();case SuccessRemoveCourseFromCart():
return successRemoveCourseFromCart(_that.message);case FailureRemoveCourseFromCart():
return failureRemoveCourseFromCart(_that.message);case LoadingClearCart():
return loadingClearCart();case SuccessClearCart():
return successClearCart(_that.message);case FailureClearCart():
return failureClearCart(_that.message);case LoadingStartCheckout():
return loadingStartCheckout();case SuccessStartCheckout():
return successStartCheckout(_that.checkoutData);case FailureStartCheckout():
return failureStartCheckout(_that.message);case LoadingOrderStatus():
return loadingOrderStatus();case SuccessOrderStatus():
return successOrderStatus(_that.orderStatusData);case FailureOrderStatus():
return failureOrderStatus(_that.message);case LoadingGetMyCourses():
return loadingGetMyCourses();case SuccessGetMyCourses():
return successGetMyCourses(_that.myCoursesReponseModel);case FailureGetMyCourses():
return failureGetMyCourses(_that.message);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>({TResult? Function()?  initial,TResult? Function()?  loadingGetMyCart,TResult? Function( MyCartData cartData)?  successGetMyCart,TResult? Function( String? message)?  failureGetMyCart,TResult? Function()?  loadingRemoveCourseFromCart,TResult? Function( String? message)?  successRemoveCourseFromCart,TResult? Function( String? message)?  failureRemoveCourseFromCart,TResult? Function()?  loadingClearCart,TResult? Function( String? message)?  successClearCart,TResult? Function( String? message)?  failureClearCart,TResult? Function()?  loadingStartCheckout,TResult? Function( CheckoutResponseData checkoutData)?  successStartCheckout,TResult? Function( String? message)?  failureStartCheckout,TResult? Function()?  loadingOrderStatus,TResult? Function( OrderStatusData orderStatusData)?  successOrderStatus,TResult? Function( String? message)?  failureOrderStatus,TResult? Function()?  loadingGetMyCourses,TResult? Function( MyCoursesResponseModel myCoursesReponseModel)?  successGetMyCourses,TResult? Function( String? message)?  failureGetMyCourses,}) {final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial();case LoadingGetMyCart() when loadingGetMyCart != null:
return loadingGetMyCart();case SuccessGetMyCart() when successGetMyCart != null:
return successGetMyCart(_that.cartData);case FailureGetMyCart() when failureGetMyCart != null:
return failureGetMyCart(_that.message);case LoadingRemoveCourseFromCart() when loadingRemoveCourseFromCart != null:
return loadingRemoveCourseFromCart();case SuccessRemoveCourseFromCart() when successRemoveCourseFromCart != null:
return successRemoveCourseFromCart(_that.message);case FailureRemoveCourseFromCart() when failureRemoveCourseFromCart != null:
return failureRemoveCourseFromCart(_that.message);case LoadingClearCart() when loadingClearCart != null:
return loadingClearCart();case SuccessClearCart() when successClearCart != null:
return successClearCart(_that.message);case FailureClearCart() when failureClearCart != null:
return failureClearCart(_that.message);case LoadingStartCheckout() when loadingStartCheckout != null:
return loadingStartCheckout();case SuccessStartCheckout() when successStartCheckout != null:
return successStartCheckout(_that.checkoutData);case FailureStartCheckout() when failureStartCheckout != null:
return failureStartCheckout(_that.message);case LoadingOrderStatus() when loadingOrderStatus != null:
return loadingOrderStatus();case SuccessOrderStatus() when successOrderStatus != null:
return successOrderStatus(_that.orderStatusData);case FailureOrderStatus() when failureOrderStatus != null:
return failureOrderStatus(_that.message);case LoadingGetMyCourses() when loadingGetMyCourses != null:
return loadingGetMyCourses();case SuccessGetMyCourses() when successGetMyCourses != null:
return successGetMyCourses(_that.myCoursesReponseModel);case FailureGetMyCourses() when failureGetMyCourses != null:
return failureGetMyCourses(_that.message);case _:
  return null;

}
}

}

/// @nodoc


class _Initial<T> implements CartState<T> {
  const _Initial();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is _Initial<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CartState<$T>.initial()';
}


}




/// @nodoc


class LoadingGetMyCart<T> implements CartState<T> {
  const LoadingGetMyCart();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingGetMyCart<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CartState<$T>.loadingGetMyCart()';
}


}




/// @nodoc


class SuccessGetMyCart<T> implements CartState<T> {
  const SuccessGetMyCart(this.cartData);
  

 final  MyCartData cartData;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessGetMyCartCopyWith<T, SuccessGetMyCart<T>> get copyWith => _$SuccessGetMyCartCopyWithImpl<T, SuccessGetMyCart<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessGetMyCart<T>&&(identical(other.cartData, cartData) || other.cartData == cartData));
}


@override
int get hashCode => Object.hash(runtimeType,cartData);

@override
String toString() {
  return 'CartState<$T>.successGetMyCart(cartData: $cartData)';
}


}

/// @nodoc
abstract mixin class $SuccessGetMyCartCopyWith<T,$Res> implements $CartStateCopyWith<T, $Res> {
  factory $SuccessGetMyCartCopyWith(SuccessGetMyCart<T> value, $Res Function(SuccessGetMyCart<T>) _then) = _$SuccessGetMyCartCopyWithImpl;
@useResult
$Res call({
 MyCartData cartData
});




}
/// @nodoc
class _$SuccessGetMyCartCopyWithImpl<T,$Res>
    implements $SuccessGetMyCartCopyWith<T, $Res> {
  _$SuccessGetMyCartCopyWithImpl(this._self, this._then);

  final SuccessGetMyCart<T> _self;
  final $Res Function(SuccessGetMyCart<T>) _then;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? cartData = null,}) {
  return _then(SuccessGetMyCart<T>(
null == cartData ? _self.cartData : cartData // ignore: cast_nullable_to_non_nullable
as MyCartData,
  ));
}


}

/// @nodoc


class FailureGetMyCart<T> implements CartState<T> {
  const FailureGetMyCart({this.message});
  

 final  String? message;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureGetMyCartCopyWith<T, FailureGetMyCart<T>> get copyWith => _$FailureGetMyCartCopyWithImpl<T, FailureGetMyCart<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureGetMyCart<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'CartState<$T>.failureGetMyCart(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureGetMyCartCopyWith<T,$Res> implements $CartStateCopyWith<T, $Res> {
  factory $FailureGetMyCartCopyWith(FailureGetMyCart<T> value, $Res Function(FailureGetMyCart<T>) _then) = _$FailureGetMyCartCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureGetMyCartCopyWithImpl<T,$Res>
    implements $FailureGetMyCartCopyWith<T, $Res> {
  _$FailureGetMyCartCopyWithImpl(this._self, this._then);

  final FailureGetMyCart<T> _self;
  final $Res Function(FailureGetMyCart<T>) _then;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureGetMyCart<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingRemoveCourseFromCart<T> implements CartState<T> {
  const LoadingRemoveCourseFromCart();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingRemoveCourseFromCart<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CartState<$T>.loadingRemoveCourseFromCart()';
}


}




/// @nodoc


class SuccessRemoveCourseFromCart<T> implements CartState<T> {
  const SuccessRemoveCourseFromCart({this.message});
  

 final  String? message;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessRemoveCourseFromCartCopyWith<T, SuccessRemoveCourseFromCart<T>> get copyWith => _$SuccessRemoveCourseFromCartCopyWithImpl<T, SuccessRemoveCourseFromCart<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessRemoveCourseFromCart<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'CartState<$T>.successRemoveCourseFromCart(message: $message)';
}


}

/// @nodoc
abstract mixin class $SuccessRemoveCourseFromCartCopyWith<T,$Res> implements $CartStateCopyWith<T, $Res> {
  factory $SuccessRemoveCourseFromCartCopyWith(SuccessRemoveCourseFromCart<T> value, $Res Function(SuccessRemoveCourseFromCart<T>) _then) = _$SuccessRemoveCourseFromCartCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$SuccessRemoveCourseFromCartCopyWithImpl<T,$Res>
    implements $SuccessRemoveCourseFromCartCopyWith<T, $Res> {
  _$SuccessRemoveCourseFromCartCopyWithImpl(this._self, this._then);

  final SuccessRemoveCourseFromCart<T> _self;
  final $Res Function(SuccessRemoveCourseFromCart<T>) _then;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(SuccessRemoveCourseFromCart<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class FailureRemoveCourseFromCart<T> implements CartState<T> {
  const FailureRemoveCourseFromCart({this.message});
  

 final  String? message;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureRemoveCourseFromCartCopyWith<T, FailureRemoveCourseFromCart<T>> get copyWith => _$FailureRemoveCourseFromCartCopyWithImpl<T, FailureRemoveCourseFromCart<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureRemoveCourseFromCart<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'CartState<$T>.failureRemoveCourseFromCart(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureRemoveCourseFromCartCopyWith<T,$Res> implements $CartStateCopyWith<T, $Res> {
  factory $FailureRemoveCourseFromCartCopyWith(FailureRemoveCourseFromCart<T> value, $Res Function(FailureRemoveCourseFromCart<T>) _then) = _$FailureRemoveCourseFromCartCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureRemoveCourseFromCartCopyWithImpl<T,$Res>
    implements $FailureRemoveCourseFromCartCopyWith<T, $Res> {
  _$FailureRemoveCourseFromCartCopyWithImpl(this._self, this._then);

  final FailureRemoveCourseFromCart<T> _self;
  final $Res Function(FailureRemoveCourseFromCart<T>) _then;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureRemoveCourseFromCart<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingClearCart<T> implements CartState<T> {
  const LoadingClearCart();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingClearCart<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CartState<$T>.loadingClearCart()';
}


}




/// @nodoc


class SuccessClearCart<T> implements CartState<T> {
  const SuccessClearCart({this.message});
  

 final  String? message;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessClearCartCopyWith<T, SuccessClearCart<T>> get copyWith => _$SuccessClearCartCopyWithImpl<T, SuccessClearCart<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessClearCart<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'CartState<$T>.successClearCart(message: $message)';
}


}

/// @nodoc
abstract mixin class $SuccessClearCartCopyWith<T,$Res> implements $CartStateCopyWith<T, $Res> {
  factory $SuccessClearCartCopyWith(SuccessClearCart<T> value, $Res Function(SuccessClearCart<T>) _then) = _$SuccessClearCartCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$SuccessClearCartCopyWithImpl<T,$Res>
    implements $SuccessClearCartCopyWith<T, $Res> {
  _$SuccessClearCartCopyWithImpl(this._self, this._then);

  final SuccessClearCart<T> _self;
  final $Res Function(SuccessClearCart<T>) _then;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(SuccessClearCart<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class FailureClearCart<T> implements CartState<T> {
  const FailureClearCart({this.message});
  

 final  String? message;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureClearCartCopyWith<T, FailureClearCart<T>> get copyWith => _$FailureClearCartCopyWithImpl<T, FailureClearCart<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureClearCart<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'CartState<$T>.failureClearCart(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureClearCartCopyWith<T,$Res> implements $CartStateCopyWith<T, $Res> {
  factory $FailureClearCartCopyWith(FailureClearCart<T> value, $Res Function(FailureClearCart<T>) _then) = _$FailureClearCartCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureClearCartCopyWithImpl<T,$Res>
    implements $FailureClearCartCopyWith<T, $Res> {
  _$FailureClearCartCopyWithImpl(this._self, this._then);

  final FailureClearCart<T> _self;
  final $Res Function(FailureClearCart<T>) _then;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureClearCart<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingStartCheckout<T> implements CartState<T> {
  const LoadingStartCheckout();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingStartCheckout<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CartState<$T>.loadingStartCheckout()';
}


}




/// @nodoc


class SuccessStartCheckout<T> implements CartState<T> {
  const SuccessStartCheckout(this.checkoutData);
  

 final  CheckoutResponseData checkoutData;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessStartCheckoutCopyWith<T, SuccessStartCheckout<T>> get copyWith => _$SuccessStartCheckoutCopyWithImpl<T, SuccessStartCheckout<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessStartCheckout<T>&&(identical(other.checkoutData, checkoutData) || other.checkoutData == checkoutData));
}


@override
int get hashCode => Object.hash(runtimeType,checkoutData);

@override
String toString() {
  return 'CartState<$T>.successStartCheckout(checkoutData: $checkoutData)';
}


}

/// @nodoc
abstract mixin class $SuccessStartCheckoutCopyWith<T,$Res> implements $CartStateCopyWith<T, $Res> {
  factory $SuccessStartCheckoutCopyWith(SuccessStartCheckout<T> value, $Res Function(SuccessStartCheckout<T>) _then) = _$SuccessStartCheckoutCopyWithImpl;
@useResult
$Res call({
 CheckoutResponseData checkoutData
});




}
/// @nodoc
class _$SuccessStartCheckoutCopyWithImpl<T,$Res>
    implements $SuccessStartCheckoutCopyWith<T, $Res> {
  _$SuccessStartCheckoutCopyWithImpl(this._self, this._then);

  final SuccessStartCheckout<T> _self;
  final $Res Function(SuccessStartCheckout<T>) _then;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? checkoutData = null,}) {
  return _then(SuccessStartCheckout<T>(
null == checkoutData ? _self.checkoutData : checkoutData // ignore: cast_nullable_to_non_nullable
as CheckoutResponseData,
  ));
}


}

/// @nodoc


class FailureStartCheckout<T> implements CartState<T> {
  const FailureStartCheckout({this.message});
  

 final  String? message;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureStartCheckoutCopyWith<T, FailureStartCheckout<T>> get copyWith => _$FailureStartCheckoutCopyWithImpl<T, FailureStartCheckout<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureStartCheckout<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'CartState<$T>.failureStartCheckout(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureStartCheckoutCopyWith<T,$Res> implements $CartStateCopyWith<T, $Res> {
  factory $FailureStartCheckoutCopyWith(FailureStartCheckout<T> value, $Res Function(FailureStartCheckout<T>) _then) = _$FailureStartCheckoutCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureStartCheckoutCopyWithImpl<T,$Res>
    implements $FailureStartCheckoutCopyWith<T, $Res> {
  _$FailureStartCheckoutCopyWithImpl(this._self, this._then);

  final FailureStartCheckout<T> _self;
  final $Res Function(FailureStartCheckout<T>) _then;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureStartCheckout<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingOrderStatus<T> implements CartState<T> {
  const LoadingOrderStatus();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingOrderStatus<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CartState<$T>.loadingOrderStatus()';
}


}




/// @nodoc


class SuccessOrderStatus<T> implements CartState<T> {
  const SuccessOrderStatus(this.orderStatusData);
  

 final  OrderStatusData orderStatusData;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessOrderStatusCopyWith<T, SuccessOrderStatus<T>> get copyWith => _$SuccessOrderStatusCopyWithImpl<T, SuccessOrderStatus<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessOrderStatus<T>&&(identical(other.orderStatusData, orderStatusData) || other.orderStatusData == orderStatusData));
}


@override
int get hashCode => Object.hash(runtimeType,orderStatusData);

@override
String toString() {
  return 'CartState<$T>.successOrderStatus(orderStatusData: $orderStatusData)';
}


}

/// @nodoc
abstract mixin class $SuccessOrderStatusCopyWith<T,$Res> implements $CartStateCopyWith<T, $Res> {
  factory $SuccessOrderStatusCopyWith(SuccessOrderStatus<T> value, $Res Function(SuccessOrderStatus<T>) _then) = _$SuccessOrderStatusCopyWithImpl;
@useResult
$Res call({
 OrderStatusData orderStatusData
});




}
/// @nodoc
class _$SuccessOrderStatusCopyWithImpl<T,$Res>
    implements $SuccessOrderStatusCopyWith<T, $Res> {
  _$SuccessOrderStatusCopyWithImpl(this._self, this._then);

  final SuccessOrderStatus<T> _self;
  final $Res Function(SuccessOrderStatus<T>) _then;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? orderStatusData = null,}) {
  return _then(SuccessOrderStatus<T>(
null == orderStatusData ? _self.orderStatusData : orderStatusData // ignore: cast_nullable_to_non_nullable
as OrderStatusData,
  ));
}


}

/// @nodoc


class FailureOrderStatus<T> implements CartState<T> {
  const FailureOrderStatus({this.message});
  

 final  String? message;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureOrderStatusCopyWith<T, FailureOrderStatus<T>> get copyWith => _$FailureOrderStatusCopyWithImpl<T, FailureOrderStatus<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureOrderStatus<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'CartState<$T>.failureOrderStatus(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureOrderStatusCopyWith<T,$Res> implements $CartStateCopyWith<T, $Res> {
  factory $FailureOrderStatusCopyWith(FailureOrderStatus<T> value, $Res Function(FailureOrderStatus<T>) _then) = _$FailureOrderStatusCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureOrderStatusCopyWithImpl<T,$Res>
    implements $FailureOrderStatusCopyWith<T, $Res> {
  _$FailureOrderStatusCopyWithImpl(this._self, this._then);

  final FailureOrderStatus<T> _self;
  final $Res Function(FailureOrderStatus<T>) _then;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureOrderStatus<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingGetMyCourses<T> implements CartState<T> {
  const LoadingGetMyCourses();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingGetMyCourses<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'CartState<$T>.loadingGetMyCourses()';
}


}




/// @nodoc


class SuccessGetMyCourses<T> implements CartState<T> {
  const SuccessGetMyCourses(this.myCoursesReponseModel);
  

 final  MyCoursesResponseModel myCoursesReponseModel;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessGetMyCoursesCopyWith<T, SuccessGetMyCourses<T>> get copyWith => _$SuccessGetMyCoursesCopyWithImpl<T, SuccessGetMyCourses<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessGetMyCourses<T>&&(identical(other.myCoursesReponseModel, myCoursesReponseModel) || other.myCoursesReponseModel == myCoursesReponseModel));
}


@override
int get hashCode => Object.hash(runtimeType,myCoursesReponseModel);

@override
String toString() {
  return 'CartState<$T>.successGetMyCourses(myCoursesReponseModel: $myCoursesReponseModel)';
}


}

/// @nodoc
abstract mixin class $SuccessGetMyCoursesCopyWith<T,$Res> implements $CartStateCopyWith<T, $Res> {
  factory $SuccessGetMyCoursesCopyWith(SuccessGetMyCourses<T> value, $Res Function(SuccessGetMyCourses<T>) _then) = _$SuccessGetMyCoursesCopyWithImpl;
@useResult
$Res call({
 MyCoursesResponseModel myCoursesReponseModel
});




}
/// @nodoc
class _$SuccessGetMyCoursesCopyWithImpl<T,$Res>
    implements $SuccessGetMyCoursesCopyWith<T, $Res> {
  _$SuccessGetMyCoursesCopyWithImpl(this._self, this._then);

  final SuccessGetMyCourses<T> _self;
  final $Res Function(SuccessGetMyCourses<T>) _then;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? myCoursesReponseModel = null,}) {
  return _then(SuccessGetMyCourses<T>(
null == myCoursesReponseModel ? _self.myCoursesReponseModel : myCoursesReponseModel // ignore: cast_nullable_to_non_nullable
as MyCoursesResponseModel,
  ));
}


}

/// @nodoc


class FailureGetMyCourses<T> implements CartState<T> {
  const FailureGetMyCourses({this.message});
  

 final  String? message;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureGetMyCoursesCopyWith<T, FailureGetMyCourses<T>> get copyWith => _$FailureGetMyCoursesCopyWithImpl<T, FailureGetMyCourses<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureGetMyCourses<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'CartState<$T>.failureGetMyCourses(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureGetMyCoursesCopyWith<T,$Res> implements $CartStateCopyWith<T, $Res> {
  factory $FailureGetMyCoursesCopyWith(FailureGetMyCourses<T> value, $Res Function(FailureGetMyCourses<T>) _then) = _$FailureGetMyCoursesCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureGetMyCoursesCopyWithImpl<T,$Res>
    implements $FailureGetMyCoursesCopyWith<T, $Res> {
  _$FailureGetMyCoursesCopyWithImpl(this._self, this._then);

  final FailureGetMyCourses<T> _self;
  final $Res Function(FailureGetMyCourses<T>) _then;

/// Create a copy of CartState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureGetMyCourses<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

// dart format on
