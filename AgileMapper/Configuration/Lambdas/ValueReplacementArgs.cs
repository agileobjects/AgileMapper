namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;

    internal class ValueReplacementArgs
    {
        private readonly LambdaExpression _lambda;

        public ValueReplacementArgs(
            LambdaExpression lambda,
            MappingConfigInfo configInfo,
            Type[] contextTypes,
            IMemberMapperData mapperData)
        {
            _lambda = lambda;
            UseTargetObject = IsBeforeObjectCreation(configInfo);
            ContextTypes = (contextTypes.Length > 1) ? contextTypes : contextTypes.Append(typeof(object));
            MapperData = mapperData;
        }

        private static bool IsBeforeObjectCreation(MappingConfigInfo configInfo)
        {
            if (configInfo.InvocationPosition == InvocationPosition.After)
            {
                return false;
            }

            var targetMember = configInfo.Get<QualifiedMember>();

            return targetMember == null ||
                   targetMember == QualifiedMember.None ||
                   targetMember == QualifiedMember.All;
        }

        public bool UseTargetObject { get; }

        public Type[] ContextTypes { get; }

        public Type ContextSourceType => ContextTypes[0];

        public Type ContextTargetType => ContextTypes[1];

        public IMemberMapperData MapperData { get; }

        private bool ContextTypesMatch() => MapperData.TypesMatch(ContextTypes);

        public ValueReplacementContext GetValueReplacementContext()
        {
            if (UseSimpleTypeValueReplacementContext())
            {
                return GetSimpleTypesValueReplacementContext();
            }

            if (ContextTypesMatch())
            {
                return new ValueReplacementContext(this);
            }

            var contextAccess = MapperData
                .GetAppropriateMappingContextAccess(
                    out var contextMapperData,
                    ContextTypes);

            return new ValueReplacementContext(this, contextMapperData, contextAccess);
        }

        private bool UseSimpleTypeValueReplacementContext()
        {
            // If ContextTargetType == object, it's a configured
            // simple -> target context; use a parent context
            // even if the MapperData types match the context types as
            // the MapperData won't have the correct MappingDataObject:
            return ContextSourceType.IsSimple() && ContextTargetType != typeof(object);
        }

        private ValueReplacementContext GetSimpleTypesValueReplacementContext()
        {
            var contextMapperData = MapperData;

            IQualifiedMember targetMember;
            IQualifiedMember sourceMember;

            while (true)
            {
                if (contextMapperData.TargetMemberIsEnumerableElement())
                {
                    sourceMember = MapperData.SourceMember.RelativeTo(contextMapperData.SourceMember);
                    targetMember = MapperData.TargetMember.RelativeTo(contextMapperData.TargetMember);
                    break;
                }

                if (!contextMapperData.IsEntryPoint)
                {
                    contextMapperData = contextMapperData.Parent;
                    continue;
                }

                sourceMember = MapperData.SourceMember;
                targetMember = MapperData.TargetMember;

                contextMapperData = MapperData.GetAppropriateMappingContext(
                    MapperData.SourceMember.RootType,
                    targetMember.RootType);

                break;
            }

            return new ValueReplacementContext(
                this,
                sourceMember.GetQualifiedAccess(contextMapperData.SourceObject),
                targetMember.GetQualifiedAccess(contextMapperData.TargetInstance));
        }

        public Expression GetFuncInvokeMappingDataArgument(ValueReplacementContext context)
        {
            if (context.IsCallback())
            {
                return _lambda.ReplaceParameterWith(context.GetToMappingDataCall());
            }

            var createObjectCreationContextCall = Expression.Call(
                ObjectCreationMappingData.CreateMethod.MakeGenericMethod(context.Types),
                context.GetToMappingDataCall(),
                context.GetCreatedObject());

            return _lambda.ReplaceParameterWith(createObjectCreationContextCall);
        }
    }
}