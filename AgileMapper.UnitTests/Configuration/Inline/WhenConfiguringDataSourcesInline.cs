namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using AgileMapper.Extensions;
    using AgileMapper.Members;
    using Common;
    using NetStandardPolyfills;
    using TestClasses;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringDataSourcesInline
    {
        [Fact]
        public void ShouldApplyInlineDataSourceExpressions()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<int>>(c => c
                        .Map(ctx => ctx.Source.Value * 2)
                        .To(pf => pf.Value));

                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicField<int>>(c => c
                        .Map(ctx => ctx.Source.Value * 2)
                        .To(pf => pf.Value));

                result1.Value.ShouldBe(source1.Value * 2);
                result2.Value.ShouldBe(source2.Value * 2);

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }

        [Fact]
        public void ShouldApplyInlineDataSourceFunctions()
        {
            Func<PublicField<string>, PublicField<string>, string> sourceYes =
                (s, t) => s.Value + "? Yes!";

            Func<PublicField<string>, PublicField<string>, string> sourceNo =
                (s, t) => s.Value + "? No!";

            var source1 = new PublicField<string> { Value = "One" };
            var source2 = new PublicField<string> { Value = "Two" };

            using (var mapper = Mapper.CreateNew())
            {
                var result1Yes = mapper
                    .Map(source1)
                    .OnTo(new PublicField<string>(), c => c
                        .Map(sourceYes)
                        .To(pf => pf.Value));

                var result1No = mapper
                    .Map(source1)
                    .OnTo(new PublicField<string>(), c => c
                        .Map(sourceNo)
                        .To(pf => pf.Value));

                var result2Yes = mapper
                    .Map(source2)
                    .OnTo(new PublicField<string>(), c => c
                        .Map(sourceYes)
                        .To(pf => pf.Value));

                var result2No = mapper
                    .Map(source2)
                    .OnTo(new PublicField<string>(), c => c
                        .Map(sourceNo)
                        .To(pf => pf.Value));

                result1Yes.Value.ShouldBe("One? Yes!");
                result1No.Value.ShouldBe("One? No!");
                result2Yes.Value.ShouldBe("Two? Yes!");
                result2No.Value.ShouldBe("Two? No!");

                mapper.InlineContexts().Count.ShouldBe(2);
            }
        }

        [Fact]
        public void ShouldApplyMultipleInlineDataSourceExpressions()
        {
            var source1 = new PublicProperty<int> { Value = 2 };
            var source2 = new PublicProperty<int> { Value = 4 };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicTwoFieldsStruct<int, long>>(
                        c => c.Map(ctx => ctx.Source.Value - 2)
                              .To(pfs => pfs.Value1),
                        c => c.Map(ctx => (long)(ctx.Source.Value / 2))
                              .To(pfs => pfs.Value2));

                result1.Value1.ShouldBe(source1.Value - 2);
                result1.Value2.ShouldBe(source1.Value / 2);

                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicTwoFieldsStruct<int, long>>(c => c
                        .Map(ctx => ctx.Source.Value - 2)
                        .To(pfs => pfs.Value1));

                result2.Value1.ShouldBe(source2.Value - 2);
                result2.Value2.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldApplyInlineConstantDataSourceExpressionsConditionally()
        {
            var source1 = new PublicProperty<string> { Value = "Yes" };
            var source2 = new PublicProperty<string> { Value = "No" };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<string>>(c => c
                        .If((pp, pf) => pp.Value != "No")
                        .Map("Maybe?", pf => pf.Value));

                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicField<string>>(c => c
                        .If((pp, pf) => pp.Value != "No")
                        .Map("Maybe?", pf => pf.Value));

                result1.Value.ShouldBe("Maybe?");
                result2.Value.ShouldBe("No");

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }

        [Fact]
        public void ShouldApplyDifferingInlineDelegateDataSources()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<int>>(c => c
                        .Map(SubtractOne)
                        .To(pf => pf.Value));


                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicField<int>>(c => c
                        .Map(SubtractThree)
                        .To(pf => pf.Value));

                result1.Value.ShouldBe(source1.Value - 1);
                result2.Value.ShouldBe(source2.Value - 3);
            }

        }

        [Fact]
        public void ShouldApplyDifferingInlineConstantDataSourcesConditionally()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<int>>(c => c
                        .If(ctx => ctx.Source.Value > 0)
                        .Map(ctx => ctx.Source.Value + 2)
                        .To(pf => pf.Value));


                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicField<int>>(c => c
                        .If(ctx => ctx.Source.Value > 0)
                        .Map(ctx => ctx.Source.Value + 1)
                        .To(pf => pf.Value));

                result1.Value.ShouldBe(source1.Value + 2);
                result2.Value.ShouldBe(source2.Value + 1);
            }
        }

        [Fact]
        public void ShouldApplyDifferingInlineDataSourceFunctions()
        {
            Func<IMappingData<MysteryCustomer, MysteryCustomerViewModel>, string> nameConcatLine1 =
                ctx => ctx.Source.Name + ", " + ctx.Source.Address.Line1;

            using (var mapper = Mapper.CreateNew())
            {
                var source = new MysteryCustomer { Name = "Bob", Address = new Address { Line1 = "Over there" } };

                var result1 = mapper
                    .Map(source)
                    .ToANew<MysteryCustomerViewModel>(c => c
                        .Map(nameConcatLine1)
                        .To(mcvm => mcvm.AddressLine1));

                result1.AddressLine1.ShouldBe("Bob, Over there");

                var result2 = mapper
                    .Map(source)
                    .ToANew<MysteryCustomerViewModel>(c => c
                        .Map(nameConcatLine1)
                        .To(mcvm => mcvm.Name));

                result2.Name.ShouldBe("Bob, Over there");
                result2.AddressLine1.ShouldBe("Over there");
            }
        }

        [Fact]
        public void ShouldExtendMapperConfiguration()
        {
            var source = new PublicProperty<int> { Value = 2 };

            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<int>>()
                    .To<PublicTwoFields<long, long>>()
                    .Map((pp, ptf) => pp.Value)
                    .To(ptf => ptf.Value1);      // Configure Value -> Value1

                var result1 = mapper
                    .Map(source)
                    .ToANew<PublicTwoFields<long, long>>(c => c
                        .Map((pp, ptf) => pp.Value)
                        .To(ptf => ptf.Value2)); // Add Value -> Value2

                result1.Value1.ShouldBe(source.Value);
                result1.Value2.ShouldBe(source.Value);

                var result2 = mapper
                    .Map(source)
                    .ToANew<PublicTwoFields<long, long>>(c => c
                        .Map((pp, ptf) => pp.Value * 2)
                        .To(ptf => ptf.Value1)   // Overwrite Value -> Value1
                        .And
                        .Map((pp, ptf) => pp.Value * 3)
                        .To(ptf => ptf.Value2)); // Add Value -> Value2m

                result2.Value1.ShouldBe(source.Value * 2);
                result2.Value2.ShouldBe(source.Value * 3);
            }
        }

        [Fact]
        public void ShouldApplyDifferingRuleSetInlineDataSourceMemberConfig()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<int>>(c => c
                        .Map(ctx => ctx.Source.Value + 1)
                        .To(pf => pf.Value));

                result1.Value.ShouldBe(source1.Value + 1);

                var result2 = mapper
                    .Map(source2)
                    .Over(result1, c => c
                        .Map(ctx => ctx.Source.Value + 1)
                        .To(pf => pf.Value));

                result2.Value.ShouldBe(source2.Value + 1);
            }
        }

        [Fact]
        public void ShouldApplyDifferingTargetTypeInlineDataSourceMemberConfig()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<int>>(c => c
                        .Map(ctx => ctx.Source.Value + 1)
                        .To(pf => pf.Value));

                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicField<long>>(c => c
                        .Map(ctx => ctx.Source.Value + 1)
                        .To(pf => pf.Value));

                result1.Value.ShouldBe(source1.Value + 1);
                result2.Value.ShouldBe(source2.Value + 1);
            }
        }

        [Fact]
        public void ShouldApplyAnInlineNullCheckedArrayIndexDataSource()
        {
            var source = new PublicProperty<PublicField<Address>[]>
            {
                Value = new[]
                {
                    new PublicField<Address> { Value = new Address { Line1 = "1.1" } },
                    new PublicField<Address> { Value = new Address { Line1 = "1.2" } }
                }
            };

            var result = Mapper.Map(source).ToANew<PublicProperty<Address>>(cfg => cfg
                .Map(s => s.Value[1].Value, t => t.Value));

            result.Value.ShouldNotBeNull();
            result.Value.Line1.ShouldBe("1.2");
        }

        [Fact]
        public void ShouldApplyAnInlineNullCheckedIndexDataSource()
        {
            var source = new PublicProperty<PublicIndex<int, PublicField<Address>>>
            {
                Value = new PublicIndex<int, PublicField<Address>>
                {
                    [0] = new PublicField<Address> { Value = new Address { Line1 = "1.1" } },
                    [1] = new PublicField<Address> { Value = new Address { Line1 = "1.2" } }
                }
            };

            var sourceParameter = Parameter(source.GetType(), "s");
            var sourceValueProperty = Property(sourceParameter, "Value");
            var sourceValueIndexer = sourceValueProperty.Type.GetPublicInstanceProperty("Item");
            var sourceValueIndex = MakeIndex(sourceValueProperty, sourceValueIndexer, new[] { Constant(1) });

            var sourceLambda = Lambda<Func<PublicProperty<PublicIndex<int, PublicField<Address>>>, Address>>(
                Field(sourceValueIndex, "Value"),
                sourceParameter);

            var result = Mapper.Map(source).ToANew<PublicField<Address>>(cfg => cfg
                .Map(sourceLambda, t => t.Value));

            result.Value.ShouldNotBeNull();
            result.Value.Line1.ShouldBe("1.2");
        }

        [Fact]
        public void ShouldHandleANullSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(default(PersonViewModel))
                    .ToANew<Person>(cfg => cfg
                        .Map((pvm, p) => "Named: " + pvm.Name)
                        .To(p => p.Name));

                result.ShouldBeNull();
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/64
        [Fact]
        public void ShouldApplyAConfiguredRootSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new { Value1 = 8392, Value = new { Value2 = 5482 } };

                var result = source
                    .MapUsing(mapper)
                    .ToANew<PublicTwoFields<int, int>>(cfg => cfg
                        .Map((s, ptf) => s.Value)
                        .ToTarget());

                result.Value1.ShouldBe(8392);
                result.Value2.ShouldBe(5482);
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredRootSourceObjectMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source1 = new PublicProperty<object>
                {
                    Value = new PublicField<string> { Value = "Hello!" }
                };

                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicProperty<string>>(cfg => cfg
                        .Map((s, t) => s.Value)
                        .ToTarget());

                result1.Value.ShouldBe("Hello!");

                var source2 = new PublicProperty<object>
                {
                    Value = new PublicProperty<string> { Value = "Goodbye!" }
                };

                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicProperty<string>>(cfg => cfg
                        .Map((s, t) => s.Value)
                        .ToTarget());

                result2.Value.ShouldBe("Goodbye!");

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }

        #region Helper Members

        private static Expression<Func<IMappingData<PublicProperty<int>, PublicField<int>>, object>> SubtractOne =>
            ctx => ctx.Source.Value - 1;

        private static Expression<Func<IMappingData<PublicProperty<int>, PublicField<int>>, object>> SubtractThree =>
            ctx => ctx.Source.Value - 3;

        #endregion
    }
}
