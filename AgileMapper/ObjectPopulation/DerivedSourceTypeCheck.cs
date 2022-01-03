namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;

    internal class DerivedSourceTypeCheck
    {
        public DerivedSourceTypeCheck(Type derivedSourceType)
        {
            DerivedSourceType = derivedSourceType;

            var typedVariableName = derivedSourceType.GetSourceValueVariableName();
            TypedVariable = Expression.Variable(derivedSourceType, typedVariableName);
        }

        public Type DerivedSourceType { get; }

        public ParameterExpression TypedVariable { get; }

        public Expression GetTypedVariableAssignment(IMemberMapperData declaredTypeMapperData)
            => GetTypedVariableAssignment(declaredTypeMapperData.SourceObject);

        public Expression GetTypedVariableAssignment(Expression sourceObject)
        {
            var typeAsConversion = Expression.TypeAs(sourceObject, DerivedSourceType);
            var typedVariableAssignment = TypedVariable.AssignTo(typeAsConversion);

            return typedVariableAssignment;
        }

        public Expression TypeCheck => TypedVariable.GetIsNotDefaultComparison();
    }
}