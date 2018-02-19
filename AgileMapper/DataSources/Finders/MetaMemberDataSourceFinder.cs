namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation.Enumerables;

    internal class MetaMemberDataSourceFinder : IDataSourceFinder
    {
        public IEnumerable<IDataSource> FindFor(DataSourceFindContext context)
        {
            var targetMemberName = context.MapperData.TargetMember.Name;
            var memberNameParts = default(List<string>);
            var previousNamePartEndIndex = targetMemberName.Length;

            for (var i = previousNamePartEndIndex - 1; i >= 0; --i)
            {
                if (!char.IsUpper(targetMemberName[i]))
                {
                    continue;
                }

                if (memberNameParts == null)
                {
                    if (i == 0)
                    {
                        yield break;
                    }

                    memberNameParts = new List<string>();
                }

                var memberNamePart = targetMemberName.Substring(i, previousNamePartEndIndex - i);

                memberNameParts.Add(memberNamePart);
                previousNamePartEndIndex = i;
            }

            if ((memberNameParts == null) || memberNameParts.None())
            {
                yield break;
            }

            if (!TryGetMetaMemberParts(memberNameParts, context, out var metaMember))
            {
                yield break;
            }

            var metaMemberAccess = metaMember.GetAccess(context.MapperData.SourceObject);
            var mappingDataSource = new AdHocDataSource(metaMember.SourceMember, metaMemberAccess, metaMember.MapperData);

            yield return context.GetFinalDataSource(mappingDataSource);
        }

        private static bool TryGetMetaMemberParts(
            IList<string> memberNameParts,
            DataSourceFindContext context,
            out IMetaMemberPart metaMember)
        {
            // Has<MemberName>   -> Enumerable?.Any() or ComplexType != null or Simple != default
            // First<MemberName> -> Enumerable?.First()
            // Last<MemberName>  -> Enumerable?.Last()

            // <MemberName>Has<SubMemberName>      -> <MemberName>Has -> as above - <MemberName> must be ComplexType
            // First<MemberName>Has<SubMemberName> -> 
            // Last<MemberName>Has<SubMemberName>  -> 

            // Target.AddressHasLine1 -> Target.Address.Line1
            // Build target QualifiedMember:
            //  1. Find matchingTargetMember
            //  2. MapperData.Parent.TargetMember.Append(matchingTargetMember)
            //  3. Find matchingSourceMember
            //  4. Build check

            metaMember = default(IMetaMemberPart);

            for (var i = 0; i < memberNameParts.Count; ++i)
            {
                var memberNamePart = memberNameParts[i];

                switch (memberNamePart)
                {
                    case "Has":
                        if (HasMetaMemberPart.TryCreateFor(ref metaMember))
                        {
                            continue;
                        }

                        return false;

                    case "First":
                        if (FirstMetaMemberPart.TryCreateFor(ref metaMember))
                        {
                            continue;
                        }

                        return false;

                    case "Last":
                        if (LastMetaMemberPart.TryCreateFor(ref metaMember))
                        {
                            continue;
                        }

                        return false;

                    default:
                        if (SourceMemberMetaMemberPart.TryCreateFor(memberNamePart, ref metaMember, context))
                        {
                            continue;
                        }

                        return false;
                }
            }

            return true;
        }

        private interface IMetaMemberPart
        {
            IQualifiedMember SourceMember { get; }

            IMemberMapperData MapperData { get; }

            Expression GetAccess(Expression parentInstance);
        }

        private class HasMetaMemberPart : IMetaMemberPart
        {
            private readonly IMetaMemberPart _queried;

            private HasMetaMemberPart(IMetaMemberPart queried)
            {
                _queried = queried;
            }

            public static bool TryCreateFor(ref IMetaMemberPart metaMemberPart)
            {
                if (metaMemberPart == null)
                {
                    return false;
                }

                metaMemberPart = new HasMetaMemberPart(metaMemberPart);
                return true;
            }

            public IQualifiedMember SourceMember => _queried.SourceMember;

            public IMemberMapperData MapperData => _queried.MapperData;

            public Expression GetAccess(Expression parentInstance)
            {
                var queriedMemberAccess = _queried.GetAccess(parentInstance);

                if (_queried.SourceMember.IsEnumerable)
                {
                    return GetHasEnumerableCheck(queriedMemberAccess);
                }

                var queriedMemberNotDefault = queriedMemberAccess.GetIsNotDefaultComparison();

                if (_queried.SourceMember.IsSimple)
                {
                    return queriedMemberNotDefault;
                }

                return queriedMemberNotDefault;
            }

            private static Expression GetHasEnumerableCheck(Expression enumerableAccess)
            {
                var helper = new EnumerableTypeHelper(enumerableAccess.Type);

                if (helper.IsEnumerableInterface)
                {
                    return Expression.Call(
                        typeof(Enumerable)
                            .GetPublicStaticMethod("Any", parameterCount: 1)
                            .MakeGenericMethod(helper.ElementType),
                        enumerableAccess);
                }

                var enumerableCount = helper.GetCountFor(enumerableAccess);
                var zero = 0.ToConstantExpression(enumerableCount.Type);
                var countGreaterThanZero = Expression.GreaterThan(enumerableCount, zero);

                return countGreaterThanZero;
            }
        }

        private class FirstMetaMemberPart : IMetaMemberPart
        {
            private readonly IMetaMemberPart _enumerable;

            private FirstMetaMemberPart(IMetaMemberPart enumerable)
            {
                _enumerable = enumerable;
                SourceMember = enumerable.SourceMember.GetElementMember();
            }

            public static bool TryCreateFor(ref IMetaMemberPart metaMemberPart)
            {
                if (metaMemberPart == null)
                {
                    return false;
                }

                metaMemberPart = new FirstMetaMemberPart(metaMemberPart);
                return true;
            }

            public IQualifiedMember SourceMember { get; }

            public IMemberMapperData MapperData => _enumerable.MapperData;

            public Expression GetAccess(Expression parentInstance)
            {
                var enumerableAccess = _enumerable.GetAccess(parentInstance);

                var helper = new EnumerableTypeHelper(enumerableAccess.Type);

                if (helper.HasListInterface)
                {
                    return enumerableAccess.GetIndexAccess(0.ToConstantExpression());
                }

                return null;
            }
        }

        private class LastMetaMemberPart : IMetaMemberPart
        {
            private readonly IMetaMemberPart _enumerable;

            private LastMetaMemberPart(IMetaMemberPart enumerable)
            {
                _enumerable = enumerable;
            }

            public static bool TryCreateFor(ref IMetaMemberPart metaMemberPart)
            {
                if (metaMemberPart == null)
                {
                    return false;
                }

                metaMemberPart = new LastMetaMemberPart(metaMemberPart);
                return true;
            }

            public IQualifiedMember SourceMember => _enumerable.SourceMember;

            public IMemberMapperData MapperData => _enumerable.MapperData;

            public Expression GetAccess(Expression parentInstance)
            {
                return null;
            }
        }

        private class SourceMemberMetaMemberPart : IMetaMemberPart
        {
            private readonly IMetaMemberPart _nextPart;

            private SourceMemberMetaMemberPart(
                IQualifiedMember sourceMember,
                IMetaMemberPart nextPart,
                IMemberMapperData mapperData)
            {
                _nextPart = nextPart;

                if (nextPart != null)
                {
                    SourceMember = nextPart.SourceMember;
                    MapperData = nextPart.MapperData;
                }
                else
                {
                    SourceMember = sourceMember;
                    MapperData = mapperData;
                }
            }

            public static bool TryCreateFor(
                string memberNamePart,
                ref IMetaMemberPart metaMemberPart,
                DataSourceFindContext context)
            {
                var matchingTargetMember = GlobalContext.Instance
                    .MemberCache
                    .GetTargetMembers(context.MapperData.TargetType)
                    .FirstOrDefault(m => m.Name == memberNamePart);

                if (matchingTargetMember == null)
                {
                    return false;
                }

                var childMemberMapperData = new ChildMemberMapperData(
                    context.MapperData.Parent.TargetMember.Append(matchingTargetMember),
                    context.MapperData.Parent);

                var memberMappingData = context.ChildMappingData.Parent.GetChildMappingData(childMemberMapperData);

                var matchingSourceMember = SourceMemberMatcher.GetMatchFor(
                    memberMappingData,
                    out var _,
                    searchParentContexts: false);

                if (matchingSourceMember == null)
                {
                    return false;
                }

                metaMemberPart = new SourceMemberMetaMemberPart(
                    matchingSourceMember,
                    metaMemberPart,
                    context.MapperData);

                return true;
            }

            public IQualifiedMember SourceMember { get; }

            public IMemberMapperData MapperData { get; }

            public Expression GetAccess(Expression parentInstance)
            {
                var memberAccess = SourceMember.GetQualifiedAccess(parentInstance);

                return (_nextPart != null)
                    ? _nextPart.GetAccess(memberAccess)
                    : memberAccess;
            }
        }
    }
}