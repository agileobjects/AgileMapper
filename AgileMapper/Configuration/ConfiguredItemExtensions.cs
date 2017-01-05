namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Members;

    internal static class ConfiguredItemExtensions
    {
        public static TItem FindMatch<TItem>(this IEnumerable<TItem> items, IBasicMapperData mapperData)
            where TItem : UserConfiguredItemBase
            => items.FirstOrDefault(im => im.AppliesTo(mapperData));
    }
}