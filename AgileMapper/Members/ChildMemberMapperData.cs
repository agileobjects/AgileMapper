namespace AgileObjects.AgileMapper.Members
{
    using ObjectPopulation;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ChildMemberMapperData : BasicMapperData, IMemberMapperData
    {
        public ChildMemberMapperData(QualifiedMember targetMember, ObjectMapperData parent)
            : this(
                parent.SourceMember,
                targetMember,
                parent)
        {
        }

        public ChildMemberMapperData(IQualifiedMember sourceMember, QualifiedMember targetMember, ObjectMapperData parent)
            : base(
                parent.RuleSet,
                parent.SourceType,
                parent.TargetType,
                sourceMember,
                targetMember,
                parent)
        {
            SourceMember = sourceMember;
            Parent = parent;
            Context = new MapperDataContext(this);
        }

        public MapperContext MapperContext => Parent.MapperContext;

        public bool IsEntryPoint => Context.IsStandalone || TargetMember.IsRecursion;

        public ObjectMapperData Parent { get; }

        public MapperDataContext Context { get; }

        public Expression ParentObject => Parent.ParentObject;

        public ParameterExpression MappingDataObject => Parent.MappingDataObject;

        public IQualifiedMember SourceMember { get; }

        public Expression SourceObject => Parent.SourceObject;

        public Expression TargetObject => Parent.TargetObject;

        public Expression CreatedObject => Parent.CreatedObject;

        public Expression EnumerableIndex => Parent.EnumerableIndex;

        public Expression TargetInstance => Parent.TargetInstance;

        public ExpressionInfoFinder ExpressionInfoFinder => Parent.ExpressionInfoFinder;

        public override bool HasCompatibleTypes(ITypePair typePair) => Parent.HasCompatibleTypes(typePair);
    }
}