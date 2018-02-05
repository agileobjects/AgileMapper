namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;
    using ReadableExpressions.Extensions;

    internal static class ComplexTypeToNullComparisonConverter
    {
        public static bool TryConvert(
            BinaryExpression comparison,
            IQueryProjectionModifier context,
            out Expression converted)
        {
            if (context.Settings.SupportsComplexTypeToNullComparison ||
               (comparison.Left.Type == typeof(object)) ||
               !comparison.Left.Type.IsComplex())
            {
                converted = null;
                return false;
            }

            converted = Convert(comparison, context);
            return true;
        }

        private static Expression Convert(BinaryExpression comparison, IQueryProjectionModifier context)
        {
            if (!context.MapperData.IsEntity(comparison.Left.Type, out var idMember))
            {
                return true.ToConstantExpression();
            }

            var entityMemberAccess = comparison.Left;
            var entityParentAccess = entityMemberAccess.GetParentOrNull();
            var entityMemberIdMember = GetEntityMemberIdMemberOrNull(entityParentAccess, entityMemberAccess, idMember);

            if (entityMemberIdMember == null)
            {
                return true.ToConstantExpression();
            }

            var entityMemberIdMemberAccess = entityMemberIdMember.GetAccess(entityParentAccess);

            return entityMemberIdMemberAccess.GetIsNotDefaultComparison();
        }

        private static Member GetEntityMemberIdMemberOrNull(
            Expression entityParentAccess,
            Expression entityMemberAccess,
            Member entityIdMember)
        {
            var sourceMembers = GlobalContext
                .Instance
                .MemberCache
                .GetSourceMembers(entityParentAccess.Type)
                .Where(m => m.IsSimple)
                .ToArray();

            var entityMemberName = entityMemberAccess.GetMemberName();

            if (TryGetEntityMemberIdMember(entityMemberName + entityIdMember.Name, sourceMembers, out var member))
            {
                return member;
            }

            if (!entityIdMember.Name.EqualsIgnoreCase("Id") &&
                TryGetEntityMemberIdMember(entityMemberName + "Id", sourceMembers, out member))
            {
                return member;
            }

            if (!entityIdMember.Name.EqualsIgnoreCase("Identifer") &&
                TryGetEntityMemberIdMember(entityMemberName + "Identifer", sourceMembers, out member))
            {
                return member;
            }

            return null;
        }

        private static bool TryGetEntityMemberIdMember(
            string idMemberName,
            IEnumerable<Member> sourceMembers,
            out Member member)
        {
            member = sourceMembers.FirstOrDefault(m => m.Name.EqualsIgnoreCase(idMemberName));
            return member != null;
        }
    }
}