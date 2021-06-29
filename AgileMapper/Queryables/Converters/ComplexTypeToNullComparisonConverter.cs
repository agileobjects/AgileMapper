namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using Members.Extensions;
    using ReadableExpressions.Extensions;

    internal static class ComplexTypeToNullComparisonConverter
    {
        public static bool TryConvert(
            BinaryExpression comparison,
            IQueryProjectionModifier modifier,
            out Expression converted)
        {
            if (modifier.Settings.SupportsComplexTypeToNullComparison ||
               (comparison.Left.Type == typeof(object)) ||
               !comparison.Left.Type.IsComplex())
            {
                converted = null;
                return false;
            }

            converted = Convert(comparison, modifier);
            return converted != null;
        }

        private static Expression Convert(BinaryExpression comparison, IQueryProjectionModifier modifier)
        {
            if (!modifier.MapperData.IsEntity(comparison.Left.Type, out var idMember))
            {
                return null;
            }

            var entityMemberAccess = comparison.Left;
            var entityParentAccess = entityMemberAccess.GetParentOrNull();

            if (TryGetEntityMemberIdMemberOrNull(
                    entityParentAccess,
                    entityMemberAccess,
                    idMember,
                    out var entityMemberIdMember))
            {
                return entityMemberIdMember
                    .GetAccess(entityParentAccess)
                    .GetIsNotDefaultComparison();
            }

            return null;
        }

        private static bool TryGetEntityMemberIdMemberOrNull(
            Expression entityParentAccess,
            Expression entityMemberAccess,
            Member entityIdMember,
            out Member entityMemberIdMember)
        {
            var sourceMembers = GlobalContext
                .Instance
                .MemberCache
                .GetSourceMembers(entityParentAccess.Type)
                .FilterToArray(m => m.IsSimple);

            var entityMemberName = entityMemberAccess.GetMemberName();

            return
                TryGetEntityMemberIdMember(
                    entityMemberName,
                    entityIdMember.Name,
                    sourceMembers,
                    out entityMemberIdMember) ||
                TryGetEntityMemberIdMember(
                    entityIdMember,
                    entityMemberName,
                    "Id",
                    sourceMembers,
                    out entityMemberIdMember) ||
                TryGetEntityMemberIdMember(
                    entityIdMember,
                    entityMemberName,
                    "Identifier",
                    sourceMembers,
                    out entityMemberIdMember);
        }

        private static bool TryGetEntityMemberIdMember(
            Member entityIdMember,
            string entityMemberName,
            string idMemberName,
            IList<Member> sourceMembers,
            out Member entityMemberIdMember)
        {
            if (entityIdMember.Name.EqualsIgnoreCase(idMemberName))
            {
                entityMemberIdMember = null;
                return false;
            }

            return TryGetEntityMemberIdMember(
                entityMemberName,
                idMemberName,
                sourceMembers,
                out entityMemberIdMember);
        }

        private static bool TryGetEntityMemberIdMember(
            string entityMemberName,
            string idMemberName,
            IList<Member> sourceMembers,
            out Member entityMemberIdMember)
        {
            idMemberName = entityMemberName + idMemberName;

            entityMemberIdMember = sourceMembers
                .FirstOrDefault(idMemberName, (idmn, m) => m.Name.EqualsIgnoreCase(idmn));

            return entityMemberIdMember != null;
        }
    }
}