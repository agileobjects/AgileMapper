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
            var runtimeSourceType = source.GetRuntimeSourceType();
            SourceMember = sourceMember.WithType(runtimeSourceType);

            Target = target;
            var runtimeTargetType = GetTargetType(target, runtimeSourceType, mappingContext);
            TargetMember = targetMember.WithType(runtimeTargetType);

            ExistingTargetInstance = existingTargetInstance;
            var runtimeInstanceType = GetTargetType(existingTargetInstance, runtimeSourceType, mappingContext);
            ExistingTargetInstanceMember = existingTargetInstanceMember.WithType(runtimeInstanceType);

            EnumerableIndex = enumerableIndex ?? mappingContext.CurrentObjectMappingContext?.GetEnumerableIndex();
            MappingContext = mappingContext;
        }

        private static Type GetTargetType<TTarget>(TTarget target, Type sourceType, MappingContext mappingContext)
        {
            var mappingData =
                mappingContext.CurrentObjectMappingContext ??
                (IMappingData)new BasicMappingData(mappingContext.RuleSet, sourceType, typeof(TDeclaredTarget));

            return mappingContext.MapperContext.UserConfigurations.GetDerivedTypeOrNull(mappingData)
                ?? target.GetRuntimeTargetType(sourceType);
        }

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
            public BasicMappingData(MappingRuleSet ruleSet, Type sourceType, Type targetType)
            {
                SourceType = sourceType;
                TargetType = targetType;
                RuleSetName = ruleSet.Name;
            }

            public IMappingData Parent => null;

            public string RuleSetName { get; }

            public Type SourceType { get; }

            public Type TargetType { get; }

            public IQualifiedMember TargetMember => QualifiedMember.All;
        }

        #endregion
    }
}