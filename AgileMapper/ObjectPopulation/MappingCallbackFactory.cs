namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Configuration;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MappingCallbackFactory : UserConfiguredItemBase
    {
        private readonly ConfiguredLambdaInfo _callbackLambda;

        public MappingCallbackFactory(
            MappingConfigInfo configInfo,
            CallbackPosition callbackPosition,
            ConfiguredLambdaInfo callbackLambda,
            QualifiedMember targetMember)
            : base(configInfo, targetMember)
        {
            CallbackPosition = callbackPosition;
            _callbackLambda = callbackLambda;
        }

        protected CallbackPosition CallbackPosition { get; }

        public virtual bool AppliesTo(CallbackPosition callbackPosition, IBasicMapperData mapperData)
            => (CallbackPosition == callbackPosition) && base.AppliesTo(mapperData);

        protected override bool MemberPathMatches(IBasicMapperData mapperData)
            => mapperData.HasCompatibleTypes(ConfigInfo);

        public Expression Create(IMemberMapperData mapperData)
        {
            mapperData.Context.UsesMappingDataObjectAsParameter =
                _callbackLambda.UsesMappingDataObjectParameter ||
                ConfigInfo.ConditionUsesMappingDataObjectParameter;

            var callback = _callbackLambda.GetBody(mapperData, CallbackPosition, TargetMember);
            var condition = GetConditionOrNull(mapperData, CallbackPosition);

            if (condition != null)
            {
                return Expression.IfThen(condition, callback);
            }

            return callback;
        }
    }
}