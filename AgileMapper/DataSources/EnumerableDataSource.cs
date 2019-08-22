namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;
    using ObjectPopulation.Enumerables;

    internal class EnumerableDataSource : DataSourceBase
    {
        public EnumerableDataSource(
            IDataSource sourceEnumerableDataSource,
            int dataSourceIndex,
            IChildMemberMappingData enumerableMappingData)
            : base(
                  sourceEnumerableDataSource,
                  GetMapping(sourceEnumerableDataSource, dataSourceIndex, enumerableMappingData))
        {
        }

        private static Expression GetMapping(
            IDataSource sourceEnumerableDataSource,
            int dataSourceIndex,
            IChildMemberMappingData enumerableMappingData)
        {
            var sourceEnumerable = GetSourceEnumerable(sourceEnumerableDataSource, enumerableMappingData);
            var sourceMember = sourceEnumerableDataSource.SourceMember.WithType(sourceEnumerable.Type);

            var mapping = MappingFactory.GetChildMapping(
                sourceMember,
                sourceEnumerable,
                dataSourceIndex,
                enumerableMappingData);

            return mapping;
        }

        private static Expression GetSourceEnumerable(
            IDataSource sourceEnumerableDataSource,
            IChildMemberMappingData enumerableMappingData)
        {
            var mapperData = enumerableMappingData.MapperData;
            var sourceElementType = sourceEnumerableDataSource.SourceMember.ElementType;

            if (IsNotMappingFromLinkingType(sourceElementType, enumerableMappingData, out var forwardLink))
            {
                return sourceEnumerableDataSource.Value;
            }

            var linkParameter = Parameters.Create(sourceElementType);

            var orderedLinks = GetLinkOrdering(
                sourceEnumerableDataSource.Value,
                linkParameter,
                forwardLink,
                mapperData);

            var sourceEnumerable = GetForwardLinkSelection(
                orderedLinks,
                linkParameter,
                forwardLink,
                mapperData);

            return sourceEnumerable;
        }

        private static bool IsNotMappingFromLinkingType(
            Type sourceElementType,
            IChildMemberMappingData enumerableMappingData,
            out Member forwardLink)
        {
            var mapperData = enumerableMappingData.MapperData;

            if ((sourceElementType == mapperData.TargetMember.ElementType) ||
                !sourceElementType.IsComplex() ||
                (mapperData.GetIdentifierOrNull(sourceElementType) != null))
            {
                forwardLink = null;
                return true;
            }

            var sourceElementMembers = GlobalContext.Instance
                .MemberCache
                .GetSourceMembers(sourceElementType);

            var backLinkMember = sourceElementMembers
                .FirstOrDefault(m => m.IsComplex && m.Type == mapperData.SourceType);

            if (backLinkMember == null)
            {
                forwardLink = null;
                return true;
            }

            var otherComplexTypeMembers = sourceElementMembers
                .Filter(mapperData, (md, m) => m.IsComplex && (m.Type != md.SourceType))
                .ToArray();

            if ((otherComplexTypeMembers.Length != 1) ||
                (mapperData.GetIdentifierOrNull(otherComplexTypeMembers[0].Type) == null))
            {
                forwardLink = null;
                return true;
            }

            forwardLink = otherComplexTypeMembers[0];
            return false;
        }

        private static Expression GetForwardLinkSelection(Expression sourceEnumerable,
            ParameterExpression linkParameter,
            Member forwardLink,
            IMemberMapperData mapperData)
        {
            var funcTypes = new[] { linkParameter.Type, forwardLink.Type };
            var forwardLinkAccess = forwardLink.GetAccess(linkParameter);

            var forwardLinkLambda = Expression.Lambda(
                Expression.GetFuncType(funcTypes),
                forwardLinkAccess,
                linkParameter);

            return Expression.Call(
                EnumerablePopulationBuilder
                    .GetProjectionMethodFor(mapperData)
                    .MakeGenericMethod(funcTypes),
                sourceEnumerable,
                forwardLinkLambda);
        }

        private static Expression GetLinkOrdering(
            Expression sourceEnumerable,
            ParameterExpression linkParameter,
            Member forwardLink,
            IMemberMapperData mapperData)
        {
            var orderMember =
                mapperData.GetOrderMember(linkParameter.Type) ??
                mapperData.GetIdentifierOrNull(forwardLink.Type)?.MemberInfo;

            if (orderMember == null)
            {
                return sourceEnumerable;
            }

            var orderMemberAccess = Expression.MakeMemberAccess(
                (orderMember.DeclaringType != linkParameter.Type)
                    ? forwardLink.GetAccess(linkParameter)
                    : linkParameter,
                orderMember);

            return sourceEnumerable.WithOrderingLinqCall(
                nameof(Enumerable.OrderBy),
                linkParameter,
                orderMemberAccess);
        }
    }
}