namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class EnumerablePopulationContext
    {
        public EnumerablePopulationContext(IMemberMapperData mapperData)
        {
            SourceElementType = mapperData.SourceMember.ElementType;

            if (SourceElementType == null)
            {
                return;
            }

            TargetElementType = mapperData.TargetMember.GetElementType(SourceElementType);
            ElementTypes = new[] { SourceElementType, TargetElementType };
            ElementTypesAreTheSame = SourceElementType == TargetElementType;
            ElementTypesAreSimple = TargetElementType.IsSimple();
        }

        public Type SourceElementType { get; }

        public Type TargetElementType { get; }

        public Type[] ElementTypes { get; }

        public bool ElementTypesAreTheSame { get; }

        public bool ElementTypesAreAssignable => SourceElementType.IsAssignableTo(TargetElementType);

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