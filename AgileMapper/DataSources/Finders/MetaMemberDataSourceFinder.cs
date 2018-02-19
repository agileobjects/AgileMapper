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

            if (TryGetMetaMemberParts(memberNameParts, context, out var metaMember))
            {
                yield return metaMember.GetDataSource(context);
            }
        }

        private static bool TryGetMetaMemberParts(
            IList<string> memberNameParts,
            DataSourceFindContext context,
            out MetaMemberPartBase metaMember)
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

            metaMember = default(MetaMemberPartBase);

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

        private abstract class MetaMemberPartBase
        {
            protected MetaMemberPartBase(
                IQualifiedMember sourceMember,
                IMemberMapperData mapperData)
            {
                SourceMember = sourceMember;
                MapperData = mapperData;
            }

            public IQualifiedMember SourceMember { get; }

            public IMemberMapperData MapperData { get; }

            public IDataSource GetDataSource(DataSourceFindContext context)
            {
                var metaMemberAccess = GetAccess(MapperData.SourceObject);
                var mappingDataSource = new AdHocDataSource(SourceMember, metaMemberAccess, MapperData);

                return context.GetFinalDataSource(mappingDataSource);
            }

            public abstract Expression GetAccess(Expression parentInstance);

            protected static Expression GetLinqMethodCall(
                string methodName,
                Expression enumerable,
                EnumerableTypeHelper helper)
            {
                return Expression.Call(
                    typeof(Enumerable)
                        .GetPublicStaticMethod(methodName, parameterCount: 1)
                        .MakeGenericMethod(helper.ElementType),
                    enumerable);
            }
        }

        private class HasMetaMemberPart : MetaMemberPartBase
        {
            private readonly MetaMemberPartBase _queried;

            private HasMetaMemberPart(MetaMemberPartBase queried)
                : base(queried.SourceMember, queried.MapperData)
            {
                _queried = queried;
            }

            public static bool TryCreateFor(ref MetaMemberPartBase metaMemberPart)
            {
                if (metaMemberPart == null)
                {
                    return false;
                }

                metaMemberPart = new HasMetaMemberPart(metaMemberPart);
                return true;
            }

            public override Expression GetAccess(Expression parentInstance)
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
                    return GetLinqMethodCall(nameof(Enumerable.Any), enumerableAccess, helper);
                }

                var enumerableCount = helper.GetCountFor(enumerableAccess);
                var zero = 0.ToConstantExpression(enumerableCount.Type);
                var countGreaterThanZero = Expression.GreaterThan(enumerableCount, zero);

                return countGreaterThanZero;
            }
        }

        private class FirstMetaMemberPart : MetaMemberPartBase
        {
            private readonly MetaMemberPartBase _enumerable;

            private FirstMetaMemberPart(MetaMemberPartBase enumerable)
                : base(enumerable.SourceMember.GetElementMember(), enumerable.MapperData)
            {
                _enumerable = enumerable;
            }

            public static bool TryCreateFor(ref MetaMemberPartBase metaMemberPart)
            {
                if (metaMemberPart == null)
                {
                    return false;
                }

                metaMemberPart = new FirstMetaMemberPart(metaMemberPart);
                return true;
            }

            public override Expression GetAccess(Expression parentInstance)
            {
                var enumerableAccess = _enumerable.GetAccess(parentInstance);

                var helper = new EnumerableTypeHelper(enumerableAccess.Type);

                if (helper.HasListInterface)
                {
                    return enumerableAccess.GetIndexAccess(0.ToConstantExpression());
                }

                return GetLinqMethodCall(nameof(Enumerable.First), enumerableAccess, helper);
            }
        }

        private class LastMetaMemberPart : MetaMemberPartBase
        {
            private readonly MetaMemberPartBase _enumerable;

            private LastMetaMemberPart(MetaMemberPartBase enumerable)
                : base(enumerable.SourceMember.GetElementMember(), enumerable.MapperData)
            {
                _enumerable = enumerable;
            }

            public static bool TryCreateFor(ref MetaMemberPartBase metaMemberPart)
            {
                if (metaMemberPart == null)
                {
                    return false;
                }

                metaMemberPart = new LastMetaMemberPart(metaMemberPart);
                return true;
            }

            public override Expression GetAccess(Expression parentInstance)
            {
                return null;
            }
        }

        private class SourceMemberMetaMemberPart : MetaMemberPartBase
        {
            private readonly MetaMemberPartBase _nextPart;

            private SourceMemberMetaMemberPart(
                IQualifiedMember sourceMember,
                MetaMemberPartBase nextPart,
                IMemberMapperData mapperData)
                : base(nextPart?.SourceMember ?? sourceMember, nextPart?.MapperData ?? mapperData)
            {
                _nextPart = nextPart;
            }

            public static bool TryCreateFor(
                string memberNamePart,
                ref MetaMemberPartBase metaMemberPart,
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

            public override Expression GetAccess(Expression parentInstance)
            {
                var memberAccess = SourceMember.GetQualifiedAccess(parentInstance);

                return (_nextPart != null)
                    ? _nextPart.GetAccess(memberAccess)
                    : memberAccess;
            }
        }
    }
}