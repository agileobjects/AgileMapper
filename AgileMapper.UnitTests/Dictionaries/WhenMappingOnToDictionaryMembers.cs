namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingOnToDictionaryMembers
    {
        [Fact]
        public void ShouldMapAnIDictionaryOnToAConvertibleSimpleTypedDictionary()
        {
            var guidOne = Guid.NewGuid();
            var guidTwo = Guid.NewGuid();
            var source = new PublicProperty<IDictionary<string, Guid?>>
            {
                Value = new Dictionary<string, Guid?> { ["ONEah-ah-ah"] = guidOne, ["TWOah-ah-ah"] = guidTwo }
            };
            var target = new PublicField<Dictionary<string, string>>
            {
                Value = new Dictionary<string, string> { ["ONEah-ah-ah"] = null, ["THREEah-ah-ah"] = "gibblets" }
            };
            Mapper.Map(source).Over(target);

            target.Value["ONEah-ah-ah"].ShouldBe(guidOne.ToString());
            target.Value["TWOah-ah-ah"].ShouldBe(guidTwo.ToString());
            target.Value["THREEah-ah-ah"].ShouldBe("gibblets");
        }

        [Fact]
        public void ShouldMapOnToAComplexTypeDictionary()
        {
            Address sourceWorkAddress, targetHomeAddress;

            var source = new PublicField<Dictionary<string, Address>>
            {
                Value = new Dictionary<string, Address>
                {
                    ["Home"] = new Address { Line1 = "Home", Line2 = "My Town" },
                    ["Work"] = sourceWorkAddress = new Address { Line1 = "Work", Line2 = "My City" }
                }
            };

            var target = new PublicReadOnlyField<Dictionary<string, Address>>(new Dictionary<string, Address>
            {
                ["Home"] = targetHomeAddress = new Address { Line1 = "My Home" }
            });

            Mapper.Map(source).OnTo(target);

            target.Value.Count.ShouldBe(2);

            target.Value.ShouldContainKey("Home");
            target.Value["Home"].ShouldBeSameAs(targetHomeAddress);
            target.Value["Home"].Line1.ShouldBe("My Home");
            target.Value["Home"].Line2.ShouldBe("My Town");

            target.Value.ShouldContainKey("Work");
            target.Value["Work"].ShouldNotBeSameAs(sourceWorkAddress);
            target.Value["Work"].Line1.ShouldBe("Work");
            target.Value["Work"].Line2.ShouldBe("My City");
        }
    }
}