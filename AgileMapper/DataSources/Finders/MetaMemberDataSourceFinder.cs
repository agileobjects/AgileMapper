namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ObjectPopulation.Enumerables;
    using TypeConversion;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using static System.StringComparison;

    internal struct MetaMemberDataSourceFinder : IDataSourceFinder
    {
        public IEnumerable<IDataSource> FindFor(DataSourceFindContext context)
        {
            if (TryGetMetaMemberNameParts(context, out var memberNameParts) &&
                TryGetMetaMember(memberNameParts, context, out var metaMember))
            {
                var dataSource = metaMember.GetDataSource();

                yield return context.GetFinalDataSource(dataSource);

                if (dataSource.IsConditional)
                {
                    yield return context.GetFallbackDataSource();
                }
            }
        }

        private static bool TryGetMetaMemberNameParts(
            DataSourceFindContext context,
            out IList<string> memberNameParts)
        {
            memberNameParts = default(IList<string>);

            var targetMemberName = context.MapperData.TargetMember.Name;
            var previousNamePartEndIndex = targetMemberName.Length;
            var currentMemberName = string.Empty;
            var noMetaMemberAdded = true;

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
                        return false;
                    }

                    memberNameParts = new List<string>();
                }

                var memberNamePart = targetMemberName.Substring(i, previousNamePartEndIndex - i);

                switch (memberNamePart)
                {
                    case HasMetaMemberPart.Name:
                    case FirstMetaMemberPart.Name:
                    case LastMetaMemberPart.Name:
                    case CountMetaMemberPart.Name:
                        if (currentMemberName.Length != 0)
                        {
                            memberNameParts.Add(GetFinalMemberName(currentMemberName, memberNamePart));
                            currentMemberName = string.Empty;
                        }

                        memberNameParts.Add(memberNamePart);
                        noMetaMemberAdded = false;
                        break;

                    default:
                        currentMemberName = memberNamePart + currentMemberName;

                        if (currentMemberName.StartsWith(NumberOfMetaMemberPart.Name, Ordinal))
                        {
                            currentMemberName = currentMemberName.Substring(NumberOfMetaMemberPart.Name.Length);
                            memberNameParts.Add(currentMemberName);
                            memberNameParts.Add(NumberOfMetaMemberPart.Name);
                            currentMemberName = string.Empty;
                            noMetaMemberAdded = false;
                        }
                        break;
                }

                previousNamePartEndIndex = i;
            }

            if (noMetaMemberAdded)
            {
                return false;
            }

            if (currentMemberName.Length == 0)
            {
                return memberNameParts.Any();
            }

            memberNameParts.Add(GetFinalMemberName(
                currentMemberName,
                memberNameParts[memberNameParts.Count - 1]));

            return memberNameParts.Any();
        }

        private static string GetFinalMemberName(string memberName, string previousNamePart)
        {
            switch (previousNamePart)
            {
                case FirstMetaMemberPart.Name:
                case LastMetaMemberPart.Name:
                case CountMetaMemberPart.Name:
                    return memberName.Pluralise();

                default:
                    return memberName;
            }
        }

        private static bool TryGetMetaMember(
            IList<string> memberNameParts,
            DataSourceFindContext context,
            out MetaMemberPartBase metaMember)
        {
            var currentMappingData = default(IObjectMappingData);
            var currentSourceMember = default(IQualifiedMember);
            var currentTargetMember = default(QualifiedMember);
            var currentMemberPart = metaMember = default(MetaMemberPartBase);

            Func<IQualifiedMember, QualifiedMember, IObjectMappingData, DataSourceFindContext, IObjectMappingData> currentMappingDataFactory =
                (sm, tm, md, c) => c.ChildMappingData.Parent;

            for (var i = memberNameParts.Count - 1; i >= 0; --i)
            {
                var memberNamePart = memberNameParts[i];

                switch (memberNamePart)
                {
                    case HasMetaMemberPart.Name:
                        if (HasMetaMemberPart.TryCreateFor(context.MapperData, ref currentMemberPart))
                        {
                            break;
                        }

                        return false;

                    case FirstMetaMemberPart.Name:
                        currentMemberPart = new FirstMetaMemberPart(context.MapperData);
                        break;

                    case LastMetaMemberPart.Name:
                        currentMemberPart = new LastMetaMemberPart(context.MapperData);
                        break;

                    case CountMetaMemberPart.Name:
                        if (CountMetaMemberPart.TryCreateFor(context.MapperData, ref currentMemberPart))
                        {
                            break;
                        }

                        return false;

                    case NumberOfMetaMemberPart.Name:
                        if (NumberOfMetaMemberPart.TryCreateFor(context.MapperData, ref currentMemberPart))
                        {
                            break;
                        }

                        return false;

                    default:
                        currentMappingData = currentMappingDataFactory.Invoke(
                            currentSourceMember,
                            currentTargetMember,
                            currentMappingData,
                            context);

                        var currentMapperData = currentMappingData.MapperData;

                        var matchingTargetMember = GlobalContext.Instance
                            .MemberCache
                            .GetTargetMembers(currentMapperData.TargetType)
                            .FirstOrDefault(m => m.Name == memberNamePart);

                        if (matchingTargetMember == null)
                        {
                            matchingTargetMember = GlobalContext.Instance
                                .MemberCache
                                .GetSourceMembers(currentMapperData.SourceType)
                                .FirstOrDefault(m => m.Name == memberNamePart);

                            if (matchingTargetMember == null)
                            {
                                return false;
                            }
                        }

                        currentTargetMember = currentMapperData.TargetMember.Append(matchingTargetMember);

                        var childMemberMapperData = new ChildMemberMapperData(currentTargetMember, currentMapperData);

                        var memberMappingData = currentMappingData.GetChildMappingData(childMemberMapperData);

                        currentSourceMember = SourceMemberMatcher.GetMatchFor(
                            memberMappingData,
                            out _,
                            searchParentContexts: false);

                        if (currentSourceMember == null)
                        {
                            return false;
                        }

                        currentMemberPart = new SourceMemberMetaMemberPart(
                            currentSourceMember,
                            currentMapperData,
                            isRootMemberPart: currentMemberPart == null);

                        currentMappingDataFactory = (sm, tm, md, c) =>
                        {
                            var mappingData = ObjectMappingDataFactory.ForChild(sm, tm, 0, md);

                            return sm.IsEnumerable
                                ? ObjectMappingDataFactory.ForElement(mappingData)
                                : ObjectMappingDataFactory.ForChild(sm, tm, 0, md);
                        };

                        break;
                }

                if (metaMember == null)
                {
                    metaMember = currentMemberPart;
                    continue;
                }

                if (!metaMember.TrySetNextPart(currentMemberPart))
                {
                    return false;
                }
            }

            return true;
        }

        #region MetaMemberPart classes

        private abstract class MetaMemberPartBase
        {
            protected MetaMemberPartBase(IMemberMapperData mapperData)
            {
                MapperData = mapperData;
            }

            protected IMemberMapperData MapperData { get; }

            public virtual IQualifiedMember SourceMember => MapperData.SourceMember;

            public MetaMemberPartBase NextPart { get; private set; }

            public bool TrySetNextPart(MetaMemberPartBase nextPart)
            {
                if (NextPart != null)
                {
                    return NextPart.TrySetNextPart(nextPart);
                }

                if (IsInvalid(nextPart))
                {
                    return false;
                }

                SetNextPart(nextPart);
                return true;
            }

            protected virtual void SetNextPart(MetaMemberPartBase nextPart) => NextPart = nextPart;

            public abstract bool IsInvalid(MetaMemberPartBase nextPart);

            public IDataSource GetDataSource()
            {
                var metaMemberAccess = GetAccess(MapperData.SourceObject);
                var condition = GetConditionOrNull();

                return (condition != null)
                    ? new AdHocDataSource(SourceMember, metaMemberAccess, condition)
                    : new AdHocDataSource(SourceMember, metaMemberAccess, MapperData);
            }

            public abstract Expression GetAccess(Expression parentInstance);

            public virtual Expression GetConditionOrNull() => null;

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
            public const string Name = "Has";

            private HasMetaMemberPart(IMemberMapperData mapperData)
                : base(mapperData)
            {
            }

            public static bool TryCreateFor(IMemberMapperData mapperData, ref MetaMemberPartBase metaMemberPart)
            {
                if (!mapperData.TargetMember.IsSimple)
                {
                    return false;
                }

                if (!mapperData.CanConvert(typeof(bool), mapperData.TargetMember.Type))
                {
                    return false;
                }

                metaMemberPart = new HasMetaMemberPart(mapperData);
                return true;
            }

            private IQualifiedMember QueriedMember => NextPart.SourceMember;

            public override bool IsInvalid(MetaMemberPartBase nextPart) => false;

            public override Expression GetAccess(Expression parentInstance)
            {
                var queriedMemberAccess = NextPart.GetAccess(parentInstance);
                var hasCheck = GetHasCheck(queriedMemberAccess);

                return MapperData.GetValueConversion(hasCheck, MapperData.TargetMember.Type);
            }

            private Expression GetHasCheck(Expression queriedMemberAccess)
            {
                if (QueriedMember.IsEnumerable)
                {
                    return GetHasEnumerableCheck(queriedMemberAccess);
                }

                var queriedMemberNotDefault = queriedMemberAccess.GetIsNotDefaultComparison();

                if (QueriedMember.IsSimple)
                {
                    return queriedMemberNotDefault;
                }

                return queriedMemberNotDefault;
            }

            private static Expression GetHasEnumerableCheck(Expression enumerableAccess)
            {
                var helper = new EnumerableTypeHelper(enumerableAccess.Type);

                return helper.IsEnumerableInterface
                    ? GetLinqMethodCall(nameof(Enumerable.Any), enumerableAccess, helper)
                    : GetEnumerableCountCheck(enumerableAccess, helper);
            }

            public static Expression GetEnumerableCountCheck(Expression enumerableAccess, EnumerableTypeHelper helper)
            {
                var enumerableCount = helper.GetCountFor(enumerableAccess);
                var zero = ToNumericConverter<int>.Zero.GetConversionTo(enumerableCount.Type);
                var countGreaterThanZero = Expression.GreaterThan(enumerableCount, zero);

                return countGreaterThanZero;
            }
        }

        private abstract class EnumerablePositionMetaMemberPart : MetaMemberPartBase
        {
            private IQualifiedMember _sourceMember;
            private Expression _condition;

            protected EnumerablePositionMetaMemberPart(IMemberMapperData mapperData)
                : base(mapperData)
            {
            }

            public override IQualifiedMember SourceMember => _sourceMember;

            protected abstract string LinqSelectionMethodName { get; }

            protected abstract string LinqOrderingMethodName { get; }

            public override bool IsInvalid(MetaMemberPartBase nextPart)
            {
                if (!nextPart.SourceMember.IsEnumerable)
                {
                    return true;
                }

                var elementType = nextPart.SourceMember.ElementType;

                return elementType.IsSimple() &&
                      !MapperData.CanConvert(elementType, MapperData.TargetMember.Type);
            }

            protected override void SetNextPart(MetaMemberPartBase nextPart)
            {
                _sourceMember = nextPart.SourceMember.GetElementMember();

                base.SetNextPart(nextPart);
            }

            public override Expression GetAccess(Expression parentInstance)
            {
                var enumerableAccess = NextPart.GetAccess(parentInstance);

                var helper = new EnumerableTypeHelper(enumerableAccess.Type, _sourceMember.Type);

                _condition = GetCondition(enumerableAccess, helper);

                var valueAccess = (helper.HasListInterface && MapperData.RuleSet.Settings.AllowIndexAccesses)
                    ? enumerableAccess.GetIndexAccess(GetIndex(enumerableAccess))
                    : GetOrderedEnumerableAccess(enumerableAccess, helper);

                if (NextPart.NextPart != null)
                {
                    valueAccess = NextPart.NextPart.GetAccess(valueAccess);
                }

                if (MapperData.TargetMember.IsSimple)
                {
                    return MapperData.GetValueConversion(valueAccess, MapperData.TargetMember.Type);
                }

                return valueAccess;
            }

            private Expression GetCondition(Expression enumerableAccess, EnumerableTypeHelper helper)
            {
                var enumerableCheck = HasMetaMemberPart.GetEnumerableCountCheck(enumerableAccess, helper);

                if (MapperData.RuleSet.Settings.GuardAccessTo(enumerableAccess))
                {
                    enumerableCheck = Expression.AndAlso(
                        enumerableAccess.GetIsNotDefaultComparison(),
                        enumerableCheck);
                }

                return enumerableCheck;
            }

            public override Expression GetConditionOrNull() => _condition;

            private Expression GetOrderedEnumerableAccess(Expression enumerableAccess, EnumerableTypeHelper helper)
            {
                var elementType = _sourceMember.Type;

                if (!elementType.IsComplex())
                {
                    return GetLinqMethodCall(LinqSelectionMethodName, enumerableAccess, helper);
                }

                var orderMember =
                    elementType.GetPublicInstanceMember("Order") ??
                    elementType.GetPublicInstanceMember("DateCreated") ??
                    MapperData.MapperContext.Naming.GetIdentifierOrNull(elementType)?.MemberInfo;

                if (orderMember == null)
                {
                    return GetLinqMethodCall(LinqSelectionMethodName, enumerableAccess, helper);
                }

                var element = Parameters.Create(_sourceMember.Type);

                enumerableAccess = enumerableAccess.WithOrderingLinqCall(
                    LinqOrderingMethodName,
                    element,
                    Expression.MakeMemberAccess(element, orderMember));

                return GetLinqMethodCall(nameof(Enumerable.FirstOrDefault), enumerableAccess, helper);
            }

            protected abstract Expression GetIndex(Expression enumerableAccess);
        }

        private class FirstMetaMemberPart : EnumerablePositionMetaMemberPart
        {
            public const string Name = "First";

            public FirstMetaMemberPart(IMemberMapperData mapperData)
                : base(mapperData)
            {
            }

            protected override string LinqSelectionMethodName => nameof(Enumerable.FirstOrDefault);

            protected override string LinqOrderingMethodName => nameof(Enumerable.OrderBy);

            protected override Expression GetIndex(Expression enumerableAccess) => ToNumericConverter<int>.Zero;
        }

        private class LastMetaMemberPart : EnumerablePositionMetaMemberPart
        {
            public const string Name = "Last";

            public LastMetaMemberPart(IMemberMapperData mapperData)
                : base(mapperData)
            {
            }

            protected override string LinqSelectionMethodName => nameof(Enumerable.LastOrDefault);

            protected override string LinqOrderingMethodName => nameof(Enumerable.OrderByDescending);

            protected override Expression GetIndex(Expression enumerableAccess)
                => Expression.Subtract(enumerableAccess.GetCount(), ToNumericConverter<int>.One);
        }

        private abstract class CountMetaMemberPartBase : MetaMemberPartBase
        {
            protected CountMetaMemberPartBase(IMemberMapperData mapperData)
                : base(mapperData)
            {
            }

            protected static bool TryCreate<TPart>(
                IMemberMapperData mapperData,
                ref MetaMemberPartBase metaMemberPart,
                Func<IMemberMapperData, TPart> partFactory)
                where TPart : CountMetaMemberPartBase
            {
                if (!mapperData.TargetMember.IsSimple || !mapperData.TargetMember.Type.IsNumeric())
                {
                    return false;
                }

                metaMemberPart = partFactory.Invoke(mapperData);
                return true;
            }

            public override bool IsInvalid(MetaMemberPartBase nextPart)
                => !nextPart.SourceMember.IsEnumerable;

            public override Expression GetAccess(Expression enumerableAccess)
            {
                var helper = new EnumerableTypeHelper(enumerableAccess.Type);

                var count = helper.IsEnumerableInterface
                    ? GetLinqMethodCall(nameof(Enumerable.Count), enumerableAccess, helper)
                    : helper.GetCountFor(enumerableAccess, MapperData.TargetMember.Type.GetNonNullableType());

                return count.GetConversionTo(MapperData.TargetMember.Type);
            }
        }

        private class CountMetaMemberPart : CountMetaMemberPartBase
        {
            public const string Name = "Count";

            private CountMetaMemberPart(IMemberMapperData mapperData)
                : base(mapperData)
            {
            }

            public static bool TryCreateFor(IMemberMapperData mapperData, ref MetaMemberPartBase metaMemberPart)
                => TryCreate(mapperData, ref metaMemberPart, md => new CountMetaMemberPart(md));
        }

        private class NumberOfMetaMemberPart : CountMetaMemberPartBase
        {
            public const string Name = "NumberOf";

            private NumberOfMetaMemberPart(IMemberMapperData mapperData)
                : base(mapperData)
            {
            }

            public static bool TryCreateFor(IMemberMapperData mapperData, ref MetaMemberPartBase metaMemberPart)
                => TryCreate(mapperData, ref metaMemberPart, md => new NumberOfMetaMemberPart(md));

            public override Expression GetAccess(Expression parentAccess)
            {
                var enumerableAccess = NextPart.GetAccess(parentAccess);

                return base.GetAccess(enumerableAccess);
            }
        }

        private class SourceMemberMetaMemberPart : MetaMemberPartBase
        {
            private readonly bool _isRootMemberPart;

            public SourceMemberMetaMemberPart(
                IQualifiedMember sourceMember,
                IMemberMapperData parentMapperData,
                bool isRootMemberPart)
                : base(parentMapperData)
            {
                _isRootMemberPart = isRootMemberPart;
                SourceMember = sourceMember.RelativeTo(parentMapperData.SourceMember);
            }

            public override IQualifiedMember SourceMember { get; }

            public override bool IsInvalid(MetaMemberPartBase nextPart) => nextPart.IsInvalid(this);

            public override Expression GetAccess(Expression parentInstance)
            {
                var memberAccess = SourceMember.GetQualifiedAccess(parentInstance);

                return _isRootMemberPart
                    ? NextPart.GetAccess(memberAccess)
                    : memberAccess;
            }
        }

        #endregion
    }
}