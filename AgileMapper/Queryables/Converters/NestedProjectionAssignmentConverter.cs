namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using Extensions.Internal;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class NestedProjectionAssignmentConverter
    {
        public static bool TryConvert(
            MemberAssignment assignment,
            IQueryProjectionModifier modifier,
            out MemberAssignment converted)
        {
            if (ShouldConvert(assignment, modifier))
            {
                converted = ConvertToMaterialisation(assignment, modifier);
                return true;
            }

            converted = null;
            return false;
        }

        private static bool ShouldConvert(
            MemberAssignment assignment,
            IQueryProjectionModifier modifier)
        {
            return modifier.Settings.SupportsEnumerableMaterialisation &&
                  (assignment.Expression.NodeType == ExpressionType.Call) &&
                 ((MethodCallExpression)assignment.Expression).IsLinqSelectCall();
        }

        private static MemberAssignment ConvertToMaterialisation(
            MemberAssignment assignment,
            IQueryProjectionModifier modifier)
        {
            var materialisedNestedProjection = GetMaterialisedNestedProjection(assignment.Expression);
            var modifiedProjection = modifier.Modify(materialisedNestedProjection);

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