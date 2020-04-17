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

    internal class ValueInjectionArgs
    {
        private readonly LambdaExpression _lambda;

        public ValueInjectionArgs(
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

        private Expression GetAppropriateMappingContextAccess()
            => MapperData.GetAppropriateMappingContextAccess(ContextTypes);

        public ValueInjectionContext GetAppropriateMappingContext()
        {
            if (UseSimpleTypeMappingContextInfo())
            {
                return GetSimpleTypesMappingContextInfo();
            }

            if (ContextTypesMatch())
            {
                return new ValueInjectionContext(this);
            }

            var contextAccess = GetAppropriateMappingContextAccess();

            return new ValueInjectionContext(this, contextAccess);
        }

        private bool UseSimpleTypeMappingContextInfo()
        {
            // If ContextTargetType == object, it's a configured
            // simple -> target context; use a parent context
            // even if the MapperData types match the context types as
            // the MapperData won't have the correct MappingDataObject:
            return ContextSourceType.IsSimple() && ContextTargetType != typeof(object);
        }

        private ValueInjectionContext GetSimpleTypesMappingContextInfo()
        {
            var mapperData = MapperData;
            var contextMapperData = MapperData;

            IQualifiedMember targetMember;
            IQualifiedMember sourceMember;

            while (true)
            {
                if (contextMapperData.TargetMemberIsEnumerableElement())
                {
                    sourceMember = mapperData.SourceMember.RelativeTo(contextMapperData.SourceMember);
                    targetMember = mapperData.TargetMember.RelativeTo(contextMapperData.TargetMember);
                    break;
                }

                if (!contextMapperData.IsEntryPoint)
                {
                    contextMapperData = contextMapperData.Parent;
                    continue;
                }

                sourceMember = mapperData.SourceMember;
                targetMember = mapperData.TargetMember;

                contextMapperData = mapperData.GetAppropriateMappingContext(
                    mapperData.SourceMember.RootType,
                    targetMember.RootType);

                break;
            }

            return new ValueInjectionContext(
                this,
                mapperData.MappingDataObject,
                sourceMember.GetQualifiedAccess(contextMapperData.SourceObject),
                targetMember.GetQualifiedAccess(contextMapperData.TargetInstance));
        }

        public Expression GetInvocationContextArgument(ValueInjectionContext context)
        {
            if (context.IsCallback())
            {
                return _lambda.ReplaceParameterWith(context.GetMappingDataAccess());
            }

            var createObjectCreationContextCall = Expression.Call(
                ObjectCreationMappingData.CreateMethod.MakeGenericMethod(context.Types),
                context.GetMappingDataAccess(),
                context.GetCreatedObject());

            return _lambda.ReplaceParameterWith(createObjectCreationContextCall);
        }
    }
}