namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Configuration;
    using Members;

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

        public Expression Create(MemberMapperData mapperData)
        {
            var callback = _callbackLambda.GetBody(mapperData);
            var condition = GetConditionOrNull(mapperData);

            if (condition != null)
            {
                return Expression.IfThen(condition, callback);
            }

            return callback;
        }
    }
}