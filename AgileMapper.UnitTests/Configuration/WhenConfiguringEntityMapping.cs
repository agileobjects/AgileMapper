namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringEntityMapping
    {
        [Fact]
        public void ShouldMapEntityKeys()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.MapEntityKeys();

                var source = new { Id = 456 };
                var result = mapper.Map(source).ToANew<ProductEntity>();

                result.Id.ShouldBe(456);
            }
        }
    }
}
