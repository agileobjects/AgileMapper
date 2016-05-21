namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Extensions;
    using Members;

    internal class ObjectMappingRequest<TDeclaredSource, TDeclaredTarget, TDeclaredInstance>
    {
        public ObjectMappingRequest(
            TDeclaredSource source,
            IQualifiedMember sourceMember,
            TDeclaredTarget target,
            IQualifiedMember targetMember,
            TDeclaredInstance existingTargetInstance,
            IQualifiedMember existingTargetInstanceMember,
            int? enumerableIndex,
            MappingContext mappingContext)
        {
            Source = source;
            SourceMember = sourceMember.WithType(source.GetRuntimeSourceType());

            Target = target;
            var runtimeTargetType = GetTargetType(SourceMember, targetMember, target, mappingContext);
            TargetMember = targetMember.WithType(runtimeTargetType);

            ExistingTargetInstance = existingTargetInstance;
            var runtimeInstanceType = GetTargetType(SourceMember, existingTargetInstanceMember, existingTargetInstance, mappingContext);
            ExistingTargetInstanceMember = existingTargetInstanceMember.WithType(runtimeInstanceType);

            EnumerableIndex = enumerableIndex ?? mappingContext.CurrentObjectMappingContext?.GetEnumerableIndex();
            MappingContext = mappingContext;
        }

        #region Setup

        private static Type GetTargetType<TTarget>(
            IQualifiedMember sourceMember,
            IQualifiedMember targetMember,
            TTarget target,
            MappingContext mappingContext)
        {
            var mappingData = new BasicMappingData(mappingContext.RuleSet, sourceMember.Type, typeof(TTarget));

            var qualifiedSourceType = GetQualifiedSourceType(sourceMember, targetMember, mappingContext);

            if (qualifiedSourceType != sourceMember.Type)
            {
                mappingData = new BasicMappingData(mappingContext.RuleSet, qualifiedSourceType, typeof(TTarget), mappingData);
            }

            return mappingContext.MapperContext.UserConfigurations.GetDerivedTypeOrNull(mappingData)
                ?? target.GetRuntimeTargetType(sourceMember.Type);
        }

        private static Type GetQualifiedSourceType(
            IQualifiedMember sourceMember,
            IQualifiedMember targetMember,
            MappingContext mappingContext)
        {
            if ((mappingContext.CurrentObjectMappingContext == null) || sourceMember.Matches(targetMember))
            {
                return sourceMember.Type;
            }

            var memberMappingContext = new MemberMappingContext(targetMember, mappingContext.CurrentObjectMappingContext);
            var matchingSourceMember = mappingContext.MapperContext.DataSources.GetSourceMemberFor(memberMappingContext);

            if (matchingSourceMember == null)
            {
                return sourceMember.Type;
            }

            return mappingContext.CurrentObjectMappingContext.GetSourceMemberRuntimeType(matchingSourceMember);
        }

        #endregion

        public TDeclaredSource Source { get; }

        public IQualifiedMember SourceMember { get; }

        public TDeclaredTarget Target { get; }

        public IQualifiedMember TargetMember { get; }

        public TDeclaredInstance ExistingTargetInstance { get; }

        public IQualifiedMember ExistingTargetInstanceMember { get; }

        public int? EnumerableIndex { get; }

        public MappingContext MappingContext { get; }

        #region Helper Class

        private class BasicMappingData : IMappingData
        {
            public BasicMappingData(
                MappingRuleSet ruleSet,
                Type sourceType,
                Type targetType,
                IMappingData parent = null)
            {
                Parent = parent;
                SourceType = sourceType;
                TargetType = targetType;
                RuleSetName = ruleSet.Name;
            }

            public IMappingData Parent { get; }

            public string RuleSetName { get; }

            public Type SourceType { get; }

            public Type TargetType { get; }

            public IQualifiedMember TargetMember => QualifiedMember.All;
        }

        #endregion
    }
}