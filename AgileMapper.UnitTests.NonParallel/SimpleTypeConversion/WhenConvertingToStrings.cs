namespace AgileObjects.AgileMapper.UnitTests.NonParallel.SimpleTypeConversion
{
    using System;
    using System.Globalization;
    using Common;
    using Common.TestClasses;
    using Xunit;

    public class WhenConvertingToStrings
    {
        [Fact]
        public void ShouldMapADateTimeToAStringUsingTheThreadDateFormat()
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

        [Fact]
        public void ShouldMapANullableDateTimeToAStringUsingTheThreadDateFormat()
        {
            var currentCulture = CultureInfo.CurrentCulture;

            var source = new PublicField<DateTime?> { Value = new DateTime(2001, 03, 01) };

            try
            {
                var enGb = CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");

                var enGbResult = Mapper.Map(source).ToANew<PublicField<string>>();
                enGbResult.Value.ShouldBe(source.Value.GetValueOrDefault().ToString(enGb));

                var enUs = CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

                var enUsResult = Mapper.Map(source).ToANew<PublicField<string>>();
                enUsResult.Value.ShouldBe(source.Value.GetValueOrDefault().ToString(enUs));
            }
            finally
            {
                CultureInfo.CurrentCulture = currentCulture;
            }
        }

        [Fact]
        public void ShouldMapANullNullableDateTimeToAStringUsingTheThreadDateFormat()
        {
            var currentCulture = CultureInfo.CurrentCulture;

            var source = new PublicField<DateTime?> { Value = null };

            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");

                var enGbResult = Mapper.Map(source).ToANew<PublicField<string>>();
                enGbResult.Value.ShouldBeNull();

                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

                var enUsResult = Mapper.Map(source).ToANew<PublicField<string>>();
                enUsResult.Value.ShouldBeNull();
            }
            finally
            {
                CultureInfo.CurrentCulture = currentCulture;
            }
        }
    }
}
