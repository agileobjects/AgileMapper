namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class EnumerablePopulationContext
    {
        public EnumerablePopulationContext(IBasicMapperData mapperData)
        {
            SourceElementType = mapperData.SourceType.GetEnumerableElementType();
            TargetElementType = mapperData.TargetMember.ElementType;
            ElementTypes = new[] { SourceElementType, TargetElementType };
            ElementTypesAreTheSame = SourceElementType == TargetElementType;
            ElementTypesAreSimple = TargetElementType.IsSimple();
        }

        public Type SourceElementType { get; }

        public Type TargetElementType { get; }

        public Type[] ElementTypes { get; }

        public bool ElementTypesAreTheSame { get; }

        public bool ElementTypesAreSimple { get; }

        public ParameterExpression GetSourceParameterFor(Type type) => GetParameterFor(type, "source");

        public ParameterExpression GetTargetParameterFor(Type type) => GetParameterFor(type, "target");

        private ParameterExpression GetParameterFor(Type type, string sameTypesPrefix)
        {
            var parameterName = ElementTypesAreTheSame
                ? sameTypesPrefix + type.GetVariableNameInPascalCase()
                : type.GetVariableNameInCamelCase();

            var parameter = Expression.Parameter(type, parameterName);

            return parameter;
        }
    }
}