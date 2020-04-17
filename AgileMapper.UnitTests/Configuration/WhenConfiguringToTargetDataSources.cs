namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AgileMapper.Extensions;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringToTargetDataSources
    {
        private int _addressCreationCount;

        // See https://github.com/agileobjects/AgileMapper/issues/64
        [Fact]
        public void ShouldApplyAToTargetDataSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new { Value1 = 123, Value = new { Value2 = 456 } };

                mapper.WhenMapping
                    .From(source)
                    .To<PublicTwoFields<int, int>>()
                    .Map(ctx => ctx.Source.Value)
                    .ToTarget();

                var result = source
                    .MapUsing(mapper)
                    .ToANew<PublicTwoFields<int, int>>();

                result.Value1.ShouldBe(123);
                result.Value2.ShouldBe(456);
            }
        }

        [Fact]
        public void ShouldApplyAToTargetDataSourceConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<PublicPropertyStruct<int>, int>>()
                    .OnTo<PublicTwoFields<int, int>>()
                    .If((s, t) => s.Value1.Value > 5)
                    .Map((s, t) => s.Value1)
                    .ToTarget();

                mapper.WhenMapping
                    .From<PublicPropertyStruct<int>>()
                    .OnTo<PublicTwoFields<int, int>>()
                    .Map((s, t) => s.Value)
                    .To(t => t.Value1);

                var matchingSource = new PublicTwoFieldsStruct<PublicPropertyStruct<int>, int>
                {
                    Value1 = new PublicPropertyStruct<int> { Value = 10 },
                    Value2 = 627
                };

                var target = new PublicTwoFields<int, int> { Value2 = 673282 };

                mapper.Map(matchingSource).OnTo(target);

                target.Value1.ShouldBe(10);
                target.Value2.ShouldBe(673282);

                var nonMatchingSource = new PublicTwoFieldsStruct<PublicPropertyStruct<int>, int>
                {
                    Value1 = new PublicPropertyStruct<int> { Value = 1 },
                    Value2 = 9285
                };

                target.Value1 = target.Value2 = default(int);

                mapper.Map(nonMatchingSource).OnTo(target);

                target.Value1.ShouldBeDefault();
                target.Value2.ShouldBe(9285);
            }
        }

        [Fact]
        public void ShouldApplyANestedOverwriteToTargetDataSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<int, PublicField<PublicTwoFields<int, int>>>>()
                    .Over<PublicTwoFields<int, int>>()
                    .Map((s, t) => s.Value2.Value)
                    .ToTarget();

                var source = new PublicTwoFields<int, PublicField<PublicTwoFields<int, int>>>
                {
                    Value1 = 6372,
                    Value2 = new PublicField<PublicTwoFields<int, int>>
                    {
                        Value = new PublicTwoFields<int, int>
                        {
                            Value2 = 8262
                        }
                    }
                };

                var target = new PublicTwoFields<int, int>
                {
                    Value1 = 637,
                    Value2 = 728
                };

                mapper.Map(source).Over(target);

                target.Value1.ShouldBeDefault(); // <- Because Value2.Value.Value1 will overwrite 6372
                target.Value2.ShouldBe(8262);
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/111
        [Fact]
        public void ShouldApplyAToTargetSimpleTypeConstantConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<string>().ToANew<string>()
                    .If(ctx => string.IsNullOrEmpty(ctx.Source))
                    .Map(default(string)).ToTarget();

                var source = new Address { Line1 = "Here", Line2 = string.Empty };
                var result = mapper.Map(source).ToANew<Address>();

                result.Line1.ShouldBe("Here");
                result.Line2.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldApplyAToTargetSimpleTypeExpressionResult()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<string>().ToANew<string>()
                    .Map((s, t) => string.IsNullOrEmpty(s) ? null : s).ToTarget();

                var source = new Address { Line1 = "There", Line2 = string.Empty };
                var result = mapper.Map(source).ToANew<Address>();

                result.Line1.ShouldBe("There");
                result.Line2.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldApplyAToTargetSimpleTypeExpressionToANestedMemberConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<int>().ToANew<int>()
                    .If(ctx => ctx.Source % 2 == 0)
                    .Map(ctx => ctx.Source * 2).ToTarget();

                var nonMatchingSource = new { ValueValue = 3 };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<PublicField<int>>>();

                nonMatchingResult.Value.ShouldNotBeNull();
                nonMatchingResult.Value.Value.ShouldBe(3);

                var matchingSource = new { ValueValue = 4 };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<PublicField<int>>>();

                matchingResult.Value.ShouldNotBeNull();
                matchingResult.Value.Value.ShouldBe(8);
            }
        }

        [Fact]
        public void ShouldApplyAToTargetSimpleTypeExpressionToAComplexTypeListMemberConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<int>().ToANew<int>()
                    .If((s, t) => s % 2 == 0)
                    .Map(ctx => ctx.Source * 2).ToTarget();

                var source = new PublicField<List<PublicField<int>>>
                {
                    Value = new List<PublicField<int>>
                    {
                        new PublicField<int> { Value = 1 },
                        new PublicField<int> { Value = 2 },
                        new PublicField<int> { Value = 3 }
                    }
                };
                var result = mapper.Map(source).ToANew<PublicField<List<PublicField<int>>>>();

                result.Value.ShouldNotBeNull();
                result.Value.ShouldBe(pf => pf.Value, 1, 4, 3);
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/113
        [Fact]
        public void ShouldApplyAComplexToSimpleTypeEnumerableProjectionToTheRootTarget()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>[]>()
                    .To<int[]>()
                    .Map(ctx => ctx.Source.Select(v => v.Value))
                    .ToTarget();

                var source = new[]
                {
                    new PublicField<int> { Value = 1 },
                    new PublicField<int> { Value = 2 },
                    new PublicField<int> { Value = 3 }
                };

                var result = mapper.Map(source).ToANew<int[]>();

                result.ShouldNotBeEmpty();
                result.ShouldBe(1, 2, 3);
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/174
        [Fact]
        public void ShouldApplyASimpleTypeToTargetDataSourceAtRuntime()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new PublicField<object> { Value = 123 };

                mapper.WhenMapping
                    .From<int>()
                    .To<PublicField<int>>()
                    .Map(i => i, t => t.Value);

                var result = source
                    .MapUsing(mapper)
                    .ToANew<PublicProperty<PublicField<int>>>();

                result.Value.ShouldNotBeNull();
                result.Value.Value.ShouldBe(123);
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/68
        [Fact]
        public void ShouldSupportConfiguringARootSourceUsingMappingContext()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<WhenConfiguringDataSources.Model>()
                    .To<WhenConfiguringDataSources.ModelDto>()
                    .Map(ctx => ctx.Source.Statistics)
                    .ToTarget();

                var source = new WhenConfiguringDataSources.Model
                {
                    SomeOtherProperties = "jyutrgf",
                    Statistics = new WhenConfiguringDataSources.Statistics
                    {
                        Ranking = 0.5f,
                        SomeOtherRankingStuff = "uityjtgrf"
                    }
                };

                var result = mapper.Map(source).ToANew<WhenConfiguringDataSources.ModelDto>();

                result.SomeOtherProperties.ShouldBe("jyutrgf");
                result.Ranking.ShouldBe(0.5f);
                result.SomeOtherRankingStuff.ShouldBe("uityjtgrf");
            }
        }

        [Fact]
        public void ShouldApplyAToTargetComplexTypeToANestedMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<PublicField<string>>>()
                    .To<PublicField<int>>()
                    .Map(ctx => ctx.Source.Value)
                    .ToTarget();

                var source = new PublicField<PublicField<PublicField<string>>>
                {
                    Value = new PublicField<PublicField<string>>
                    {
                        Value = new PublicField<string> { Value = "53632" }
                    }
                };

                var result = mapper.Map(source).ToANew<PublicField<PublicField<int>>>();

                result.Value.Value.ShouldBe(53632);
            }
        }

        [Fact]
        public void ShouldApplyAToTargetComplexTypeToAComplexTypeEnumerableElement()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<PublicField<string>>>()
                    .ToANew<PublicField<string>>()
                    .Map(ctx => ctx.Source.Value)
                    .ToTarget();

                var source = new[]
                {
                    new PublicField<PublicField<string>>
                    {
                        Value = new PublicField<string> { Value = "kjfcrkjnad" }
                    },
                    new PublicField<PublicField<string>>
                    {
                        Value = new PublicField<string> { Value = "owkjwsnbsgtf" }
                    }
                };

                var result = mapper.Map(source).ToANew<Collection<PublicField<string>>>();

                result.Count.ShouldBe(2);
                result.First().Value.ShouldBe("kjfcrkjnad");
                result.Second().Value.ShouldBe("owkjwsnbsgtf");
            }
        }

        [Fact]
        public void ShouldApplyAToTargetComplexTypeEnumerable()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<Address, Address[]>>()
                    .To<List<Address>>()
                    .Map((s, r) => s.Value2)
                    .ToTarget();

                var source = new PublicTwoFields<Address, Address[]>
                {
                    Value1 = new Address { Line1 = "Here", Line2 = "There" },
                    Value2 = new[]
                    {
                        new Address { Line1 = "Somewhere", Line2 = "Else" },
                        new Address { Line1 = "Elsewhere"}
                    }
                };

                var result = mapper.Map(source).ToANew<List<Address>>();

                result.Count.ShouldBe(2);
                result.First().Line1.ShouldBe("Somewhere");
                result.First().Line2.ShouldBe("Else");
                result.Second().Line1.ShouldBe("Elsewhere");
                result.Second().Line2.ShouldBeNull();

                source.Value2 = null;

                var nullResult = mapper.Map(source).ToANew<List<Address>>();

                nullResult.ShouldBeEmpty();
            }
        }

        [Fact]
        public void ShouldApplyMultipleToTargetComplexTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new
                {
                    PropertyOne = new { Value1 = "Value 1!" },
                    PropertyTwo = new { Value2 = "Value 2!" },
                };

                mapper.WhenMapping
                    .From(source)
                    .To<PublicTwoFields<string, string>>()
                    .Map((s, t) => s.PropertyOne)
                    .ToTarget()
                    .And
                    .Map((s, t) => s.PropertyTwo)
                    .ToTarget();

                var result = mapper.Map(source).ToANew<PublicTwoFields<string, string>>();

                result.Value1.ShouldBe("Value 1!");
                result.Value2.ShouldBe("Value 2!");
            }
        }

        [Fact]
        public void ShouldApplyMultipleToTargetSimpleTypeEnumerables()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<int[], long[]>>()
                    .To<decimal[]>()
                    .Map(xtx => xtx.Source.Value1)
                    .ToTarget()
                    .And
                    .Map((s, t) => s.Value2)
                    .ToTarget();

                var source = new PublicTwoFields<int[], long[]>
                {
                    Value1 = new[] { 1, 2, 3 },
                    Value2 = new[] { 1L, 2L, 3L }
                };

                var result = mapper.Map(source).ToANew<decimal[]>();

                result.Length.ShouldBe(6);
            }
        }

        [Fact]
        public void ShouldApplyAToTargetSimpleTypeToANestedComplexTypeMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<string>().To<PublicEnumerable<int>>()
                    .Map(ctx => PublicEnumerable<int>.Parse(ctx.Source)).ToTarget();

                mapper.GetPlanFor<PublicField<string>>().ToANew<PublicField<PublicEnumerable<int>>>();

                var source = new PublicField<string> { Value = "1,2,3" };
                var result = mapper.Map(source).ToANew<PublicField<PublicEnumerable<int>>>();

                result.ShouldNotBeNull();
                result.Value.ShouldNotBeNull();
                result.Value.ShouldBe(1, 2, 3);
            }
        }

        [Fact]
        public void ShouldApplyAToTargetSimpleTypeToANestedComplexTypeMemberConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<string>().To<PublicEnumerable<int>>()
                    .If(cxt => cxt.Source.Contains(','))
                    .Map(ctx => PublicEnumerable<int>.Parse(ctx.Source)).ToTarget();

                mapper.GetPlanFor<PublicField<string>>().ToANew<PublicField<PublicEnumerable<int>>>();
            }
        }

        [Fact]
        public void ShouldMapAToTargetEnumerableProjectionResultToANestedEnumerable()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Address[]>()
                    .ToANew<Address[]>()
                    .Map(ctx => ctx.Source.Select(CreateAddress).ToArray())
                    .ToTarget();

                var source = new PublicField<Address[]>
                {
                    Value = new[]
                    {
                        new Address { Line1 = "My house" },
                        new Address { Line1 = "Your house" }
                    }
                };

                _addressCreationCount = 0;

                var result = mapper.Map(source).ToANew<PublicProperty<Address[]>>();

                result.Value.ShouldNotBeNull().Length.ShouldBe(4);
                result.Value.First().Line1.ShouldBe("My house");
                result.Value.First().Line2.ShouldBeNull();
                result.Value.Second().Line1.ShouldBe("Your house");
                result.Value.Second().Line2.ShouldBeNull();
                result.Value.Third().Line1.ShouldBe("Address 1");
                result.Value.Third().Line2.ShouldBe("My house");
                result.Value.Fourth().Line1.ShouldBe("Address 2");
                result.Value.Fourth().Line2.ShouldBe("Your house");

                _addressCreationCount.ShouldBe(2);
            }
        }

        [Fact]
        public void ShouldHandleANullToTargetValue()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MysteryCustomer>()
                    .ToANew<PublicTwoFields<string, string>>()
                    .Map((mc, t) => mc.Name)
                    .To(t => t.Value1)
                    .And
                    .Map((mc, t) => mc.Address)
                    .ToTarget();

                var source = new MysteryCustomer { Name = "Nelly", Address = null };

                var result = mapper.Map(source).ToANew<PublicTwoFields<string, string>>();

                result.Value1.ShouldBe("Nelly");
                result.Value2.ShouldBeNull();
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/145
        [Fact]
        public void ShouldHandleNullToTargetDataSourceNestedMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue145.DataSource>().To<Issue145.DataTarget>()
                    .Map((srcData, tgtData) => srcData.cont).ToTarget();

                var source = new Issue145.DataSource
                {
                    cont = new Issue145.DataSourceContainer()
                };

                var result = mapper.Map(source).ToANew<Issue145.DataTarget>();

                result.ShouldNotBeNull();
                result.ids.ShouldBeNull();
                result.res.ShouldBeNull();
                result.oth.ShouldBeNull();
            }
        }

        #region Helper Members

        // ReSharper disable InconsistentNaming
        internal static class Issue145
        {
            public class IdsSource
            {
                public string Ids { get; set; }
            }

            public class ResultSource
            {
                public string Result { get; set; }
            }

            public class OtherDataSource
            {
                public string COD { get; set; }
            }

            public class DataSourceContainer
            {

                public IdsSource ids;
                public ResultSource res;
                public OtherDataSource oth;
            }

            public class DataSource
            {
                public DataSourceContainer cont;
            }

            public class IdsTarget
            {
                public string Ids { get; set; }
            }

            public class ResultTarget
            {
                public string Result { get; set; }
            }

            public class OtherDataTarget
            {
                public string COD { get; set; }
            }

            public class DataTarget
            {
                public IdsTarget ids;
                public ResultTarget res;
                public OtherDataTarget oth;
            }
        }
        // ReSharper restore InconsistentNaming

        private Address CreateAddress(Address source, int index)
        {
            ++_addressCreationCount;

            return new Address { Line1 = "Address " + (index + 1), Line2 = source.Line1 };
        }

        #endregion
    }
}
