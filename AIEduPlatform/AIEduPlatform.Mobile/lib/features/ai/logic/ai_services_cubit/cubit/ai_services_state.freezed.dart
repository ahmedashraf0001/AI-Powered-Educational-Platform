// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'ai_services_state.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// dart format off
T _$identity<T>(T value) => value;
/// @nodoc
mixin _$AiServicesState {





@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is AiServicesState);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'AiServicesState()';
}


}

/// @nodoc
class $AiServicesStateCopyWith<$Res>  {
$AiServicesStateCopyWith(AiServicesState _, $Res Function(AiServicesState) __);
}


/// Adds pattern-matching-related methods to [AiServicesState].
extension AiServicesStatePatterns on AiServicesState {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>({TResult Function( _Initial value)?  initial,TResult Function( LoadingFlashCards value)?  loadingFlashCards,TResult Function( SuccessFlashCards value)?  successFlashCards,TResult Function( FailureFlashCards value)?  failureFlashCards,TResult Function( LoadingSummaryTopic value)?  loadingSummaryTopic,TResult Function( SuccessSummaryTopic value)?  successSummaryTopic,TResult Function( FailureSummaryTopic value)?  failureSummaryTopic,TResult Function( LoadingGenerateQuiz value)?  loadingGenerateQuiz,TResult Function( SuccessGenerateQuiz value)?  successGenerateQuiz,TResult Function( FailureGenerateQuiz value)?  failureGenerateQuiz,TResult Function( LoadingSubmitQuiz value)?  loadingSubmitQuiz,TResult Function( SuccessSubmitQuiz value)?  successSubmitQuiz,TResult Function( FailureSubmitQuiz value)?  failureSubmitQuiz,TResult Function( LoadingMindMap value)?  loadingMindMap,TResult Function( SuccessMindMap value)?  successMindMap,TResult Function( FailureMindMap value)?  failureMindMap,required TResult orElse(),}){
final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial(_that);case LoadingFlashCards() when loadingFlashCards != null:
return loadingFlashCards(_that);case SuccessFlashCards() when successFlashCards != null:
return successFlashCards(_that);case FailureFlashCards() when failureFlashCards != null:
return failureFlashCards(_that);case LoadingSummaryTopic() when loadingSummaryTopic != null:
return loadingSummaryTopic(_that);case SuccessSummaryTopic() when successSummaryTopic != null:
return successSummaryTopic(_that);case FailureSummaryTopic() when failureSummaryTopic != null:
return failureSummaryTopic(_that);case LoadingGenerateQuiz() when loadingGenerateQuiz != null:
return loadingGenerateQuiz(_that);case SuccessGenerateQuiz() when successGenerateQuiz != null:
return successGenerateQuiz(_that);case FailureGenerateQuiz() when failureGenerateQuiz != null:
return failureGenerateQuiz(_that);case LoadingSubmitQuiz() when loadingSubmitQuiz != null:
return loadingSubmitQuiz(_that);case SuccessSubmitQuiz() when successSubmitQuiz != null:
return successSubmitQuiz(_that);case FailureSubmitQuiz() when failureSubmitQuiz != null:
return failureSubmitQuiz(_that);case LoadingMindMap() when loadingMindMap != null:
return loadingMindMap(_that);case SuccessMindMap() when successMindMap != null:
return successMindMap(_that);case FailureMindMap() when failureMindMap != null:
return failureMindMap(_that);case _:
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

@optionalTypeArgs TResult map<TResult extends Object?>({required TResult Function( _Initial value)  initial,required TResult Function( LoadingFlashCards value)  loadingFlashCards,required TResult Function( SuccessFlashCards value)  successFlashCards,required TResult Function( FailureFlashCards value)  failureFlashCards,required TResult Function( LoadingSummaryTopic value)  loadingSummaryTopic,required TResult Function( SuccessSummaryTopic value)  successSummaryTopic,required TResult Function( FailureSummaryTopic value)  failureSummaryTopic,required TResult Function( LoadingGenerateQuiz value)  loadingGenerateQuiz,required TResult Function( SuccessGenerateQuiz value)  successGenerateQuiz,required TResult Function( FailureGenerateQuiz value)  failureGenerateQuiz,required TResult Function( LoadingSubmitQuiz value)  loadingSubmitQuiz,required TResult Function( SuccessSubmitQuiz value)  successSubmitQuiz,required TResult Function( FailureSubmitQuiz value)  failureSubmitQuiz,required TResult Function( LoadingMindMap value)  loadingMindMap,required TResult Function( SuccessMindMap value)  successMindMap,required TResult Function( FailureMindMap value)  failureMindMap,}){
final _that = this;
switch (_that) {
case _Initial():
return initial(_that);case LoadingFlashCards():
return loadingFlashCards(_that);case SuccessFlashCards():
return successFlashCards(_that);case FailureFlashCards():
return failureFlashCards(_that);case LoadingSummaryTopic():
return loadingSummaryTopic(_that);case SuccessSummaryTopic():
return successSummaryTopic(_that);case FailureSummaryTopic():
return failureSummaryTopic(_that);case LoadingGenerateQuiz():
return loadingGenerateQuiz(_that);case SuccessGenerateQuiz():
return successGenerateQuiz(_that);case FailureGenerateQuiz():
return failureGenerateQuiz(_that);case LoadingSubmitQuiz():
return loadingSubmitQuiz(_that);case SuccessSubmitQuiz():
return successSubmitQuiz(_that);case FailureSubmitQuiz():
return failureSubmitQuiz(_that);case LoadingMindMap():
return loadingMindMap(_that);case SuccessMindMap():
return successMindMap(_that);case FailureMindMap():
return failureMindMap(_that);case _:
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>({TResult? Function( _Initial value)?  initial,TResult? Function( LoadingFlashCards value)?  loadingFlashCards,TResult? Function( SuccessFlashCards value)?  successFlashCards,TResult? Function( FailureFlashCards value)?  failureFlashCards,TResult? Function( LoadingSummaryTopic value)?  loadingSummaryTopic,TResult? Function( SuccessSummaryTopic value)?  successSummaryTopic,TResult? Function( FailureSummaryTopic value)?  failureSummaryTopic,TResult? Function( LoadingGenerateQuiz value)?  loadingGenerateQuiz,TResult? Function( SuccessGenerateQuiz value)?  successGenerateQuiz,TResult? Function( FailureGenerateQuiz value)?  failureGenerateQuiz,TResult? Function( LoadingSubmitQuiz value)?  loadingSubmitQuiz,TResult? Function( SuccessSubmitQuiz value)?  successSubmitQuiz,TResult? Function( FailureSubmitQuiz value)?  failureSubmitQuiz,TResult? Function( LoadingMindMap value)?  loadingMindMap,TResult? Function( SuccessMindMap value)?  successMindMap,TResult? Function( FailureMindMap value)?  failureMindMap,}){
final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial(_that);case LoadingFlashCards() when loadingFlashCards != null:
return loadingFlashCards(_that);case SuccessFlashCards() when successFlashCards != null:
return successFlashCards(_that);case FailureFlashCards() when failureFlashCards != null:
return failureFlashCards(_that);case LoadingSummaryTopic() when loadingSummaryTopic != null:
return loadingSummaryTopic(_that);case SuccessSummaryTopic() when successSummaryTopic != null:
return successSummaryTopic(_that);case FailureSummaryTopic() when failureSummaryTopic != null:
return failureSummaryTopic(_that);case LoadingGenerateQuiz() when loadingGenerateQuiz != null:
return loadingGenerateQuiz(_that);case SuccessGenerateQuiz() when successGenerateQuiz != null:
return successGenerateQuiz(_that);case FailureGenerateQuiz() when failureGenerateQuiz != null:
return failureGenerateQuiz(_that);case LoadingSubmitQuiz() when loadingSubmitQuiz != null:
return loadingSubmitQuiz(_that);case SuccessSubmitQuiz() when successSubmitQuiz != null:
return successSubmitQuiz(_that);case FailureSubmitQuiz() when failureSubmitQuiz != null:
return failureSubmitQuiz(_that);case LoadingMindMap() when loadingMindMap != null:
return loadingMindMap(_that);case SuccessMindMap() when successMindMap != null:
return successMindMap(_that);case FailureMindMap() when failureMindMap != null:
return failureMindMap(_that);case _:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>({TResult Function()?  initial,TResult Function()?  loadingFlashCards,TResult Function( List<FlashCardModel> dataList)?  successFlashCards,TResult Function( String? message)?  failureFlashCards,TResult Function()?  loadingSummaryTopic,TResult Function( SummaryDataModel dataModel)?  successSummaryTopic,TResult Function( String? message)?  failureSummaryTopic,TResult Function()?  loadingGenerateQuiz,TResult Function( QuizDataModel quizData)?  successGenerateQuiz,TResult Function( String? message)?  failureGenerateQuiz,TResult Function()?  loadingSubmitQuiz,TResult Function( SubmitQuizResponseData submitQuizData)?  successSubmitQuiz,TResult Function( String? message)?  failureSubmitQuiz,TResult Function()?  loadingMindMap,TResult Function( MindMapDataModel mindMapData)?  successMindMap,TResult Function( String? message)?  failureMindMap,required TResult orElse(),}) {final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial();case LoadingFlashCards() when loadingFlashCards != null:
return loadingFlashCards();case SuccessFlashCards() when successFlashCards != null:
return successFlashCards(_that.dataList);case FailureFlashCards() when failureFlashCards != null:
return failureFlashCards(_that.message);case LoadingSummaryTopic() when loadingSummaryTopic != null:
return loadingSummaryTopic();case SuccessSummaryTopic() when successSummaryTopic != null:
return successSummaryTopic(_that.dataModel);case FailureSummaryTopic() when failureSummaryTopic != null:
return failureSummaryTopic(_that.message);case LoadingGenerateQuiz() when loadingGenerateQuiz != null:
return loadingGenerateQuiz();case SuccessGenerateQuiz() when successGenerateQuiz != null:
return successGenerateQuiz(_that.quizData);case FailureGenerateQuiz() when failureGenerateQuiz != null:
return failureGenerateQuiz(_that.message);case LoadingSubmitQuiz() when loadingSubmitQuiz != null:
return loadingSubmitQuiz();case SuccessSubmitQuiz() when successSubmitQuiz != null:
return successSubmitQuiz(_that.submitQuizData);case FailureSubmitQuiz() when failureSubmitQuiz != null:
return failureSubmitQuiz(_that.message);case LoadingMindMap() when loadingMindMap != null:
return loadingMindMap();case SuccessMindMap() when successMindMap != null:
return successMindMap(_that.mindMapData);case FailureMindMap() when failureMindMap != null:
return failureMindMap(_that.message);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>({required TResult Function()  initial,required TResult Function()  loadingFlashCards,required TResult Function( List<FlashCardModel> dataList)  successFlashCards,required TResult Function( String? message)  failureFlashCards,required TResult Function()  loadingSummaryTopic,required TResult Function( SummaryDataModel dataModel)  successSummaryTopic,required TResult Function( String? message)  failureSummaryTopic,required TResult Function()  loadingGenerateQuiz,required TResult Function( QuizDataModel quizData)  successGenerateQuiz,required TResult Function( String? message)  failureGenerateQuiz,required TResult Function()  loadingSubmitQuiz,required TResult Function( SubmitQuizResponseData submitQuizData)  successSubmitQuiz,required TResult Function( String? message)  failureSubmitQuiz,required TResult Function()  loadingMindMap,required TResult Function( MindMapDataModel mindMapData)  successMindMap,required TResult Function( String? message)  failureMindMap,}) {final _that = this;
switch (_that) {
case _Initial():
return initial();case LoadingFlashCards():
return loadingFlashCards();case SuccessFlashCards():
return successFlashCards(_that.dataList);case FailureFlashCards():
return failureFlashCards(_that.message);case LoadingSummaryTopic():
return loadingSummaryTopic();case SuccessSummaryTopic():
return successSummaryTopic(_that.dataModel);case FailureSummaryTopic():
return failureSummaryTopic(_that.message);case LoadingGenerateQuiz():
return loadingGenerateQuiz();case SuccessGenerateQuiz():
return successGenerateQuiz(_that.quizData);case FailureGenerateQuiz():
return failureGenerateQuiz(_that.message);case LoadingSubmitQuiz():
return loadingSubmitQuiz();case SuccessSubmitQuiz():
return successSubmitQuiz(_that.submitQuizData);case FailureSubmitQuiz():
return failureSubmitQuiz(_that.message);case LoadingMindMap():
return loadingMindMap();case SuccessMindMap():
return successMindMap(_that.mindMapData);case FailureMindMap():
return failureMindMap(_that.message);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>({TResult? Function()?  initial,TResult? Function()?  loadingFlashCards,TResult? Function( List<FlashCardModel> dataList)?  successFlashCards,TResult? Function( String? message)?  failureFlashCards,TResult? Function()?  loadingSummaryTopic,TResult? Function( SummaryDataModel dataModel)?  successSummaryTopic,TResult? Function( String? message)?  failureSummaryTopic,TResult? Function()?  loadingGenerateQuiz,TResult? Function( QuizDataModel quizData)?  successGenerateQuiz,TResult? Function( String? message)?  failureGenerateQuiz,TResult? Function()?  loadingSubmitQuiz,TResult? Function( SubmitQuizResponseData submitQuizData)?  successSubmitQuiz,TResult? Function( String? message)?  failureSubmitQuiz,TResult? Function()?  loadingMindMap,TResult? Function( MindMapDataModel mindMapData)?  successMindMap,TResult? Function( String? message)?  failureMindMap,}) {final _that = this;
switch (_that) {
case _Initial() when initial != null:
return initial();case LoadingFlashCards() when loadingFlashCards != null:
return loadingFlashCards();case SuccessFlashCards() when successFlashCards != null:
return successFlashCards(_that.dataList);case FailureFlashCards() when failureFlashCards != null:
return failureFlashCards(_that.message);case LoadingSummaryTopic() when loadingSummaryTopic != null:
return loadingSummaryTopic();case SuccessSummaryTopic() when successSummaryTopic != null:
return successSummaryTopic(_that.dataModel);case FailureSummaryTopic() when failureSummaryTopic != null:
return failureSummaryTopic(_that.message);case LoadingGenerateQuiz() when loadingGenerateQuiz != null:
return loadingGenerateQuiz();case SuccessGenerateQuiz() when successGenerateQuiz != null:
return successGenerateQuiz(_that.quizData);case FailureGenerateQuiz() when failureGenerateQuiz != null:
return failureGenerateQuiz(_that.message);case LoadingSubmitQuiz() when loadingSubmitQuiz != null:
return loadingSubmitQuiz();case SuccessSubmitQuiz() when successSubmitQuiz != null:
return successSubmitQuiz(_that.submitQuizData);case FailureSubmitQuiz() when failureSubmitQuiz != null:
return failureSubmitQuiz(_that.message);case LoadingMindMap() when loadingMindMap != null:
return loadingMindMap();case SuccessMindMap() when successMindMap != null:
return successMindMap(_that.mindMapData);case FailureMindMap() when failureMindMap != null:
return failureMindMap(_that.message);case _:
  return null;

}
}

}

/// @nodoc


class _Initial implements AiServicesState {
  const _Initial();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is _Initial);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'AiServicesState.initial()';
}


}




/// @nodoc


class LoadingFlashCards implements AiServicesState {
  const LoadingFlashCards();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingFlashCards);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'AiServicesState.loadingFlashCards()';
}


}




/// @nodoc


class SuccessFlashCards implements AiServicesState {
  const SuccessFlashCards(final  List<FlashCardModel> dataList): _dataList = dataList;
  

 final  List<FlashCardModel> _dataList;
 List<FlashCardModel> get dataList {
  if (_dataList is EqualUnmodifiableListView) return _dataList;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_dataList);
}


/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessFlashCardsCopyWith<SuccessFlashCards> get copyWith => _$SuccessFlashCardsCopyWithImpl<SuccessFlashCards>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessFlashCards&&const DeepCollectionEquality().equals(other._dataList, _dataList));
}


@override
int get hashCode => Object.hash(runtimeType,const DeepCollectionEquality().hash(_dataList));

@override
String toString() {
  return 'AiServicesState.successFlashCards(dataList: $dataList)';
}


}

