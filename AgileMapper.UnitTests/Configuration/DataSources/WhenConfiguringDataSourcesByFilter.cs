namespace AgileObjects.AgileMapper.UnitTests.Configuration.DataSources
{
    using System;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    // See https://github.com/agileobjects/AgileMapper/issues/208
    public class WhenConfiguringDataSourcesByFilter
    {
        [Fact]
        public void ShouldApplyAConstantByTargetMemberTypeAndMemberType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<int>()
                    .IfTargetMembersMatch(member => member.IsField)
                    .Map(123)
                    .ToTarget();

                var source = new { Value = 456 };

                var matchingResult = mapper.Map(source).ToANew<PublicField<int>>();
                matchingResult.Value.ShouldBe(123);

                var nonMatchingResult = mapper.Map(source).ToANew<PublicProperty<int>>();
                nonMatchingResult.Value.ShouldBe(456);
            }
        }

        [Fact]
        public void ShouldApplyAnAlternateConstantByTargetTypeAndTargetMemberType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<PublicTwoFields<int, Address>>()
                    .IfTargetMembersMatch(member => member.HasType<string>())
                    .Map("Hurrah!")
                    .ToTargetInstead();

                var source = new PublicTwoFields<int, Address>
                {
                    Value1 = 123,
                    Value2 = new Address { Line1 = "One", Line2 = "Two" }
                };

                var matchingResult = mapper.Map(source).ToANew<PublicTwoFields<int, Address>>();
                matchingResult.Value1.ShouldBe(123);
                matchingResult.Value2.Line1.ShouldBe("Hurrah!");
                matchingResult.Value2.Line2.ShouldBe("Hurrah!");

                var nonMatchingResult = mapper.Map(source).ToANew<PublicTwoFields<Address, int>>();
                nonMatchingResult.Value1.Line1.ShouldBe("One");
                nonMatchingResult.Value1.Line2.ShouldBe("Two");
                nonMatchingResult.Value2.ShouldBe(123);
            }
        }

        [Fact]
        public void ShouldApplyAnExpressionBySourceTypeTargetTypeAndTargetMemberAttribute()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<bool>()
                    .To<string>()
                    .IfTargetMembersMatch(member => member.HasAttribute<Issue208.YesNoBoolAttribute>())
                    .Map((b, s) => b ? "Y" : "N")
                    .ToTarget();

                var source = new { AttributeValue = true, NoAttributeValue = false };

                var matchingResult = mapper.Map(source).ToANew<Issue208.YesNoBoolValue>();
                matchingResult.AttributeValue.ShouldBe("Y");
                matchingResult.NoAttributeValue.ShouldBe("false");
            }
        }

        [Fact]
        public void ShouldApplyASourceMemberByTargetTypeAndTargetMemberNameConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<Address, string>>()
                    .ToANew<Address>()
                    .Map(ctx => ctx.Source.Value1.Line1)
                    .To(addr => addr.Line1)
                    .And
                    .IfTargetMembersMatch(member => member.Name.StartsWith(nameof(Address.Line2)))
                    .If(ctx => !string.IsNullOrEmpty(ctx.Source.Value2))
                    .Map(ctx => ctx.Source.Value2)
                    .ToTarget();

                var matchingSource = new PublicTwoFields<Address, string>
                {
                    Value1 = new Address { Line1 = "Value1.Line1", Line2 = "Value1.Line2" },
                    Value2 = "Value2"
                };

                var matchingResult = mapper.Map(matchingSource).ToANew<Address>();
                matchingResult.Line1.ShouldBe("Value1.Line1");
                matchingResult.Line2.ShouldBe("Value2");

                var nonMatchingSource = new PublicTwoFields<Address, string>
                {
                    Value1 = new Address { Line1 = "Value1.Line1", Line2 = "Value1.Line2" },
                    Value2 = string.Empty
                };

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<Address>();
                nonMatchingResult.Line1.ShouldBe("Value1.Line1");
                nonMatchingResult.Line2.ShouldBeNull();
            }
        }

        #region Helper Classes

        private static class Issue208
        {
            public sealed class YesNoBoolAttribute : Attribute
            {
            }

            // ReSharper disable ClassNeverInstantiated.Local
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public class YesNoBoolValue
            {
                [YesNoBool]
                public string AttributeValue { get; set; }

                public string NoAttributeValue { get; set; }
            }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
            // ReSharper restore ClassNeverInstantiated.Local
        }

        #endregion
    }
}
