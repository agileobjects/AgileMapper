namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
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
            : base(configInfo, callbackPosition, callbackLambda, QualifiedMember.All)
        {
            _creationTargetType = creationTargetType;
        }

        public override bool AppliesTo(CallbackPosition callbackPosition, IBasicMapperData data)
            => _creationTargetType.IsAssignableFrom(data.TargetMember.Type) && base.AppliesTo(callbackPosition, data);

        public override Expression GetConditionOrNull(MemberMapperData mapperData)
        {
            var condition = base.GetConditionOrNull(mapperData);

            if (CallbackPosition != CallbackPosition.After)
            {
                return condition;
            }

            var newObjectHasBeenCreated = ((ObjectMapperData)mapperData).CreatedObject.GetIsNotDefaultComparison();

            if (condition == null)
            {
                return newObjectHasBeenCreated;
            }

            return Expression.AndAlso(newObjectHasBeenCreated, condition);
        }
    }
}