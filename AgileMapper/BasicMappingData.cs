namespace AgileObjects.AgileMapper
{
    using System;
    using Members;

    internal class BasicMappingData<TSource, TTarget> :
        MappingInstanceDataBase<TSource, TTarget>,
        IBasicMappingData
    {
        private readonly IBasicMappingData _parent;
        private IBasicMapperData _mapperData;

        public BasicMappingData(
           TSource source,
           TTarget target,
           int? enumerableIndex,
           Type runtimeSourceType,
           Type runtimeTargetType,
           MappingRuleSet ruleSet,
           IBasicMappingData parent)
           : base(source, target, enumerableIndex, parent)
        {
            RuleSet = ruleSet;
            SourceType = runtimeSourceType;
            TargetType = runtimeTargetType;
            _parent = parent;
        }

        public MappingRuleSet RuleSet { get; }

        public Type SourceType { get; }

        public Type TargetType { get; }


        IBasicMapperData IBasicMappingData.MapperData => _mapperData ?? (_mapperData = CreateMapperData());

        private IBasicMapperData CreateMapperData()
            => new BasicMapperData(RuleSet, SourceType, TargetType, QualifiedMember.All, _parent?.MapperData);

        T IMappingData.GetSource<T>() => (T)(object)Source;

        T IMappingData.GetTarget<T>() => (T)(object)Target;

        int? IMappingData.GetEnumerableIndex() => EnumerableIndex ?? Parent?.GetEnumerableIndex();

        IMappingData<TDataSource, TDataTarget> IMappingData.As<TDataSource, TDataTarget>()
            => (IMappingData<TDataSource, TDataTarget>)this;
    }
}