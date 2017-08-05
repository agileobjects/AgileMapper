namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
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

        [Fact]
        public void ShouldIgnorePropertiesByMemberTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<PublicProperty<string>>()
                    .IgnoreTargetMembersWhere(member => member.IsProperty);

                var nonMatchingResult = mapper.Map(new { Line1 = "xyz" }).ToANew<Address>();
                nonMatchingResult.Line1.ShouldBe("xyz");

                var matchingResult = mapper.Map(new { Value = "xyz" }).ToANew<PublicProperty<string>>();
                matchingResult.Value.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldIgnorePropertiesBySourceTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string>>()
                    .To<PublicProperty<int>>()
                    .IgnoreTargetMembersWhere(member => member.IsProperty);

                var matchingSource = new PublicField<string> { Value = "123" };
                var nonMatchingSource = new { Value = 456 };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicProperty<int>>();
                matchingResult.Value.ShouldBeDefault();

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicField<int>>();
                nonMatchingTargetResult.Value.ShouldBe(123);

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicProperty<int>>();
                nonMatchingSourceResult.Value.ShouldBe(456);

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<int>>();
                nonMatchingResult.Value.ShouldBe(456);
            }
        }

        [Fact]
        public void ShouldIgnoreFieldsByMemberTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<PublicField<int>>()
                    .IgnoreTargetMembersWhere(member => member.IsField);

                var nonMatchingResult = mapper.Map(new { Value = "x" }).ToANew<PublicField<char>>();
                nonMatchingResult.Value.ShouldBe('x');

                var matchingResult = mapper.Map(new { Value = "5" }).ToANew<PublicField<int>>();
                matchingResult.Value.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldIgnoreFieldsBySourceTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<long>>()
                    .To<PublicField<short>>()
                    .IgnoreTargetMembersWhere(member => member.IsField);

                var matchingSource = new PublicField<long> { Value = 111L };
                var nonMatchingSource = new { Value = 222L };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<short>>();
                matchingResult.Value.ShouldBeDefault();

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicField<int>>();
                nonMatchingTargetResult.Value.ShouldBe(111);

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicField<short>>();
                nonMatchingSourceResult.Value.ShouldBe(222);

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<int>>();
                nonMatchingResult.Value.ShouldBe(222);
            }
        }

        [Fact]
        public void ShouldIgnoreSetMethodsByMemberTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<PublicSetMethod<DateTime>>()
                    .IgnoreTargetMembersWhere(member => member.IsSetMethod);

                var nonMatchingResult = mapper.Map(new { Value = 'x' }).ToANew<PublicSetMethod<string>>();
                nonMatchingResult.Value.ShouldBe("x");

                var matchingResult = mapper.Map(new { Value = DateTime.Now }).ToANew<PublicSetMethod<DateTime>>();
                matchingResult.Value.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldIgnoreSetMethodsBySourceTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicGetMethod<int>>()
                    .To<PublicSetMethod<int>>()
                    .IgnoreTargetMembersWhere(member => member.IsSetMethod);

                var matchingSource = new PublicGetMethod<int>(888);
                var nonMatchingSource = new { Value = 333 };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicSetMethod<int>>();
                matchingResult.Value.ShouldBeDefault();

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicField<int>>();
                nonMatchingTargetResult.Value.ShouldBe(888);

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicSetMethod<int>>();
                nonMatchingSourceResult.Value.ShouldBe(333);

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<int>>();
                nonMatchingResult.Value.ShouldBe(333);
            }
        }
    }
}