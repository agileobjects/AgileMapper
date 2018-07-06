namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingOnToStructMembers
    {
        [Fact]
        public void ShouldMapAMemberProperty()
        {
            var source = new PublicPropertyStruct<PublicTwoFieldsStruct<string, string>>
            {
                Value = new PublicTwoFieldsStruct<string, string>
                {
                    Value1 = "Over here!",
                    Value2 = "Yes, here!"
                }
            };

            var target = new PublicPropertyStruct<PublicTwoFieldsStruct<string, string>>
            {
                Value = new PublicTwoFieldsStruct<string, string>
                {
                    Value1 = "Over there!"
                }
            };

            var result = Mapper.Map(source).OnTo(target);

            result.Value.Value1.ShouldNotBe("Over here!");
            result.Value.Value1.ShouldBe("Over there!");
            result.Value.Value2.ShouldBe("Yes, here!");
        }

        [Fact]
        public void ShouldMapFromAConfiguredSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MysteryCustomer>()
                    .OnTo<PublicPropertyStruct<string>>()
                    .Map(ctx => ctx.Source.Address.Line1)
                    .To(pps => pps.Value);

                var source = new MysteryCustomer { Name = "Andy", Address = new Address { Line1 = "Line 1!" } };
                var target = new PublicPropertyStruct<string>();
                var result = mapper.Map(source).OnTo(target);

                result.Value.ShouldBe("Line 1!");
            }
        }

        [Fact]
        public void ShouldHandleANullConfiguredSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MysteryCustomer>()
                    .OnTo<PublicPropertyStruct<string>>()
                    .Map(ctx => ctx.Source.Address.Line1)
                    .To(pps => pps.Value);

                var source = new MysteryCustomer { Name = "Andy" };
                var target = new PublicPropertyStruct<string>();
                var result = mapper.Map(source).OnTo(target);

                result.Value.ShouldBeNull();
            }
        }
    }
}