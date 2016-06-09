namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Api.Configuration;
    using Extensions;
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

        public virtual bool AppliesTo(CallbackPosition callbackPosition, IMappingData data)
            => (CallbackPosition == callbackPosition) && base.AppliesTo(data);

        public Expression Create(IObjectMappingContext omc)
        {
            var callback = GetCallback(omc);
            var condition = GetConditionOrNull(omc);

            if (condition != null)
            {
                return Expression.IfThen(condition, callback);
            }

            return callback;
        }

        private Expression GetCallback(IMemberMappingContext context)
        {
            var callback = _callbackLambda.GetBody(context);

            if ((CallbackPosition != CallbackPosition.Before) || (TargetMemberPath != null))
            {
                return callback;
            }

            var callbackWithExistingObject = callback.Replace(context.InstanceVariable, context.ExistingObject);

            return callbackWithExistingObject;
        }

        protected virtual Expression GetConditionOrNull(IObjectMappingContext omc)
            => base.GetConditionOrNull(omc);
    }
}