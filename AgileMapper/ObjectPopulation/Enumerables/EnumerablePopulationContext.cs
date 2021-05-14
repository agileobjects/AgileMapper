namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;

    internal class EnumerablePopulationContext
    {
        public EnumerablePopulationContext(IQualifiedMemberContext context)
        {
            SourceElementType = context.SourceMember.ElementType;

            if (SourceElementType == null)
            {
                return;
            }

            TargetElementType = context.TargetMember.GetElementType(SourceElementType);
            ElementTypes = new[] { SourceElementType, TargetElementType };
            ElementTypesAreTheSame = SourceElementType == TargetElementType;
            TargetElementsAreSimple = TargetElementType.IsSimple();
        }

        public Type SourceElementType { get; }

        public Type TargetElementType { get; }

        public Type[] ElementTypes { get; }

        public bool ElementTypesAreTheSame { get; }

        public bool ElementTypesAreAssignable => SourceElementType.IsAssignableTo(TargetElementType);

        public bool TargetElementsAreSimple { get; }

        public ParameterExpression GetSourceParameterFor(Type type, string prefix = null)
            => GetParameterFor(type, prefix, "source");

        public ParameterExpression GetTargetParameterFor(Type type, string prefix = null)
            => GetParameterFor(type, prefix, "target");

        private ParameterExpression GetParameterFor(
            Type type,
            string prefix,
            string sameTypesPrefix)
        {
            string parameterName;

            if (ElementTypesAreTheSame)
            {
                parameterName =
                    prefix +
                    sameTypesPrefix +
                    type.GetVariableNameInPascalCase();
            }
            else
            {
                parameterName = prefix != null
                    ? prefix + type.GetVariableNameInPascalCase()
                    : type.GetVariableNameInCamelCase();
            }

            return Parameters.Create(type, parameterName);
        }
    }
}