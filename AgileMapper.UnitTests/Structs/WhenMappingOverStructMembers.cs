namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using TestClasses;
    using Xunit;

    public class WhenMappingOverStructMembers
    {
        [Fact]
        public void ShouldOverwriteAMemberToDefault()
        {
            var source = new PublicProperty<PublicField<string>> { Value = null };
            var target = new PublicProperty<PublicPropertyStruct<string>>
            {
                Value = new PublicPropertyStruct<string> { Value = "Gone" }
            };

            var result = Mapper.Map(source).Over(target);

            result.ShouldBeSameAs(target);
            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceMember()
        {
            var source = new { Value1 = "You" };
            var target = new PublicTwoFieldsStruct<string, int> { Value1 = "kjd", Value2 = 527 };
            var result = Mapper.Map(source).Over(target);

            result.Value1.ShouldBe("You");  
            result.Value2.ShouldBe(527);
        }

        [Fact]
        public void ShouldApplyAConfiguredConstant()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string>>()
                    .Over<PublicPropertyStruct<int>>()
                    .Map("123")
                    .To(pps => pps.Value);

                var source = new PublicField<PublicField<string>>
                {
                    Value = new PublicField<string> { Value = "456" }
                };
                var target = new PublicField<PublicPropertyStruct<int>>
                {
                    Value = new PublicPropertyStruct<int> { Value = 789 }
                };
                var result = mapper.Map(source).Over(target);

                result.Value.Value.ShouldBe(123);
            }
        }
    }
}