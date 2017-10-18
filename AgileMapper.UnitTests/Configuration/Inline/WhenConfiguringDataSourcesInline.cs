namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using AgileMapper.Members;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDataSourcesInline
    {
        [Fact]
        public void ShouldApplyInlineDataSourceExpressions()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            using (var mapper = Mapper.CreateNew())
            {
                var timer = Stopwatch.StartNew();

                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<int>>(c => c
                        .Map(ctx => ctx.Source.Value * 2)
                        .To(pf => pf.Value));

                var result1Duration = timer.ElapsedTicks;

                timer.Restart();

                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicField<int>>(c => c
                        .Map(ctx => ctx.Source.Value * 2)
                        .To(pf => pf.Value));

                var result2Duration = timer.ElapsedTicks;

                timer.Stop();

                result1.Value.ShouldBe(source1.Value * 2);
                result2.Value.ShouldBe(source2.Value * 2);

                // Better-than-nothing check on inline MapperContext caching:
                result1Duration.ShouldBeGreaterThan(result2Duration);
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
        public void ShouldApplyInlineConstantDataSourceExpressions()
        {
            var source1 = new PublicProperty<string> { Value = "Yes" };
            var source2 = new PublicProperty<string> { Value = "No" };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<string>>(c => c
                        .Map("Maybe?")
                        .To(pf => pf.Value));

                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicField<string>>(c => c
                        .Map("Maybe?")
                        .To(pf => pf.Value));

                result1.Value.ShouldBe("Maybe?");
                result2.Value.ShouldBe("Maybe?");
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
        public void ShouldApplyDifferingInlineConstantDataSources()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<int>>(c => c
                        .Map(ctx => ctx.Source.Value + 2)
                        .To(pf => pf.Value));


                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicField<int>>(c => c
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

        #region Helper Members

        private static Expression<Func<IMappingData<PublicProperty<int>, PublicField<int>>, object>> SubtractOne =>
            ctx => ctx.Source.Value - 1;

        private static Expression<Func<IMappingData<PublicProperty<int>, PublicField<int>>, object>> SubtractThree =>
            ctx => ctx.Source.Value - 3;

        #endregion
    }
}