/// @nodoc
abstract mixin class $SuccessFlashCardsCopyWith<$Res> implements $AiServicesStateCopyWith<$Res> {
  factory $SuccessFlashCardsCopyWith(SuccessFlashCards value, $Res Function(SuccessFlashCards) _then) = _$SuccessFlashCardsCopyWithImpl;
@useResult
$Res call({
 List<FlashCardModel> dataList
});




}
/// @nodoc
class _$SuccessFlashCardsCopyWithImpl<$Res>
    implements $SuccessFlashCardsCopyWith<$Res> {
  _$SuccessFlashCardsCopyWithImpl(this._self, this._then);

  final SuccessFlashCards _self;
  final $Res Function(SuccessFlashCards) _then;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? dataList = null,}) {
  return _then(SuccessFlashCards(
null == dataList ? _self._dataList : dataList // ignore: cast_nullable_to_non_nullable
as List<FlashCardModel>,
  ));
}


}

/// @nodoc


class FailureFlashCards implements AiServicesState {
  const FailureFlashCards({this.message});
  

 final  String? message;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureFlashCardsCopyWith<FailureFlashCards> get copyWith => _$FailureFlashCardsCopyWithImpl<FailureFlashCards>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureFlashCards&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'AiServicesState.failureFlashCards(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureFlashCardsCopyWith<$Res> implements $AiServicesStateCopyWith<$Res> {
  factory $FailureFlashCardsCopyWith(FailureFlashCards value, $Res Function(FailureFlashCards) _then) = _$FailureFlashCardsCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureFlashCardsCopyWithImpl<$Res>
    implements $FailureFlashCardsCopyWith<$Res> {
  _$FailureFlashCardsCopyWithImpl(this._self, this._then);

  final FailureFlashCards _self;
  final $Res Function(FailureFlashCards) _then;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureFlashCards(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingSummaryTopic implements AiServicesState {
  const LoadingSummaryTopic();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingSummaryTopic);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'AiServicesState.loadingSummaryTopic()';
}


}




/// @nodoc


class SuccessSummaryTopic implements AiServicesState {
  const SuccessSummaryTopic(this.dataModel);
  

