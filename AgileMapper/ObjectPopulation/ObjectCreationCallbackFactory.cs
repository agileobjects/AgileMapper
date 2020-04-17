namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Configuration;
    using Configuration.Lambdas;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;

    internal class ObjectCreationCallbackFactory : MappingCallbackFactory
    {
        private readonly Type _creationTargetType;

        public ObjectCreationCallbackFactory(
            MappingConfigInfo configInfo,
            Type creationTargetType,
            ConfiguredLambdaInfo callbackLambda)
            : base(configInfo, callbackLambda, QualifiedMember.All)
        {
            _creationTargetType = creationTargetType;
        }

        public override bool AppliesTo(InvocationPosition invocationPosition, IQualifiedMemberContext context)
            => context.TargetMember.Type.IsAssignableTo(_creationTargetType) && base.AppliesTo(invocationPosition, context);

        protected override bool TypesMatch(IQualifiedMemberContext context)
             => SourceAndTargetTypesMatch(context);

        public override Expression GetConditionOrNull(IMemberMapperData mapperData)
        {
            var condition = base.GetConditionOrNull(mapperData);

            if ((InvocationPosition != InvocationPosition.After) || mapperData.TargetMemberIsUserStruct())
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