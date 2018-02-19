namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation.Enumerables;
    using TypeConversion;

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
                        if (HasMetaMemberPart.TryCreateFor(ref metaMember, context))
                        {
                            continue;
                        }

                        return false;

                    case "First":
                        if (FirstMetaMemberPart.TryCreateFor(ref metaMember, context))
                        {
                            continue;
                        }

                        return false;

                    case "Last":
                        if (LastMetaMemberPart.TryCreateFor(ref metaMember, context))
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
                var metaMemberAccess = GetAccess(MapperData.SourceObject, MapperData);
                var mappingDataSource = new AdHocDataSource(SourceMember, metaMemberAccess, MapperData);

                return context.GetFinalDataSource(mappingDataSource);
            }

            public abstract Expression GetAccess(Expression parentInstance, IMemberMapperData mapperData);

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

            public static bool TryCreateFor(ref MetaMemberPartBase metaMemberPart, DataSourceFindContext context)
            {
                if (metaMemberPart == null)
                {
                    return false;
                }

                if (!context.MapperData.CanConvert(typeof(bool), context.MapperData.TargetMember.Type))
                {
                    return false;
                }

                metaMemberPart = new HasMetaMemberPart(metaMemberPart);
                return true;
            }

            public override Expression GetAccess(Expression parentInstance, IMemberMapperData mapperData)
            {
                var queriedMemberAccess = _queried.GetAccess(parentInstance, mapperData);

                var hasCheck = GetHasCheck(queriedMemberAccess);

                return mapperData.GetValueConversion(hasCheck, mapperData.TargetMember.Type);
            }

            private Expression GetHasCheck(Expression queriedMemberAccess)
            {
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

        private abstract class EnumerablePositionMetaMemberPart : MetaMemberPartBase
        {
            private readonly MetaMemberPartBase _enumerable;

            protected EnumerablePositionMetaMemberPart(MetaMemberPartBase enumerable)
                : base(enumerable.SourceMember.GetElementMember(), enumerable.MapperData)
            {
                _enumerable = enumerable;
            }

            protected static bool TryCreateFor<TPart>(
                ref MetaMemberPartBase metaMemberPart,
                Func<MetaMemberPartBase, TPart> partFactory,
                DataSourceFindContext context)
                where TPart : EnumerablePositionMetaMemberPart
            {
                if (metaMemberPart == null)
                {
                    return false;
                }

                var elementType = metaMemberPart
                    .SourceMember
                    .Type
                    .GetEnumerableElementType();

                if (elementType.IsSimple() &&
                   !context.MapperData.CanConvert(elementType, context.MapperData.TargetMember.Type))
                {
                    return false;
                }

                metaMemberPart = partFactory.Invoke(metaMemberPart);
                return true;
            }

            protected abstract string LinqMethodName { get; }

            public override Expression GetAccess(Expression parentInstance, IMemberMapperData mapperData)
            {
                var enumerableAccess = _enumerable.GetAccess(parentInstance, mapperData);

                var helper = new EnumerableTypeHelper(enumerableAccess.Type);

                var elementAccess = helper.HasListInterface
                    ? enumerableAccess.GetIndexAccess(GetIndex(enumerableAccess))
                    : GetLinqMethodCall(LinqMethodName, enumerableAccess, helper);

                if (mapperData.TargetMember.IsSimple)
                {
                    return mapperData.GetValueConversion(elementAccess, mapperData.TargetMember.Type);
                }

                return elementAccess;
            }

            protected abstract Expression GetIndex(Expression enumerableAccess);
        }

        private class FirstMetaMemberPart : EnumerablePositionMetaMemberPart
        {
            private FirstMetaMemberPart(MetaMemberPartBase enumerable)
                : base(enumerable)
            {
            }

            public static bool TryCreateFor(ref MetaMemberPartBase metaMemberPart, DataSourceFindContext context)
                => TryCreateFor(ref metaMemberPart, part => new FirstMetaMemberPart(part), context);

            protected override string LinqMethodName => nameof(Enumerable.First);

            protected override Expression GetIndex(Expression enumerableAccess) => ToNumericConverter<int>.Zero;
        }

        private class LastMetaMemberPart : EnumerablePositionMetaMemberPart
        {
            private LastMetaMemberPart(MetaMemberPartBase enumerable)
                : base(enumerable)
            {
            }

            public static bool TryCreateFor(ref MetaMemberPartBase metaMemberPart, DataSourceFindContext context)
                => TryCreateFor(ref metaMemberPart, part => new LastMetaMemberPart(part), context);

            protected override string LinqMethodName => nameof(Enumerable.Last);

            protected override Expression GetIndex(Expression enumerableAccess)
                => Expression.Subtract(enumerableAccess.GetCount(), ToNumericConverter<int>.One);
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

            public override Expression GetAccess(Expression parentInstance, IMemberMapperData mapperData)
            {
                var memberAccess = SourceMember.GetQualifiedAccess(parentInstance);

                return (_nextPart != null)
                    ? _nextPart.GetAccess(memberAccess, mapperData)
                    : memberAccess;
            }
        }
    }
}