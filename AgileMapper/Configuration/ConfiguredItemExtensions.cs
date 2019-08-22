﻿namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using Extensions.Internal;
    using Members;

    internal static class ConfiguredItemExtensions
    {
        public static TItem FindMatch<TItem>(this IList<TItem> items, IBasicMapperData mapperData)
            where TItem : UserConfiguredItemBase
        {
            return items?.FirstOrDefault(mapperData, (md, item) => item.AppliesTo(md));
        }

        public static IList<TItem> FindRelevantMatches<TItem>(this IEnumerable<TItem> items, IBasicMapperData mapperData)
            where TItem : UserConfiguredItemBase
        {
            return items?.Filter(mapperData, (md, item) => item.CouldApplyTo(md)).ToArray() ?? Enumerable<TItem>.EmptyArray;
        }

        public static IEnumerable<TItem> FindMatches<TItem>(this IEnumerable<TItem> items, IBasicMapperData mapperData)
            where TItem : UserConfiguredItemBase
        {
            return items?.Filter(mapperData, (md, item) => item.AppliesTo(md)) ?? Enumerable<TItem>.Empty;
        }
    }
}