namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;
    using Members;

    internal static class ConfiguredItemExtensions
    {
        public static TItem FindMatch<TItem>(this IList<TItem> items, IBasicMapperData mapperData)
            where TItem : UserConfiguredItemBase
        {
            return items?.FirstOrDefault(item => item.AppliesTo(mapperData));
        }

        public static IEnumerable<TItem> FindMatches<TItem>(this IEnumerable<TItem> items, IBasicMapperData mapperData)
            where TItem : UserConfiguredItemBase
        {
            return items?.Filter(item => item.AppliesTo(mapperData)).OrderBy(item => item) ?? Enumerable<TItem>.Empty;
        }
    }
}