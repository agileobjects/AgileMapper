namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;

    internal static class Parameters
    {
        public static readonly ParameterExpression MappingContext = Create<IMappingContext>();
        public static readonly ParameterExpression MappingData = Create<IMappingData>();
        public static readonly ParameterExpression SourceObject = Create<object>();
        public static readonly ParameterExpression ObjectMappingData = Create<IObjectMappingData>();
        public static readonly ParameterExpression Queryable = Create<IQueryable>();
        public static readonly ParameterExpression Mapper = Create<IMapper>();

        public static ParameterExpression Create<T>(string name = null) => Create(typeof(T), name);

        public static ParameterExpression Create(Type type) => Create(type, type.GetShortVariableName());

        public static ParameterExpression Create(Type type, string name)
            => Expression.Parameter(type, name ?? type.GetShortVariableName());
    }
}