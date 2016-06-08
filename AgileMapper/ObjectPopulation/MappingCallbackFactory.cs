namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Api.Configuration;
    using Extensions;
    using Members;

    internal class MappingCallbackFactory : UserConfiguredItemBase
    {
        private readonly CallbackPosition _callbackPosition;
        private readonly ConfiguredLambdaInfo _callbackLambda;

        public MappingCallbackFactory(
            MappingConfigInfo configInfo,
            CallbackPosition callbackPosition,
            ConfiguredLambdaInfo callbackLambda)
            : base(configInfo)
        {
            _callbackPosition = callbackPosition;
            _callbackLambda = callbackLambda;
        }

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

            if (_callbackPosition != CallbackPosition.Before)
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