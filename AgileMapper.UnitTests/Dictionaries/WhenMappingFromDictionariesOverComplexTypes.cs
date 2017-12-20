namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDictionariesOverComplexTypes
    {
        [Fact]
        public void ShouldPopulateADateTimeMemberFromAnUntypedEntry()
        {
            var now = DateTime.Now.ToCurrentCultureString();

            var source = new Dictionary<string, object> { ["Value"] = now };
            var target = new PublicProperty<DateTime> { Value = DateTime.Now.AddHours(1) };
            var result = Mapper.Map(source).Over(target);

            result.Value.ToCurrentCultureString().ShouldBe(now);
        }

        [Fact]
        public void ShouldOverwriteAStringPropertyToNullFromATypedEntry()
        {
            var source = new Dictionary<string, string> { ["Value"] = null };
            var target = new PublicField<string> { Value = "To be overwritten..." };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldOverwriteAnIntPropertyToDefaultFromATypedEntry()
        {
            var source = new Dictionary<string, string> { ["Value"] = null };
            var target = new PublicField<int> { Value = 6473 };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldOverwriteAComplexTypePropertyToNull()
        {
            var source = new Dictionary<string, object>
            {
                ["Name"] = "Frank",
                ["Address"] = default(Address)
            };
            var target = new Customer { Name = "Charlie", Address = new Address { Line1 = "Cat Lane" } };
            var result = Mapper.Map(source).Over(target);

            result.Name.ShouldBe("Frank");
            result.Address.ShouldBeNull();
        }

        [Fact]
        public void ShouldOverwriteFromASimpleTypeDictionaryImplementation()
        {
            var source = new StringKeyedDictionary<string> { ["Value"] = "LaLaLa!" };
            var target = new PublicField<string> { Value = "DumDeeDum!" };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBe("LaLaLa!");
        }
    }
}