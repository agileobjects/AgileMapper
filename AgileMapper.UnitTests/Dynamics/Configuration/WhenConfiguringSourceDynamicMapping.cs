namespace AgileObjects.AgileMapper.UnitTests.Dynamics.Configuration
{
    using System.Dynamic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringSourceDynamicMapping
    {
        [Fact]
        public void ShouldNotApplyDictionaryConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .DictionariesWithValueType<object>()
                    .To<PublicField<string>>()
                    .MapFullKey("LaLaLa")
                    .To(pf => pf.Value);

                dynamic source = new ExpandoObject();

                source.LaLaLa = 1;
                source.Value = 2;

                var result = (PublicField<string>)mapper.Map(source).ToANew<PublicField<string>>();

                result.ShouldNotBeNull();
                result.Value.ShouldBe("2");
            }
        }
    }
}
