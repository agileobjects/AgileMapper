namespace AgileObjects.AgileMapper.UnitTests.MapperCloning
{
    using System.Collections.Generic;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenCloningDictionarySettings
    {
        [Fact]
        public void ShouldCloneFullAndMemberKeys()
        {
            using (var baseMapper = Mapper.CreateNew())
            {
                baseMapper.WhenMapping
                    .FromDictionariesWithValueType<object>().To<Product>()
                    .MapFullKey("BlahBlah").To(p => p.ProductId)
                    .And
                    .MapMemberNameKey("ProductPrice").To(p => p.Price);

                using (var clonedMapper = baseMapper.CloneSelf())
                {
                    var source = new Dictionary<string, object>
                    {
                        ["BlahBlah"] = "DictionaryAdventures.co.uk",
                        ["ProductPrice"] = 12.00
                    };
                    var result = clonedMapper.Map(source).ToANew<Product>();

                    result.ProductId.ShouldBe("DictionaryAdventures.co.uk");
                    result.Price.ShouldBe(12.00);
                }
            }
        }
    }
}
