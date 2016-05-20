﻿namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using DataSources;
    using ObjectPopulation;

    internal class MemberMappingContext : IMemberMappingContext
    {
        public MemberMappingContext(IQualifiedMember targetMember, IObjectMappingContext parent)
        {
            TargetMember = targetMember;
            Parent = parent;
        }

        public MapperContext MapperContext => Parent.MapperContext;

        public MappingContext MappingContext => Parent.MappingContext;

        public IObjectMappingContext Parent { get; }

        public ParameterExpression Parameter => Parent.Parameter;

        public string RuleSetName => Parent.RuleSetName;

        public IQualifiedMember SourceMember => Parent.SourceMember;

        public Expression SourceObject => Parent.SourceObject;

        public int SourceObjectDepth => Parent.SourceObjectDepth;

        public IQualifiedMember TargetMember { get; }

        public Expression ExistingObject => Parent.ExistingObject;

        public Expression EnumerableIndex => Parent.EnumerableIndex;

        public ParameterExpression InstanceVariable => Parent.InstanceVariable;

        public NestedAccessFinder NestedAccessFinder => Parent.NestedAccessFinder;

        IEnumerable<IDataSource> IMemberMappingContext.GetDataSources() => this.GetDataSources();

        Expression IMemberMappingContext.GetTryCall(Expression expression) => Parent.GetTryCall(expression);
    }
}