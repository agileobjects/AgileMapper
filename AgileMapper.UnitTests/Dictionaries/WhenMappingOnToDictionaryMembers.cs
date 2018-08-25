namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using Common;
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
    }
}