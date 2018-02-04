namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;
    using Extensions.Internal;

    internal static class NestedProjectionAssignmentConverter
    {
        public static bool TryConvert(
            MemberAssignment assignment,
            IQueryProjectionModifier modifier,
            out MemberAssignment converted)
        {
            if (modifier.Settings.SupportsEnumerableMaterialisation &&
               (assignment.Expression.NodeType == ExpressionType.Call) &&
                assignment.Expression.Type.IsEnumerable())
            {
                converted = ConvertToMaterialisation(assignment, modifier);
                return true;
            }

            converted = null;
            return false;
        }

        private static MemberAssignment ConvertToMaterialisation(
            MemberAssignment assignment,
            IQueryProjectionModifier context)
        {
            var materialisedNestedProjection = GetMaterialisedNestedProjection(assignment.Expression);
            var modifiedProjection = context.Modify(materialisedNestedProjection);

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