namespace AgileObjects.AgileMapper.UnitTests.Configuration.DataSources
{
    using System;
    using Common;
    using Common.TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    [Trait("Category", "Checked")]
    // See https://github.com/agileobjects/AgileMapper/issues/208
    public class WhenConfiguringMatcherDataSources
    {
        [Fact]
        public void ShouldApplyAConstantByTargetMemberTypeAndMemberType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<int>()
                    .IfTargetMemberMatches(member => member.IsField)
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
                    .IfTargetMemberMatches(member => member.HasType<string>())
                    .Map("Hurrah!")
                    .ToTargetInstead();

                mapper.WhenMapping
                    .From<PublicTwoFields<int, Address>>()
                    .To<PublicTwoFields<Address, int>>()
                    .Map((s, t) => s.Value1).To(t => t.Value2)
                    .And
                    .Map((s, t) => s.Value2).To(t => t.Value1);

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
                    .IfTargetMemberMatches(member => member.HasAttribute<Issue208.YesNoBoolAttribute>())
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
                    .IfTargetMemberMatches(member => member.Name.StartsWith(nameof(Address.Line2)))
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

        [Fact]
        public void ShouldAllowMultipleDifferentMatcherDataSources()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<DateTime>().To<int>()
                    .IfTargetMemberMatches(member => member.Name == "Value1")
                    .Map((dt, _) => dt.Year).ToTarget()
                    .But
                    .IfTargetMemberMatches(member => member.Name == "Value2")
                    .Map((dt, _) => dt.Month).ToTarget();

                var source = new PublicField<PublicTwoFields<DateTime, DateTime>>
                {
                    Value = new PublicTwoFields<DateTime, DateTime>
                    {
                        Value1 = new DateTime(2001, 02, 03),
                        Value2 = new DateTime(2004, 05, 06)
                    }
                };

                var target = new PublicProperty<PublicTwoFields<int, int>>
                {
                    Value = new PublicTwoFields<int, int>()
                };

                mapper.Map(source).Over(target);

                target.Value.ShouldNotBeNull();
                target.Value.Value1.ShouldBe(2001);
                target.Value.Value2.ShouldBe(05);
            }
        }

        [Fact]
        public void ShouldAllowOverlappingConditionalMemberSpecificDataSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>().To<PublicProperty<string>>()
                    .IfTargetMemberMatches(member => member.HasType<string>())
                    .Map((pf, pp) => pf.Value == 1 ? "Yes" : "No")
                    .ToTarget();

                mapper.WhenMapping
                    .From<PublicField<int>>().To<PublicProperty<string>>()
                    .If(ctx => ctx.Source.Value >= 100)
                    .Map((pf, pp) => pf.Value >= 200 ? "JAH" : "NOPE")
                    .To(pp => pp.Value);

                var matcherYesSource = new PublicField<int> { Value = 1 };
                var matcherYesResult = mapper.Map(matcherYesSource).ToANew<PublicProperty<string>>();
                matcherYesResult.Value.ShouldBe("Yes");

                var matcherNoSource = new PublicField<int> { Value = 0 };
                var matcherNoResult = mapper.Map(matcherNoSource).ToANew<PublicProperty<string>>();
                matcherNoResult.Value.ShouldBe("No");

                var dataSourceYesSource = new PublicField<int> { Value = 1000 };
                var dataSourceYesResult = mapper.Map(dataSourceYesSource).ToANew<PublicProperty<string>>();
                dataSourceYesResult.Value.ShouldBe("JAH");

                var dataSourceNoSource = new PublicField<int> { Value = 150 };
                var dataSourceNoResult = mapper.Map(dataSourceNoSource).ToANew<PublicProperty<string>>();
                dataSourceNoResult.Value.ShouldBe("NOPE");
            }
        }

        [Fact]
        public void ShouldAllowOverlappingMemberSpecificIgnore()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Over<PublicTwoFields<string, string>>()
                    .IfTargetMemberMatches(member => member
                        .IsFieldMatching(f => f.FieldType == typeof(string)))
                    .Map((s, _) => s.ToString())
                    .ToTarget();

                mapper.WhenMapping
                    .To<PublicTwoFields<string, string>>()
                    .Ignore(ptf => ptf.Value1);

                var source = new PublicTwoFields<string, string>
                {
                    Value1 = "Tata",
                    Value2 = "Goodbye"
                };

                var target = new PublicTwoFields<string, string>
                {
                    Value1 = "Cya",
                    Value2 = "Laterz"
                };

                mapper.Map(source).Over(target);

                target.Value1.ShouldBe("Cya");
                target.Value2.ShouldBe(source.ToString());
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
