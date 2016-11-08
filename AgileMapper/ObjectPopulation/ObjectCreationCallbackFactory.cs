namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
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

        public override bool AppliesTo(CallbackPosition callbackPosition, IBasicMapperData mapperData)
            => _creationTargetType.IsAssignableFrom(mapperData.TargetMember.Type) && base.AppliesTo(callbackPosition, mapperData);

        protected override bool TargetMembersMatch(IBasicMapperData mapperData) => true;

        public override Expression GetConditionOrNull(IMemberMapperData mapperData)
        {
            var condition = GetConditionOrNull(mapperData, CallbackPosition);

            if (CallbackPosition != CallbackPosition.After)
            {
                return condition;
            }

            var newObjectHasBeenCreated = mapperData.CreatedObject.GetIsNotDefaultComparison();

            if (condition == null)
            {
                return newObjectHasBeenCreated;
            }

            return Expression.AndAlso(newObjectHasBeenCreated, condition);
        }
    }
}