namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using Members;
    using Members.Extensions;

    /// <summary>
    /// Provides options for configuring an element of how this mapper performs a mapping.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public abstract class EventConfigStartingPointBase<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal EventConfigStartingPointBase(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        internal CallbackSpecifier<TSource, TTarget> CreateCallbackSpecifier(LambdaExpression targetMemberLambda = null)
        {
            var targetMember = (targetMemberLambda != null)
                ? targetMemberLambda.ToTargetMember(_configInfo.MapperContext)
                : QualifiedMember.None;

            return new CallbackSpecifier<TSource, TTarget>(_configInfo, targetMember);
        }

        internal InstanceCreationCallbackSpecifier<TSource, TTarget, TObject> CreateCallbackSpecifier<TObject>()
            => new InstanceCreationCallbackSpecifier<TSource, TTarget, TObject>(_configInfo, _configInfo.InvocationPosition);
    }
}