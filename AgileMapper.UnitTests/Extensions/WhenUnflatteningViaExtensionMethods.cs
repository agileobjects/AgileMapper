namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using System.Collections.Generic;
    using AgileMapper.Extensions;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenUnflatteningViaExtensionMethods
    {
        [Fact]
        public void ShouldPopulateASimpleTypeMemberFromADictionary()
        {
            var source = new Dictionary<string, string> { ["Value"] = "Unflatten THIS" };
            var result = source.Unflatten().To<PublicProperty<string>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe("Unflatten THIS");
        }

        [Fact]
        public void ShouldPopulateASimpleTypeArrayMemberFromADictionaryWithASpecifiedMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionariesWithValueType<object>()
                    .To<PublicField<double[]>>()
                    .UseElementKeyPattern("_i");

                var source = new Dictionary<string, object>
                {
                    ["Value_0"] = 1L,
                    ["Value_1"] = 2L,
                    ["Value_2"] = 3L
                };
                var result = source.UnflattenUsing(mapper).To<PublicField<double[]>>();

                result.ShouldNotBeNull();
                result.Value.ShouldBe(1.0, 2.0, 3.0);
            }
        }
    }
}
