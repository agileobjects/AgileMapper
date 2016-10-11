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

        public virtual bool AppliesTo(CallbackPosition callbackPosition, IBasicMapperData data)
            => (CallbackPosition == callbackPosition) && base.AppliesTo(data);

        public Expression Create(MemberMapperData data)
        {
            var callback = _callbackLambda.GetBody(data);
            var condition = GetConditionOrNull(data);

            if (condition != null)
            {
                return Expression.IfThen(condition, callback);
            }

            return callback;
        }
    }
}