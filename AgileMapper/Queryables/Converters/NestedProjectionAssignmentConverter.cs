namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Settings;

    internal static class NestedProjectionAssignmentConverter
    {
        public static bool TryConvert(
            MemberAssignment assignment,
            IQueryProviderSettings settings,
            Func<Expression, Expression> modifyCallback,
            out MemberAssignment converted)
        {
            if (settings.SupportsEnumerableMaterialisation &&
               (assignment.Expression.NodeType == ExpressionType.Call) &&
                assignment.Expression.Type.IsEnumerable())
            {
                converted = ConvertToMaterialisation(assignment, modifyCallback);
                return true;
            }

            converted = null;
            return false;
        }

        private static MemberAssignment ConvertToMaterialisation(
            MemberAssignment assignment,
            Func<Expression, Expression> modifyCallback)
        {
            var materialisedNestedProjection = GetMaterialisedNestedProjection(assignment.Expression);
            var modifiedProjection = modifyCallback.Invoke(materialisedNestedProjection);

            return assignment.Update(modifiedProjection);
        }

        private static Expression GetMaterialisedNestedProjection(Expression nestedProjection)
        {
            var elementType = nestedProjection.Type.GetEnumerableElementType();

            return nestedProjection.Type.IsArray
                ? nestedProjection.WithToArrayLinqCall(elementType)
                : nestedProjection.WithToListLinqCall(elementType);
        }
    }
}