 final  SummaryDataModel dataModel;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessSummaryTopicCopyWith<SuccessSummaryTopic> get copyWith => _$SuccessSummaryTopicCopyWithImpl<SuccessSummaryTopic>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessSummaryTopic&&(identical(other.dataModel, dataModel) || other.dataModel == dataModel));
}


@override
int get hashCode => Object.hash(runtimeType,dataModel);

@override
String toString() {
  return 'AiServicesState.successSummaryTopic(dataModel: $dataModel)';
}


}

/// @nodoc
abstract mixin class $SuccessSummaryTopicCopyWith<$Res> implements $AiServicesStateCopyWith<$Res> {
  factory $SuccessSummaryTopicCopyWith(SuccessSummaryTopic value, $Res Function(SuccessSummaryTopic) _then) = _$SuccessSummaryTopicCopyWithImpl;
@useResult
$Res call({
 SummaryDataModel dataModel
});




}
/// @nodoc
class _$SuccessSummaryTopicCopyWithImpl<$Res>
    implements $SuccessSummaryTopicCopyWith<$Res> {
  _$SuccessSummaryTopicCopyWithImpl(this._self, this._then);

  final SuccessSummaryTopic _self;
  final $Res Function(SuccessSummaryTopic) _then;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? dataModel = null,}) {
  return _then(SuccessSummaryTopic(
null == dataModel ? _self.dataModel : dataModel // ignore: cast_nullable_to_non_nullable
as SummaryDataModel,
  ));
}


}

