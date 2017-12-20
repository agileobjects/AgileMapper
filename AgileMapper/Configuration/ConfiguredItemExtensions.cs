namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Members;

    internal static class ConfiguredItemExtensions
    {
        public static TItem FindMatch<TItem>(this IEnumerable<TItem> items, IBasicMapperData mapperData)
            where TItem : UserConfiguredItemBase
        {
            return items?.FirstOrDefault(item => item.AppliesTo(mapperData));
        }

        public static IEnumerable<TItem> FindMatches<TItem>(this IEnumerable<TItem> items, IBasicMapperData mapperData)
            where TItem : UserConfiguredItemBase
        {
            return items?.Where(item => item.AppliesTo(mapperData)).OrderBy(item => item) ?? Enumerable<TItem>.Empty;
        }
    }
}