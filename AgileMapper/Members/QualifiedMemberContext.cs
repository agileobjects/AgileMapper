namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal class QualifiedMemberContext : IQualifiedMemberContext
    {
        private readonly IQualifiedMemberContext _parent;

        public QualifiedMemberContext(
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            IQualifiedMemberContext parent)
            : this(
                parent.RuleSet,
                parent.SourceType,
                parent.TargetType,
                sourceMember,
                targetMember,
                parent,
                parent.MapperContext)
        {
        }

        public QualifiedMemberContext(
            MappingRuleSet ruleSet,
            Type sourceType,
            Type targetType,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            IQualifiedMemberContext parent,
            MapperContext mapperContext)
            : this(
                ruleSet,
                sourceType,
                targetType,
                targetMember,
                parent,
                mapperContext)
        {
            SourceMember = sourceMember.SetContext(this);
        }

        public QualifiedMemberContext(
            MappingRuleSet ruleSet,
            Type sourceType,
            Type targetType,
            QualifiedMember targetMember,
            IQualifiedMemberContext parent,
            MapperContext mapperContext)
        {
            if (parent == null)
            {
                IsRoot = true;
                IsEntryPoint = true;
            }
            else
            {
                _parent = parent;
                IsEntryPoint = sourceType == typeof(object);
            }

            SourceType = sourceType;
            TargetType = targetType;
            RuleSet = ruleSet;
            MapperContext = mapperContext;
            TargetMember = targetMember?.SetContext(this) ?? QualifiedMember.All;
        }

        public MapperContext MapperContext { get; }

        IQualifiedMemberContext IQualifiedMemberContext.Parent => _parent;

        public bool IsRoot { get; }

        public virtual bool IsEntryPoint { get; }

        public MappingRuleSet RuleSet { get; }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public virtual IQualifiedMember SourceMember { get; }

        public QualifiedMember TargetMember { get; }

        public virtual bool HasCompatibleTypes(ITypePair typePair)
            => typePair.HasTypesCompatibleWith(this);

        bool ITypePair.IsForSourceType(ITypePair typePair) => this.IsForSourceType(typePair);

        bool ITypePair.IsForTargetType(ITypePair typePair) => this.IsForTargetType(typePair);
    }
}