/// @nodoc


class FailureSummaryTopic implements AiServicesState {
  const FailureSummaryTopic({this.message});
  

 final  String? message;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureSummaryTopicCopyWith<FailureSummaryTopic> get copyWith => _$FailureSummaryTopicCopyWithImpl<FailureSummaryTopic>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureSummaryTopic&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'AiServicesState.failureSummaryTopic(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureSummaryTopicCopyWith<$Res> implements $AiServicesStateCopyWith<$Res> {
  factory $FailureSummaryTopicCopyWith(FailureSummaryTopic value, $Res Function(FailureSummaryTopic) _then) = _$FailureSummaryTopicCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureSummaryTopicCopyWithImpl<$Res>
    implements $FailureSummaryTopicCopyWith<$Res> {
  _$FailureSummaryTopicCopyWithImpl(this._self, this._then);

  final FailureSummaryTopic _self;
  final $Res Function(FailureSummaryTopic) _then;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureSummaryTopic(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingGenerateQuiz implements AiServicesState {
  const LoadingGenerateQuiz();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingGenerateQuiz);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'AiServicesState.loadingGenerateQuiz()';
}


}




/// @nodoc


class SuccessGenerateQuiz implements AiServicesState {
  const SuccessGenerateQuiz(this.quizData);
  

