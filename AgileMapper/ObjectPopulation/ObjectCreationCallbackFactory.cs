namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Extensions;
    using Members;

    internal class ObjectCreationCallbackFactory : MappingCallbackFactory
    {
        private readonly Type _creationTargetType;

        public ObjectCreationCallbackFactory(
            MappingConfigInfo configInfo,
            Type creationTargetType,
            CallbackPosition callbackPosition,
            ConfiguredLambdaInfo callbackLambda)
            : base(configInfo, callbackPosition, callbackLambda)
        {
            _creationTargetType = creationTargetType;
        }

        public override bool AppliesTo(CallbackPosition callbackPosition, IMappingData data)
            => _creationTargetType.IsAssignableFrom(data.TargetMember.Type) && base.AppliesTo(callbackPosition, data);

        protected override Expression GetConditionOrNull(IObjectMappingContext omc)
        {
            var condition = base.GetConditionOrNull(omc);

            if (CallbackPosition != CallbackPosition.After)
            {
                return condition;
            }

            var newObjectHasBeenCreated = omc.CreatedObject.GetIsNotDefaultComparison();

            if (condition == null)
            {
                return newObjectHasBeenCreated;
            }

            return Expression.AndAlso(newObjectHasBeenCreated, condition);
        }
    }
}