﻿namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class ChildMemberMapperData : BasicMapperData, IMemberMapperData
    {
        public ChildMemberMapperData(QualifiedMember targetMember, ObjectMapperData parent)
            : base(
                parent.RuleSet,
                parent.SourceType,
                parent.TargetType,
                parent.SourceMember,
                targetMember,
                parent)
        {
            Parent = parent;
            Context = new MapperDataContext(this);
        }

        public MapperContext MapperContext => Parent.MapperContext;

        public ObjectMapperData Parent { get; }

        public MapperDataContext Context { get; }

        public Expression ParentObject => Parent.ParentObject;

        public ParameterExpression MappingDataObject => Parent.MappingDataObject;

        public IQualifiedMember SourceMember => Parent.SourceMember;

        public Expression SourceObject => Parent.SourceObject;

        public Expression TargetObject => Parent.TargetObject;

        public Expression CreatedObject => Parent.CreatedObject;

        public Expression EnumerableIndex => Parent.EnumerableIndex;

        public Expression TargetInstance => Parent.TargetInstance;

        public ExpressionInfoFinder ExpressionInfoFinder => Parent.ExpressionInfoFinder;

        public override bool HasCompatibleTypes(ITypePair typePair) => Parent.HasCompatibleTypes(typePair);
    }
}