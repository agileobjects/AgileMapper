namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal static class DerivedTypeMappingConverter
    {
        public static bool TryConvert(
            BlockExpression mappingBlock,
            IQueryProjectionModifier modifier,
            out Expression converted)
        {
            if (IsNotDerivedTypeMappingsBlock(mappingBlock, out var derivedTypeMappings))
            {
                converted = null;
                return false;
            }

            var fallbackValue = GetReturnedValue(mappingBlock.Expressions.Last(), modifier);

            converted = derivedTypeMappings.Reverse().Aggregate(
                fallbackValue,
                (mappingSoFar, derivedTypeMapping) => Expression.Condition(
                    derivedTypeMapping.Test,
                    GetReturnedValue(derivedTypeMapping.IfTrue, modifier).GetConversionTo(fallbackValue.Type),
                    mappingSoFar));

            return true;
        }

        private static bool IsNotDerivedTypeMappingsBlock(
            BlockExpression mappingBlock,
            out ICollection<ConditionalExpression> derivedTypeMappings)
        {
            derivedTypeMappings = new List<ConditionalExpression>();

            var firstExpression = mappingBlock.Expressions.First();

            switch (firstExpression.NodeType)
            {
                case Block:
                    var nestedMappingBlock = (BlockExpression)firstExpression;

                    if (nestedMappingBlock.Expressions.All(exp => exp.NodeType == Conditional))
                    {
                        foreach (var derivedTypeMapping in nestedMappingBlock.Expressions)
                        {
                            derivedTypeMappings.Add((ConditionalExpression)derivedTypeMapping);
                        }

                        return false;
                    }

                    break;
            }

            return true;
        }

        private static Expression GetReturnedValue(
            Expression returnExpression,
            IQueryProjectionModifier modifier)
        {
            switch (returnExpression.NodeType)
            {
                case Label:
                    returnExpression = ((LabelExpression)returnExpression).DefaultValue;
                    break;

                case Goto:
                    returnExpression = ((GotoExpression)returnExpression).Value;
                    break;
            }

            return modifier.Modify(returnExpression);
        }
    }
}