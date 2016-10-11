namespace AgileObjects.AgileMapper
{
    using System;
    using Members;

    internal class BasicMappingContextData<TSource, TTarget> :
        MappingInstanceDataBase<TSource, TTarget>,
        IBasicMappingContextData
    {
        private readonly IBasicMappingContextData _parent;

        public BasicMappingContextData(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            MappingRuleSet ruleSet,
            IBasicMappingContextData parent)
            : base(source, target, enumerableIndex, parent)
        {
            SourceMember = sourceMember;
            TargetMember = targetMember;
            RuleSet = ruleSet;
            _parent = parent;
        }

        public MappingRuleSet RuleSet { get; }

        IBasicMapperData IBasicMapperData.Parent => _parent;

        public Type SourceType => SourceMember.Type;

        public Type TargetType => TargetMember.Type;

        public IQualifiedMember SourceMember { get; }

        public QualifiedMember TargetMember { get; }

        T IMappingData.GetSource<T>() => (T)(object)Source;

        T IMappingData.GetTarget<T>() => (T)(object)Target;

        int? IMappingData.GetEnumerableIndex() => EnumerableIndex ?? Parent?.GetEnumerableIndex();

        IMappingData<TDataSource, TDataTarget> IMappingData.As<TDataSource, TDataTarget>()
            => (IMappingData<TDataSource, TDataTarget>)this;
    }
}