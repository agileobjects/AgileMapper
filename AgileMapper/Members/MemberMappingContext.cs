namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Linq.Expressions;
    using DataSources;
    using ObjectPopulation;

    internal class MemberMappingContext : IMemberMappingContext
    {
        private readonly IObjectMappingContext _parent;

        public MemberMappingContext(QualifiedMember targetMember, IObjectMappingContext parent)
        {
            _parent = parent;
            TargetMember = targetMember;
        }

        public MapperContext MapperContext => _parent.MapperContext;

        public MappingContext MappingContext => _parent.MappingContext;

        IMappingData IMappingData.Parent => _parent;

        IObjectMappingContext IMemberMappingContext.Parent => _parent;

        public ParameterExpression Parameter => _parent.Parameter;

        public string RuleSetName => _parent.RuleSetName;

        public IQualifiedMember SourceMember => _parent.SourceMember;

        public Expression SourceObject => _parent.SourceObject;

        Type IMappingData.SourceType => SourceObject.Type;

        public QualifiedMember TargetMember { get; }

        public Expression ExistingObject => _parent.ExistingObject;

        Type IMappingData.TargetType => ExistingObject.Type;

        public Expression EnumerableIndex => _parent.EnumerableIndex;

        public ParameterExpression InstanceVariable => _parent.InstanceVariable;

        public NestedAccessFinder NestedAccessFinder => _parent.NestedAccessFinder;

        DataSourceSet IMemberMappingContext.GetDataSources() => this.GetDataSources();
    }
}