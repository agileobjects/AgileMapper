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

            return Process(callback, context);
        }

        protected virtual Expression GetConditionOrNull(IObjectMappingContext omc)
            => Process(base.GetConditionOrNull(omc), omc);

        private Expression Process(Expression expression, IMemberMappingContext context)
        {
            if ((expression == null) ||
                (CallbackPosition != CallbackPosition.Before) ||
                (TargetMemberPath != null))
            {
                return expression;
            }

            var expressionWithExistingObject = expression.Replace(context.InstanceVariable, context.ExistingObject);

            return expressionWithExistingObject;
        }
    }
}