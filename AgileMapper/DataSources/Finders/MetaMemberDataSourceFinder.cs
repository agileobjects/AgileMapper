namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;
    using Members;

    internal class MetaMemberDataSourceFinder : IDataSourceFinder
    {
        public IEnumerable<IDataSource> FindFor(DataSourceFindContext context)
        {
            var targetMemberName = context.MapperData.TargetMember.Name;
            var memberNameParts = new List<string>();
            var previousNamePartEndIndex = targetMemberName.Length;

            for (var i = previousNamePartEndIndex - 1; i >= 0; --i)
            {
                if (char.IsUpper(targetMemberName[i]))
                {
                    if ((i == 0) && memberNameParts.None())
                    {
                        yield break;
                    }

                    memberNameParts.Insert(0, targetMemberName.Substring(i, previousNamePartEndIndex - i));
                    previousNamePartEndIndex = i;
                }
            }

            if (!TryGetMetaMemberParts(memberNameParts, context, out var metaMember))
            {
                yield break;
            }

            
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

            for (var i = memberNameParts.Count - 1; i >= 0; --i)
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
        }

        private class FirstMetaMemberPart : IMetaMemberPart
        {
            private readonly IMetaMemberPart _enumerable;

            private FirstMetaMemberPart(IMetaMemberPart enumerable)
            {
                _enumerable = enumerable;
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
        }

        private class SourceMemberMetaMemberPart : IMetaMemberPart
        {
            private readonly IQualifiedMember _sourceMember;
            private readonly IMetaMemberPart _parent;

            private SourceMemberMetaMemberPart(IQualifiedMember sourceMember, IMetaMemberPart parent)
            {
                _sourceMember = sourceMember;
                _parent = parent;
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

                metaMemberPart = new SourceMemberMetaMemberPart(matchingSourceMember, metaMemberPart);
                return true;
            }
        }
    }
}