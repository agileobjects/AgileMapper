namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Configuration;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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
            => mapperData.TargetMember.Type.IsAssignableTo(_creationTargetType) && base.AppliesTo(callbackPosition, mapperData);

        protected override bool MemberPathMatches(IBasicMapperData mapperData)
             => MemberPathHasMatchingSourceAndTargetTypes(mapperData);

        protected override Expression GetConditionOrNull(IMemberMapperData mapperData, CallbackPosition position)
        {
            var condition = base.GetConditionOrNull(mapperData, position);

            if ((CallbackPosition != CallbackPosition.After) || mapperData.TargetMemberIsUserStruct())
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