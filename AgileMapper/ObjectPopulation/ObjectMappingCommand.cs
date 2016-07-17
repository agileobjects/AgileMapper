namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Extensions;
    using Members;

    internal interface IObjectMappingCommand<out T>
    {
        T Execute();

        IObjectMappingContext ToOmc();
    }

    internal class ObjectMappingCommand
    {
        public static ObjectMappingCommand<TSource, TTarget> Create<TSource, TTarget>(
            IQualifiedMember sourceMember,
            TSource source,
            QualifiedMember targetMember,
            TTarget target,
            int? enumerableIndex,
            MappingContext mappingContext)
        {
            sourceMember = sourceMember.WithType(source.GetRuntimeSourceType());
            targetMember = GetTargetMember(sourceMember, targetMember, target, mappingContext);

            return new ObjectMappingCommand<TSource, TTarget>(
                sourceMember,
                source,
                targetMember,
                target,
                enumerableIndex,
                mappingContext);
        }

        private static QualifiedMember GetTargetMember<TTarget>(
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            TTarget target,
            MappingContext mappingContext)
        {
            var mappingData = new BasicMappingData(mappingContext.RuleSet, sourceMember.Type, typeof(TTarget));

            var targetMemberType =
                mappingContext.MapperContext.UserConfigurations.GetDerivedTypeOrNull(mappingData)
                    ?? target.GetRuntimeTargetType(sourceMember.Type);

            targetMember = targetMember.WithType(targetMemberType);
            return targetMember;
        }

        #region Helper Classes

        private class BasicMappingData : IMappingData
        {
            public BasicMappingData(
                MappingRuleSet ruleSet,
                Type sourceType,
                Type targetType)
            {
                SourceType = sourceType;
                TargetType = targetType;
                RuleSetName = ruleSet.Name;
            }

            public IMappingData Parent => null;

            public string RuleSetName { get; }

            public Type SourceType { get; }

            public Type TargetType { get; }

            public QualifiedMember TargetMember => QualifiedMember.All;
        }

        #endregion
    }

    internal class ObjectMappingCommand<TSource, TTarget> : IObjectMappingCommand<TTarget>
    {
        public ObjectMappingCommand(
            IQualifiedMember sourceMember,
            TSource source,
            QualifiedMember targetMember,
            TTarget target,
            int? enumerableIndex,
            MappingContext mappingContext)
        {
            Source = source;
            SourceMember = sourceMember;

            Target = target;
            TargetMember = targetMember;

            EnumerableIndex = enumerableIndex;
            MappingContext = mappingContext;
        }

        public TSource Source { get; set; }

        public IQualifiedMember SourceMember { get; }

        public TTarget Target { get; set; }

        public QualifiedMember TargetMember { get; }

        public int? EnumerableIndex { get; set; }

        public MappingContext MappingContext { get; }

        public TTarget Execute() => MappingContext.MapChild(this);

        public IObjectMappingContext ToOmc() => ObjectMappingContextFactory.Create(this);
    }
}