 final  QuizDataModel quizData;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessGenerateQuizCopyWith<SuccessGenerateQuiz> get copyWith => _$SuccessGenerateQuizCopyWithImpl<SuccessGenerateQuiz>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessGenerateQuiz&&(identical(other.quizData, quizData) || other.quizData == quizData));
}


@override
int get hashCode => Object.hash(runtimeType,quizData);

@override
String toString() {
  return 'AiServicesState.successGenerateQuiz(quizData: $quizData)';
}


}

/// @nodoc
abstract mixin class $SuccessGenerateQuizCopyWith<$Res> implements $AiServicesStateCopyWith<$Res> {
  factory $SuccessGenerateQuizCopyWith(SuccessGenerateQuiz value, $Res Function(SuccessGenerateQuiz) _then) = _$SuccessGenerateQuizCopyWithImpl;
@useResult
$Res call({
 QuizDataModel quizData
});




}
/// @nodoc
class _$SuccessGenerateQuizCopyWithImpl<$Res>
    implements $SuccessGenerateQuizCopyWith<$Res> {
  _$SuccessGenerateQuizCopyWithImpl(this._self, this._then);

  final SuccessGenerateQuiz _self;
  final $Res Function(SuccessGenerateQuiz) _then;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? quizData = null,}) {
  return _then(SuccessGenerateQuiz(
null == quizData ? _self.quizData : quizData // ignore: cast_nullable_to_non_nullable
as QuizDataModel,
  ));
}


}

