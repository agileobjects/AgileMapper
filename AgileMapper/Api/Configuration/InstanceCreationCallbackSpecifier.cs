namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using Members;
    using ObjectPopulation;

    internal class InstanceCreationCallbackSpecifier<TSource, TTarget, TObject> :
        IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget>,
        IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>
    {
        private readonly MappingConfigInfo _configInfo;

        public InstanceCreationCallbackSpecifier(MapperContext mapperContext, InvocationPosition invocationPosition)
            : this(MappingConfigInfo.AllRuleSetsAndSourceTypes(mapperContext).ForTargetType<TTarget>(), invocationPosition)
        {
        }

        public InstanceCreationCallbackSpecifier(MappingConfigInfo configInfo, InvocationPosition invocationPosition)
        {
            _configInfo = configInfo.WithInvocationPosition(invocationPosition);
        }

        #region IConditionalPreInstanceCreationCallbackSpecifier

        IPreInstanceCreationCallbackSpecifier<TSource, TTarget>
            IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget>.If(
                Expression<Func<IMappingData<TSource, TTarget>, bool>> condition)
            => SetCondition(condition);

        IPreInstanceCreationCallbackSpecifier<TSource, TTarget>
            IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget>.If(
                Expression<Func<TSource, TTarget, bool>> condition)
            => SetCondition(condition);

        IPreInstanceCreationCallbackSpecifier<TSource, TTarget>
            IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget>.If(
                Expression<Func<TSource, TTarget, int?, bool>> condition)
            => SetCondition(condition);

        private InstanceCreationCallbackSpecifier<TSource, TTarget, TObject> SetCondition(
            LambdaExpression conditionLambda)
        {
            _configInfo.AddConditionOrThrow(conditionLambda);
            return this;
        }

        IMappingConfigContinuation<TSource, TTarget> IPreInstanceCreationCallbackSpecifier<TSource, TTarget>.Call(
            Action<IMappingData<TSource, TTarget>> callback)
        {
            return CreateCallbackFactory(callback);
        }

        IMappingConfigContinuation<TSource, TTarget> IPreInstanceCreationCallbackSpecifier<TSource, TTarget>.Call(
            Action<TSource, TTarget> callback)
        {
            return CreateCallbackFactory(callback);
        }

        IMappingConfigContinuation<TSource, TTarget> IPreInstanceCreationCallbackSpecifier<TSource, TTarget>.Call(
            Action<TSource, TTarget, int?> callback)
        {
            return CreateCallbackFactory(callback);
        }

        #endregion

        #region IConditionalPostInstanceCreationCallbackSpecifier

        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>
            IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>.If(
                Expression<Func<IObjectCreationMappingData<TSource, TTarget, TObject>, bool>> condition)
            => SetCondition(condition);

        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>
            IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>.If(
                Expression<Func<TSource, TTarget, bool>> condition)
            => SetCondition(condition);

        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>
            IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>.If(
                Expression<Func<TSource, TTarget, TObject, bool>> condition)
            => SetCondition(condition);

        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>
            IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>.If(
                Expression<Func<TSource, TTarget, TObject, int?, bool>> condition)
            => SetCondition(condition);

        IMappingConfigContinuation<TSource, TTarget> IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>.Call(
            Action<IObjectCreationMappingData<TSource, TTarget, TObject>> callback)
            => CreateCallbackFactory(callback);

        IMappingConfigContinuation<TSource, TTarget> IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>.Call(
            Action<TSource, TTarget> callback)
            => CreateCallbackFactory(callback);

        IMappingConfigContinuation<TSource, TTarget> IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>.Call(
            Action<TSource, TTarget, TObject> callback)
            => CreateCallbackFactory(callback);

        IMappingConfigContinuation<TSource, TTarget> IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>.Call(
            Action<TSource, TTarget, TObject, int?> callback)
            => CreateCallbackFactory(callback);

        #endregion

        private MappingConfigContinuation<TSource, TTarget> CreateCallbackFactory<TAction>(TAction callback)
        {
            var callbackLambda = ConfiguredLambdaInfo.ForAction(callback, typeof(TSource), typeof(TTarget), typeof(TObject));
            callbackLambda.InvocationPosition = _configInfo.InvocationPosition;

            var creationCallbackFactory = new ObjectCreationCallbackFactory(
                _configInfo,
                typeof(TObject),
                callbackLambda);

            _configInfo.UserConfigurations.Add(creationCallbackFactory);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }
    }
}