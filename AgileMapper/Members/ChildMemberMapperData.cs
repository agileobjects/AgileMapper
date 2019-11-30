namespace AgileObjects.AgileMapper.Members
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using ObjectPopulation;

    internal class ChildMemberMapperData : BasicMapperData, IMemberMapperData
    {
        private readonly bool _useParentForTypeCheck;
        private bool? _isRepeatMapping;

        public ChildMemberMapperData(QualifiedMember targetMember, ObjectMapperData parent)
            : base(parent.SourceMember, targetMember, parent)
        {
            _useParentForTypeCheck = true;
            Parent = parent;
            Context = new MapperDataContext(this);
        }

        public ChildMemberMapperData(IQualifiedMember sourceMember, QualifiedMember targetMember, ObjectMapperData parent)
            : base(
                parent.RuleSet,
                sourceMember.Type,
                targetMember.Type,
                sourceMember,
                targetMember,
                parent)
        {
            Parent = parent;
            Context = new MapperDataContext(this);
        }

        public MapperContext MapperContext => Parent.MapperContext;

        public override bool IsEntryPoint => Context.IsStandalone || IsRepeatMapping;

        private bool IsRepeatMapping => (_isRepeatMapping ?? (_isRepeatMapping = this.IsRepeatMapping())).Value;

        public ObjectMapperData Parent { get; }

        public MapperDataContext Context { get; }

        public Expression ParentObject => Parent.ParentObject;

        public ParameterExpression MappingDataObject => Parent.MappingDataObject;

        public Expression SourceObject => Parent.SourceObject;

        public Expression TargetObject => Parent.TargetObject;

        public Expression CreatedObject => Parent.CreatedObject;

        public Expression ElementIndex => Parent.ElementIndex;

        public Expression TargetInstance => Parent.TargetInstance;

        public ExpressionInfoFinder ExpressionInfoFinder => Parent.ExpressionInfoFinder;

        public override bool HasCompatibleTypes(ITypePair typePair)
        {
            return _useParentForTypeCheck
                ? Parent.HasCompatibleTypes(typePair)
                : typePair.HasCompatibleTypes(this, SourceMember, TargetMember);
        }
    }
}