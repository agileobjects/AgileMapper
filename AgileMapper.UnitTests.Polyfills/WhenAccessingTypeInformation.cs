namespace AgileObjects.AgileMapper.UnitTests.Polyfills
{
    using System.Linq;
    using Extensions;
    using Shouldly;
    using Xunit;

    public class WhenAccessingTypeInformation
    {
        [Fact]
        public void ShouldRetrieveAPublicInstanceField()
        {
            var fields = typeof(TestHelper).GetPublicInstanceFields();

            fields.ShouldNotBeNull();
            fields.ShouldHaveSingleItem();
            fields.First().Name.ShouldBe("PublicInstanceField");
        }

        [Fact]
        public void ShouldRetrieveAPublicInstanceProperty()
        {
            var properties = typeof(TestHelper).GetPublicInstanceProperties();

            properties.ShouldNotBeNull();
            properties.ShouldHaveSingleItem();
            properties.First().Name.ShouldBe("PublicInstanceProperty");
        }

        public class TestHelper
        {
            public int PublicInstanceField;
            public static int PublicStaticField;
            internal int NonPublicInstanceField = 0;
            internal static int NonPublicStaticField = 0;

            public int PublicInstanceProperty { get; set; }

            public static int PublicStaticProperty { get; set; }

            internal int NonPublicInstanceProperty { get; set; }

            internal static int NonPublicStaticProperty { get; set; }
        }
    }
}