/// @nodoc


class FailureGenerateQuiz implements AiServicesState {
  const FailureGenerateQuiz({this.message});
  

 final  String? message;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureGenerateQuizCopyWith<FailureGenerateQuiz> get copyWith => _$FailureGenerateQuizCopyWithImpl<FailureGenerateQuiz>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureGenerateQuiz&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'AiServicesState.failureGenerateQuiz(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureGenerateQuizCopyWith<$Res> implements $AiServicesStateCopyWith<$Res> {
  factory $FailureGenerateQuizCopyWith(FailureGenerateQuiz value, $Res Function(FailureGenerateQuiz) _then) = _$FailureGenerateQuizCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureGenerateQuizCopyWithImpl<$Res>
    implements $FailureGenerateQuizCopyWith<$Res> {
  _$FailureGenerateQuizCopyWithImpl(this._self, this._then);

  final FailureGenerateQuiz _self;
  final $Res Function(FailureGenerateQuiz) _then;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureGenerateQuiz(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingSubmitQuiz implements AiServicesState {
  const LoadingSubmitQuiz();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingSubmitQuiz);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'AiServicesState.loadingSubmitQuiz()';
}


}




/// @nodoc


class SuccessSubmitQuiz implements AiServicesState {
  const SuccessSubmitQuiz(this.submitQuizData);
  

 final  SubmitQuizResponseData submitQuizData;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessSubmitQuizCopyWith<SuccessSubmitQuiz> get copyWith => _$SuccessSubmitQuizCopyWithImpl<SuccessSubmitQuiz>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessSubmitQuiz&&(identical(other.submitQuizData, submitQuizData) || other.submitQuizData == submitQuizData));
}


@override
int get hashCode => Object.hash(runtimeType,submitQuizData);

@override
String toString() {
  return 'AiServicesState.successSubmitQuiz(submitQuizData: $submitQuizData)';
}


}

/// @nodoc
abstract mixin class $SuccessSubmitQuizCopyWith<$Res> implements $AiServicesStateCopyWith<$Res> {
  factory $SuccessSubmitQuizCopyWith(SuccessSubmitQuiz value, $Res Function(SuccessSubmitQuiz) _then) = _$SuccessSubmitQuizCopyWithImpl;
@useResult
$Res call({
 SubmitQuizResponseData submitQuizData
});




}
/// @nodoc
class _$SuccessSubmitQuizCopyWithImpl<$Res>
    implements $SuccessSubmitQuizCopyWith<$Res> {
  _$SuccessSubmitQuizCopyWithImpl(this._self, this._then);

  final SuccessSubmitQuiz _self;
  final $Res Function(SuccessSubmitQuiz) _then;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? submitQuizData = null,}) {
  return _then(SuccessSubmitQuiz(
null == submitQuizData ? _self.submitQuizData : submitQuizData // ignore: cast_nullable_to_non_nullable
as SubmitQuizResponseData,
  ));
}


}

