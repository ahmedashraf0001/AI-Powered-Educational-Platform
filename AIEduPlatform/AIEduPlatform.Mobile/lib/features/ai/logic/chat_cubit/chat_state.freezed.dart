// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'chat_state.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// dart format off
T _$identity<T>(T value) => value;
/// @nodoc
mixin _$ChatState<T> {





@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is ChatState<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'ChatState<$T>()';
}


}

/// @nodoc
class $ChatStateCopyWith<T,$Res>  {
$ChatStateCopyWith(ChatState<T> _, $Res Function(ChatState<T>) __);
}


/// Adds pattern-matching-related methods to [ChatState].
extension ChatStatePatterns<T> on ChatState<T> {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>({TResult Function( _Initial<T> value)?  initial,TResult Function( LoadingSendChatMessage<T> value)?  loadingSendChatMessage,TResult Function( SuccessSendChatMessage<T> value)?  successSendChatMessage,TResult Function( FailureSendChatMessage<T> value)?  failureSendChatMessage,required TResult orElse(),}){
final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial(_that);case LoadingSendChatMessage() when loadingSendChatMessage != null:
return loadingSendChatMessage(_that);case SuccessSendChatMessage() when successSendChatMessage != null:
return successSendChatMessage(_that);case FailureSendChatMessage() when failureSendChatMessage != null:
return failureSendChatMessage(_that);case _:
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

@optionalTypeArgs TResult map<TResult extends Object?>({required TResult Function( _Initial<T> value)  initial,required TResult Function( LoadingSendChatMessage<T> value)  loadingSendChatMessage,required TResult Function( SuccessSendChatMessage<T> value)  successSendChatMessage,required TResult Function( FailureSendChatMessage<T> value)  failureSendChatMessage,}){
final _that = this;
switch (_that) {
case _Initial():
return initial(_that);case LoadingSendChatMessage():
return loadingSendChatMessage(_that);case SuccessSendChatMessage():
return successSendChatMessage(_that);case FailureSendChatMessage():
return failureSendChatMessage(_that);case _:
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>({TResult? Function( _Initial<T> value)?  initial,TResult? Function( LoadingSendChatMessage<T> value)?  loadingSendChatMessage,TResult? Function( SuccessSendChatMessage<T> value)?  successSendChatMessage,TResult? Function( FailureSendChatMessage<T> value)?  failureSendChatMessage,}){
final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial(_that);case LoadingSendChatMessage() when loadingSendChatMessage != null:
return loadingSendChatMessage(_that);case SuccessSendChatMessage() when successSendChatMessage != null:
return successSendChatMessage(_that);case FailureSendChatMessage() when failureSendChatMessage != null:
return failureSendChatMessage(_that);case _:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>({TResult Function()?  initial,TResult Function()?  loadingSendChatMessage,TResult Function( ChatMessageResponseModel response,  String fullMessage)?  successSendChatMessage,TResult Function( String? message)?  failureSendChatMessage,required TResult orElse(),}) {final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial();case LoadingSendChatMessage() when loadingSendChatMessage != null:
return loadingSendChatMessage();case SuccessSendChatMessage() when successSendChatMessage != null:
return successSendChatMessage(_that.response,_that.fullMessage);case FailureSendChatMessage() when failureSendChatMessage != null:
return failureSendChatMessage(_that.message);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>({required TResult Function()  initial,required TResult Function()  loadingSendChatMessage,required TResult Function( ChatMessageResponseModel response,  String fullMessage)  successSendChatMessage,required TResult Function( String? message)  failureSendChatMessage,}) {final _that = this;
switch (_that) {
case _Initial():
return initial();case LoadingSendChatMessage():
return loadingSendChatMessage();case SuccessSendChatMessage():
return successSendChatMessage(_that.response,_that.fullMessage);case FailureSendChatMessage():
return failureSendChatMessage(_that.message);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>({TResult? Function()?  initial,TResult? Function()?  loadingSendChatMessage,TResult? Function( ChatMessageResponseModel response,  String fullMessage)?  successSendChatMessage,TResult? Function( String? message)?  failureSendChatMessage,}) {final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial();case LoadingSendChatMessage() when loadingSendChatMessage != null:
return loadingSendChatMessage();case SuccessSendChatMessage() when successSendChatMessage != null:
return successSendChatMessage(_that.response,_that.fullMessage);case FailureSendChatMessage() when failureSendChatMessage != null:
return failureSendChatMessage(_that.message);case _:
  return null;

}
}

}

/// @nodoc


class _Initial<T> implements ChatState<T> {
  const _Initial();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is _Initial<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'ChatState<$T>.initial()';
}


}




/// @nodoc


class LoadingSendChatMessage<T> implements ChatState<T> {
  const LoadingSendChatMessage();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingSendChatMessage<T>);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'ChatState<$T>.loadingSendChatMessage()';
}


}




/// @nodoc


class SuccessSendChatMessage<T> implements ChatState<T> {
  const SuccessSendChatMessage(this.response, this.fullMessage);
  

 final  ChatMessageResponseModel response;
 final  String fullMessage;

/// Create a copy of ChatState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessSendChatMessageCopyWith<T, SuccessSendChatMessage<T>> get copyWith => _$SuccessSendChatMessageCopyWithImpl<T, SuccessSendChatMessage<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessSendChatMessage<T>&&(identical(other.response, response) || other.response == response)&&(identical(other.fullMessage, fullMessage) || other.fullMessage == fullMessage));
}


@override
int get hashCode => Object.hash(runtimeType,response,fullMessage);

@override
String toString() {
  return 'ChatState<$T>.successSendChatMessage(response: $response, fullMessage: $fullMessage)';
}


}

/// @nodoc
abstract mixin class $SuccessSendChatMessageCopyWith<T,$Res> implements $ChatStateCopyWith<T, $Res> {
  factory $SuccessSendChatMessageCopyWith(SuccessSendChatMessage<T> value, $Res Function(SuccessSendChatMessage<T>) _then) = _$SuccessSendChatMessageCopyWithImpl;
@useResult
$Res call({
 ChatMessageResponseModel response, String fullMessage
});




}
/// @nodoc
class _$SuccessSendChatMessageCopyWithImpl<T,$Res>
    implements $SuccessSendChatMessageCopyWith<T, $Res> {
  _$SuccessSendChatMessageCopyWithImpl(this._self, this._then);

  final SuccessSendChatMessage<T> _self;
  final $Res Function(SuccessSendChatMessage<T>) _then;

/// Create a copy of ChatState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? response = null,Object? fullMessage = null,}) {
  return _then(SuccessSendChatMessage<T>(
null == response ? _self.response : response // ignore: cast_nullable_to_non_nullable
as ChatMessageResponseModel,null == fullMessage ? _self.fullMessage : fullMessage // ignore: cast_nullable_to_non_nullable
as String,
  ));
}


}

/// @nodoc


class FailureSendChatMessage<T> implements ChatState<T> {
  const FailureSendChatMessage({this.message});
  

 final  String? message;

/// Create a copy of ChatState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureSendChatMessageCopyWith<T, FailureSendChatMessage<T>> get copyWith => _$FailureSendChatMessageCopyWithImpl<T, FailureSendChatMessage<T>>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureSendChatMessage<T>&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'ChatState<$T>.failureSendChatMessage(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureSendChatMessageCopyWith<T,$Res> implements $ChatStateCopyWith<T, $Res> {
  factory $FailureSendChatMessageCopyWith(FailureSendChatMessage<T> value, $Res Function(FailureSendChatMessage<T>) _then) = _$FailureSendChatMessageCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureSendChatMessageCopyWithImpl<T,$Res>
    implements $FailureSendChatMessageCopyWith<T, $Res> {
  _$FailureSendChatMessageCopyWithImpl(this._self, this._then);

  final FailureSendChatMessage<T> _self;
  final $Res Function(FailureSendChatMessage<T>) _then;

/// Create a copy of ChatState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureSendChatMessage<T>(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

// dart format on
