namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class DerivedSourceTypeCheck
    {
        private readonly Type _derivedSourceType;

        public DerivedSourceTypeCheck(Type derivedSourceType)
        {
            _derivedSourceType = derivedSourceType;

            var typedVariableName = "source" + derivedSourceType.GetVariableNameInPascalCase();
            TypedVariable = Expression.Variable(derivedSourceType, typedVariableName);
        }

        public ParameterExpression TypedVariable { get; }

        public Expression GetTypedVariableAssignment(IMemberMapperData declaredTypeMapperData)
            => GetTypedVariableAssignment(declaredTypeMapperData.SourceObject);

        public Expression GetTypedVariableAssignment(Expression sourceObject)
        {
            var typeAsConversion = Expression.TypeAs(sourceObject, _derivedSourceType);
            var typedVariableAssignment = TypedVariable.AssignTo(typeAsConversion);

            return typedVariableAssignment;
        }

        public Expression TypeCheck => TypedVariable.GetIsNotDefaultComparison();
    }
}