/// @nodoc


class FailureSubmitQuiz implements AiServicesState {
  const FailureSubmitQuiz({this.message});
  

 final  String? message;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureSubmitQuizCopyWith<FailureSubmitQuiz> get copyWith => _$FailureSubmitQuizCopyWithImpl<FailureSubmitQuiz>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureSubmitQuiz&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'AiServicesState.failureSubmitQuiz(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureSubmitQuizCopyWith<$Res> implements $AiServicesStateCopyWith<$Res> {
  factory $FailureSubmitQuizCopyWith(FailureSubmitQuiz value, $Res Function(FailureSubmitQuiz) _then) = _$FailureSubmitQuizCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureSubmitQuizCopyWithImpl<$Res>
    implements $FailureSubmitQuizCopyWith<$Res> {
  _$FailureSubmitQuizCopyWithImpl(this._self, this._then);

  final FailureSubmitQuiz _self;
  final $Res Function(FailureSubmitQuiz) _then;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureSubmitQuiz(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

/// @nodoc


class LoadingMindMap implements AiServicesState {
  const LoadingMindMap();
  






@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is LoadingMindMap);
}


@override
int get hashCode => runtimeType.hashCode;

@override
String toString() {
  return 'AiServicesState.loadingMindMap()';
}


}




/// @nodoc


class SuccessMindMap implements AiServicesState {
  const SuccessMindMap(this.mindMapData);
  

 final  MindMapDataModel mindMapData;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$SuccessMindMapCopyWith<SuccessMindMap> get copyWith => _$SuccessMindMapCopyWithImpl<SuccessMindMap>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is SuccessMindMap&&(identical(other.mindMapData, mindMapData) || other.mindMapData == mindMapData));
}


@override
int get hashCode => Object.hash(runtimeType,mindMapData);

@override
String toString() {
  return 'AiServicesState.successMindMap(mindMapData: $mindMapData)';
}


}

/// @nodoc
abstract mixin class $SuccessMindMapCopyWith<$Res> implements $AiServicesStateCopyWith<$Res> {
  factory $SuccessMindMapCopyWith(SuccessMindMap value, $Res Function(SuccessMindMap) _then) = _$SuccessMindMapCopyWithImpl;
@useResult
$Res call({
 MindMapDataModel mindMapData
});




}
/// @nodoc
class _$SuccessMindMapCopyWithImpl<$Res>
    implements $SuccessMindMapCopyWith<$Res> {
  _$SuccessMindMapCopyWithImpl(this._self, this._then);

  final SuccessMindMap _self;
  final $Res Function(SuccessMindMap) _then;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? mindMapData = null,}) {
  return _then(SuccessMindMap(
null == mindMapData ? _self.mindMapData : mindMapData // ignore: cast_nullable_to_non_nullable
as MindMapDataModel,
  ));
}


}

/// @nodoc


class FailureMindMap implements AiServicesState {
  const FailureMindMap({this.message});
  

 final  String? message;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$FailureMindMapCopyWith<FailureMindMap> get copyWith => _$FailureMindMapCopyWithImpl<FailureMindMap>(this, _$identity);



@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is FailureMindMap&&(identical(other.message, message) || other.message == message));
}


@override
int get hashCode => Object.hash(runtimeType,message);

@override
String toString() {
  return 'AiServicesState.failureMindMap(message: $message)';
}


}

/// @nodoc
abstract mixin class $FailureMindMapCopyWith<$Res> implements $AiServicesStateCopyWith<$Res> {
  factory $FailureMindMapCopyWith(FailureMindMap value, $Res Function(FailureMindMap) _then) = _$FailureMindMapCopyWithImpl;
@useResult
$Res call({
 String? message
});




}
/// @nodoc
class _$FailureMindMapCopyWithImpl<$Res>
    implements $FailureMindMapCopyWith<$Res> {
  _$FailureMindMapCopyWithImpl(this._self, this._then);

  final FailureMindMap _self;
  final $Res Function(FailureMindMap) _then;

/// Create a copy of AiServicesState
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') $Res call({Object? message = freezed,}) {
  return _then(FailureMindMap(
message: freezed == message ? _self.message : message // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}

// dart format on
