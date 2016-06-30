namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

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
                targetMemberLambda?.Body.ToTargetMember(
                    _configInfo.GlobalContext.MemberFinder,
                    _configInfo.MapperContext.NamingSettings)
                ?? QualifiedMember.None;

            return new CallbackSpecifier<TSource, TTarget>(_configInfo, _callbackPosition, targetMember);
        }

        internal InstanceCreationCallbackSpecifier<TSource, TTarget, TObject> CreateCallbackSpecifier<TObject>()
            => new InstanceCreationCallbackSpecifier<TSource, TTarget, TObject>(_callbackPosition, _configInfo);
    }
}