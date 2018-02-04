namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;
    using ReadableExpressions.Extensions;
    using Settings;

    internal static class ComplexTypeToNullComparisonConverter
    {
        public static bool TryConvert(
            BinaryExpression comparison,
            IQueryProviderSettings settings,
            IMemberMapperData mapperData,
            out Expression converted)
        {
            if (settings.SupportsComplexTypeToNullComparison || !comparison.Right.Type.IsComplex())
            {
                converted = null;
                return false;
            }

            converted = Convert(comparison, mapperData);
            return true;
        }

        private static Expression Convert(BinaryExpression comparison, IMemberMapperData mapperData)
        {
            if (!mapperData.IsEntity(comparison.Left.Type, out var idMember))
            {
                return true.ToConstantExpression();
            }

            var entityMemberAccess = comparison.Left;
            var entityAccess = entityMemberAccess.GetParentOrNull();
            var entityMemberIdMember = GetEntityMemberIdMemberOrNull(entityAccess, entityMemberAccess, idMember);

            if (entityMemberIdMember == null)
            {
                return true.ToConstantExpression();
            }

            var entityMemberIdMemberAccess = entityMemberIdMember.GetAccess(entityAccess);

            return entityMemberIdMemberAccess.GetIsNotDefaultComparison();
        }

        private static Member GetEntityMemberIdMemberOrNull(
            Expression entityAccess,
            Expression entityMemberAccess,
            Member entityIdMember)
        {
            var sourceMembers = GlobalContext
                .Instance
                .MemberCache
                .GetSourceMembers(entityAccess.Type)
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