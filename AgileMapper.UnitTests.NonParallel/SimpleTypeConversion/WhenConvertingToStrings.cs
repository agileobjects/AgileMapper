namespace AgileObjects.AgileMapper.UnitTests.NonParallel.SimpleTypeConversion
{
    using System;
    using System.Globalization;
    using TestClasses;
    using Xunit;

    public class WhenConvertingToStrings
    {
        [Fact]
        public void ShouldMapADateTimeToAStringUsingTheThreadCulture()
        {
            var currentCulture = CultureInfo.CurrentCulture;

            var source = new PublicField<DateTime> { Value = new DateTime(2001, 07, 06) };

            try
            {
                var enGb = CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");

                var enGbResult = Mapper.Map(source).ToANew<PublicField<string>>();
                enGbResult.Value.ShouldBe(source.Value.ToString(enGb));

                var enUs = CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

                var enUsResult = Mapper.Map(source).ToANew<PublicField<string>>();
                enUsResult.Value.ShouldBe(source.Value.ToString(enUs));
            }
            finally
            {
                CultureInfo.CurrentCulture = currentCulture;
            }
        }
    }
}
