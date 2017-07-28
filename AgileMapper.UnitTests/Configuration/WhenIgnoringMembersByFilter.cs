namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using TestClasses;
    using Xunit;

    public class WhenIgnoringMembersByFilter
    {
        [Fact]
        public void ShouldIgnoreMembersByTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<PublicTwoFields<int, long>>()
                    .IgnoreTargetMembersOfType<int>();

                var source = new { Value1 = 1, Value2 = 2 };

                var matchingResult = mapper.Map(source).ToANew<PublicTwoFields<int, long>>();
                matchingResult.Value1.ShouldBeDefault();
                matchingResult.Value2.ShouldBe(2L);

                var nonMatchingResult = mapper.Map(source).ToANew<PublicTwoFields<long, int>>();
                nonMatchingResult.Value1.ShouldBe(1L);
                nonMatchingResult.Value2.ShouldBe(2L);
            }
        }

        [Fact]
        public void ShouldIgnoreMembersByTypeSourceTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicField<int>>()
                    .IgnoreTargetMembersOfType<int>();

                var matchingSource = new PublicProperty<string> { Value = "999" };
                var nonMatchingSource = new { Value = 987 };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<int>>();
                matchingResult.Value.ShouldBeDefault();

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicProperty<int>>();
                nonMatchingTargetResult.Value.ShouldBe(999);

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicField<int>>();
                nonMatchingSourceResult.Value.ShouldBe(987);

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicProperty<int>>();
                nonMatchingResult.Value.ShouldBe(987);
            }
        }
    }
}