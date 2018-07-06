namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using Members;
    using ObjectPopulation;

    /// <summary>
    /// Provides options for configuring an element of how this mapper performs a mapping.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public abstract class MappingConfigStartingPointBase<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly CallbackPosition _callbackPosition;

        internal MappingConfigStartingPointBase(MappingConfigInfo configInfo, CallbackPosition callbackPosition)
        {
            _configInfo = configInfo;
            _callbackPosition = callbackPosition;
        }

        internal CallbackSpecifier<TSource, TTarget> CreateCallbackSpecifier(LambdaExpression targetMemberLambda = null)
        {
            var targetMember =
                targetMemberLambda?.ToTargetMember(_configInfo.MapperContext)
                ?? QualifiedMember.None;

            return new CallbackSpecifier<TSource, TTarget>(_configInfo, _callbackPosition, targetMember);
        }

        internal InstanceCreationCallbackSpecifier<TSource, TTarget, TObject> CreateCallbackSpecifier<TObject>()
            => new InstanceCreationCallbackSpecifier<TSource, TTarget, TObject>(_callbackPosition, _configInfo);
    }
}