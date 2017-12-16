namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using TestClasses;
    using Xunit;

    public class WhenMappingOnToDynamicMembers
    {
        [Fact]
        public void ShouldMapFromANestedSimpleTypedDictionary()
        {
            var guidOne = Guid.NewGuid();
            var guidTwo = Guid.NewGuid();

            var source = new PublicProperty<Dictionary<string, Guid?>>
            {
                Value = new Dictionary<string, Guid?> { ["ONEah-ah-ah"] = guidOne, ["TWOah-ah-ah"] = guidTwo }
            };

            dynamic targetDynamic = new ExpandoObject();

            targetDynamic.ONEah_ah_ah = guidOne;
            targetDynamic.TWOah_ah_ah = guidTwo;
            targetDynamic.THREEah_ah_ah = "gibblets";

            var target = new PublicField<dynamic> { Value = targetDynamic };

            Mapper.Map(source).Over(target);

            ((Guid?)target.Value.ONEah_ah_ah).ShouldBe(guidOne);
            ((Guid?)target.Value.TWOah_ah_ah).ShouldBe(guidTwo);
            ((string)target.Value.THREEah_ah_ah).ShouldBe("gibblets");
        }
    }
}
