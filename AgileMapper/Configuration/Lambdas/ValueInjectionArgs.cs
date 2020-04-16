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
        public ValueInjectionArgs(
            LambdaExpression lambda,
            InvocationPosition invocationPosition,
            Type[] contextTypes,
            IMemberMapperData mapperData)
        {
            Lambda = lambda;
            InvocationPosition = invocationPosition;
            ContextTypes = (contextTypes.Length > 1) ? contextTypes : contextTypes.Append(typeof(object));
            MapperData = mapperData;
        }

        public LambdaExpression Lambda { get; }

        public InvocationPosition InvocationPosition { get; }

        public Type[] ContextTypes { get; }

        public Type ContextSourceType => ContextTypes[0];

        public Type ContextTargetType => ContextTypes[1];

        public IMemberMapperData MapperData { get; }

        public bool ContextTypesMatch() => MapperData.TypesMatch(ContextTypes);

        public Expression GetAppropriateMappingContextAccess()
            => MapperData.GetAppropriateMappingContextAccess(ContextTypes);

        public Expression GetTypedContextAccess(Expression contextAccess)
            => MapperData.GetTypedContextAccess(contextAccess, ContextTypes);

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
            if (context.IsCallback)
            {
                return Lambda.ReplaceParameterWith(context.MappingDataAccess);
            }

            var createObjectCreationContextCall = Expression.Call(
                ObjectCreationMappingData.CreateMethod.MakeGenericMethod(context.Types),
                context.MappingDataAccess,
                context.CreatedObject);

            return Lambda.ReplaceParameterWith(createObjectCreationContextCall);
        }
    }
}