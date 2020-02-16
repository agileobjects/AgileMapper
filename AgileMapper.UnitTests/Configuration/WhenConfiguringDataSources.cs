namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AgileMapper.Extensions;
    using AgileMapper.Members;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringDataSources
    {
        private int _returnInstanceCount;

        [Fact]
        public void ShouldApplyAConstant()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicProperty<string>>()
                    .Map("Hello there!")
                    .To(x => x.Value);

                var source = new PublicProperty<string> { Value = "Goodbye!" };
                var result = mapper.Map(source).ToANew<PublicProperty<string>>();

                result.Value.ShouldBe("Hello there!");
            }
        }

        [Fact]
        public void ShouldApplyAConstantFromAllSourceTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<PublicProperty<decimal>>()
                    .Map(decimal.MaxValue)
                    .To(x => x.Value);

                var mapNewResult1 = mapper.Map(new PublicField<decimal>()).ToANew<PublicProperty<decimal>>();
                var mapNewResult2 = mapper.Map(new Person()).ToANew<PublicProperty<decimal>>();
                var mapNewResult3 = mapper.Map(new PublicGetMethod<float>(1.0f)).ToANew<PublicProperty<decimal>>();

                mapNewResult1.Value.ShouldBe(decimal.MaxValue);
                mapNewResult2.Value.ShouldBe(decimal.MaxValue);
                mapNewResult3.Value.ShouldBe(decimal.MaxValue);
            }
        }

        [Fact]
        public void ShouldApplyAConstantConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Over<PublicProperty<string>>()
                    .If(ctx => ctx.Target.Value.Length < 5)
                    .Map("Too small!")
                    .To(x => x.Value);

                var source = new PublicProperty<string> { Value = "Replaced" };

                var nonMatchingTarget = new PublicProperty<string> { Value = "This has more than 5 characters" };
                var nonMatchingResult = mapper.Map(source).Over(nonMatchingTarget);

                nonMatchingResult.Value.ShouldBe("Replaced");

                var matchingTarget = new PublicProperty<string> { Value = "Tiny" };
                var matchingResult = mapper.Map(source).Over(matchingTarget);

                matchingResult.Value.ShouldBe("Too small!");
            }
        }

        [Fact]
        public void ShouldApplyAConstantToANestedMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicField<PublicField<PublicField<string>>>>()
                    .Map("Deep!")
                    .To(x => x.Value.Value.Value);

                var source = new PublicProperty<string>();
                var target = new PublicField<PublicField<PublicField<string>>>();
                var result = mapper.Map(source).Over(target);

                result.Value.ShouldNotBeNull();
                result.Value.Value.ShouldNotBeNull();
                result.Value.Value.Value.ShouldNotBeNull("Deep!");
            }
        }

        [Fact]
        public void ShouldApplyASourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicProperty<Guid>>()
                    .Map(ctx => ctx.Source.Id)
                    .To(x => x.Value);

                var source = new Person { Id = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicProperty<Guid>>();

                result.Value.ShouldBe(source.Id);
            }
        }

        [Fact]
        public void ShouldApplyASourceInterfaceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<IPublicInterface<string>>()
                    .To<PublicProperty<string>>()
                    .Map(ctx => ctx.Source.Value + "!")
                    .To(pp => pp.Value);

                var source = new PublicImplementation<string> { Value = "Impl" };
                var result = mapper.Map(source).ToANew<PublicProperty<string>>();

                result.Value.ShouldBe("Impl!");
            }
        }

        [Fact]
        public void ShouldApplyAnInterfaceMemberBetweenInterfaces()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<IPublicInterface<int>>()
                    .To<IPublicInterface>()
                    .Map(ctx => ctx.Source.Value * 2)
                    .To(pp => pp.Value);

                var source = new PublicOtherImplementation<int> { Value = 2 };
                var target = new PublicImplementation<long> { Value = 2L };

                mapper.Map(source).Over((IPublicInterface)target);

                target.Value.ShouldBe(4L);
            }
        }

        [Fact]
        public void ShouldApplyMultipleSourceMembersBySourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicProperty<string>>()
                    .Map((c, t) => c.Name)
                    .To(x => x.Value);

                mapper.WhenMapping
                    .From<Customer>()
                    .To<PublicProperty<string>>()
                    .Map(c => c.Discount, pp => pp.Value);

                var personSource = new Person { Name = "Wilma" };
                var personResult = mapper.Map(personSource).ToANew<PublicProperty<string>>();

                personResult.Value.ShouldBe("Wilma");

                var customerSource = new Customer { Name = "Betty", Discount = 10.0m };
                var customerResult = mapper.Map(customerSource).ToANew<PublicProperty<string>>();

                customerResult.Value.ShouldBe("10.0");
            }
        }

        [Fact]
        public void ShouldAllowConditionTypeTestsIfSourceIsAnInterface()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var sourceData = default(Issue146.Source.Data);

                mapper.WhenMapping
                    .From<Issue146.Source.Container>().To<Issue146.Target.Cont>()
                    .Map(s => s.Empty, t => t.Info);

                mapper.WhenMapping
                    .From<Issue146.Source.IEmpty>().To<Issue146.Target.Data>()
                    .After
                    .MappingEnds
                    .If(ctx => ctx.Source is Issue146.Source.Data)
                    .Call(ctx => sourceData = (Issue146.Source.Data)ctx.Source);

                var source = new Issue146.Source.Container("xxx");
                var result = mapper.Map(source).ToANew<Issue146.Target.Cont>();

                result.ShouldNotBeNull();
                sourceData.ShouldNotBeNull();
                sourceData.ShouldBeSameAs(source.Empty);
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredExpression()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .Map(ctx => ctx.Source.Name + ", " + ctx.Source.AddressLine1)
                    .To(x => x.Address.Line1);

                var source = new PersonViewModel { Name = "Fred", AddressLine1 = "Lala Land" };
                var result = mapper.Map(source).ToANew<Person>();

                result.Address.Line1.ShouldBe("Fred, Lala Land");
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

        [Fact]
        public void ShouldApplyASourceMemberConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicGetMethod<int>>()
                    .ToANew<PublicSetMethod<string>>()
                    .If(ctx => ctx.Source.GetValue() % 2 == 0)
                    .Map(ctx => ctx.Source.GetValue())
                    .To<string>(x => x.SetValue);

                var matchingSource = new PublicGetMethod<int>(6);
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicSetMethod<string>>();

                matchingResult.Value.ShouldBe("6");

                var nonMatchingSource = new PublicGetMethod<int>(7);
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicSetMethod<string>>();

                nonMatchingResult.Value.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldNotOverwriteATargetWithNoMatchingSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<int, string>>()
                    .To<PublicField<string>>()
                    .If(ctx => ctx.Source.Value1 > 100)
                    .Map((ptf, pf) => ptf.Value1)
                    .To(pf => pf.Value);

                var source = new PublicTwoFieldsStruct<int, string> { Value1 = 50 };
                var target = new PublicField<string> { Value = "Value!" };

                mapper.Map(source).Over(target);

                target.Value.ShouldBe("Value!");
            }
        }

        [Fact]
        public void ShouldApplyMultipleSourceMembersConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Customer>()
                    .To<PublicProperty<string>>()
                    .If((c, t) => c.Name.Length < 5)
                    .Map((c, t) => c.Id)
                    .To(x => x.Value);

                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicProperty<string>>()
                    .If((p, t) => p.Name.Length > 8)
                    .Map((p, t) => p.Title)
                    .To(x => x.Value);

                var shortNameSource = new Customer { Id = Guid.NewGuid(), Name = "Fred", Title = Title.Count };
                var shortNameResult = mapper.Map(shortNameSource).ToANew<PublicProperty<string>>();

                shortNameResult.Value.ShouldBe(shortNameSource.Id.ToString());

                var midNameSource = new Customer { Id = Guid.NewGuid(), Name = "Frankie", Title = Title.Count };
                var midNameResult = mapper.Map(midNameSource).ToANew<PublicProperty<string>>();

                midNameResult.Value.ShouldBeDefault();

                var longNameSource = new Customer { Id = Guid.NewGuid(), Name = "Bartholomew", Title = Title.Duke };
                var longNameResult = mapper.Map(longNameSource).ToANew<PublicProperty<string>>();

                longNameResult.Value.ShouldBe("Duke");
            }
        }

        [Fact]
        public void ShouldApplyConditionalAndUnconditionalDataSourcesInOrder()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicProperty<string>>()
                    .Map((p, t) => p.Title + " " + p.Name)
                    .To(x => x.Value);

                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicProperty<string>>()
                    .If((p, t) => p.Title == default(Title) || !Enum.IsDefined(p.Title.GetType(), p.Title))
                    .Map((p, t) => p.Name)
                    .To(x => x.Value);

                var undefinedTitleSource = new Person { Title = (Title)1234, Name = "Bill" };
                var undefinedTitleResult = mapper.Map(undefinedTitleSource).ToANew<PublicProperty<string>>();

                undefinedTitleResult.Value.ShouldBe("Bill");

                var definedTitleSource = new Customer { Title = Title.Duke, Name = "Bart" };
                var definedTitleResult = mapper.Map(definedTitleSource).ToANew<PublicProperty<string>>();

                definedTitleResult.Value.ShouldBe("Duke Bart");
            }
        }

        [Fact]
        public void ShouldHandleANullMemberInACondition()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicField<long>>()
                    .If(ctx => ctx.Source.Value.Length % 2 == 0)
                    .Map(ctx => int.Parse(ctx.Source.Value) / 2)
                    .To(x => x.Value);

                var matchingSource = new PublicProperty<string> { Value = "20" };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<long>>();

                matchingResult.Value.ShouldBe(10L);

                var nullSource = new PublicProperty<string> { Value = null };
                var nullSourceResult = mapper.Map(nullSource).ToANew<PublicField<long>>();

                nullSourceResult.Value.ShouldBeDefault();
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/176
        [Fact]
        public void ShouldHandleANullConfiguredStaticFactoryMethodResult()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.ThrowIfAnyMappingPlanIsIncomplete();

                mapper.WhenMapping
                    .From<Address>()
                    .To<PublicProperty<PublicProperty<string>>>()
                    .Map((pf, _) => ReturnNull<PublicProperty<string>>())
                    .To(pp => pp.Value);

                var source = new Address();
                var result = mapper.Map(source).ToANew<PublicProperty<PublicProperty<string>>>();

                result.ShouldNotBeNull().Value.ShouldBeNull();
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/176
        [Fact]
        public void ShouldCacheAConfiguredInstanceFactoryMethodResult()
        {
            using (var mapper = Mapper.CreateNew())
            {
                _returnInstanceCount = 0;

                mapper.WhenMapping
                    .From<Address>()
                    .To<PublicProperty<PublicProperty<string>>>()
                    .Map((s, t) => ReturnInstance<PublicProperty<string>>())
                    .To(pp => pp.Value);

                var source = new Address();
                var result = mapper.Map(source).ToANew<PublicProperty<PublicProperty<string>>>();

                result.ShouldNotBeNull().Value.ShouldNotBeNull();
                _returnInstanceCount.ShouldBe(1);
            }
        }

        [Fact]
        public void ShouldWrapAnExceptionThrownInACondition()
        {
            Should.Throw<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicField<int>>()
                        .If((p, t) => int.Parse(p.Name) > 0)
                        .Map(ctx => ctx.Source.Name)
                        .To(x => x.Value);

                    var source = new Person { Name = "CantParseThis" };

                    mapper.Map(source).ToANew<PublicField<int>>();
                }
            });
        }

        [Fact]
        public void ShouldWrapADataSourceException()
        {
            Should.Throw<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicSetMethod<int>>()
                        .Map((p, psm) => int.Parse(p.Name))
                        .To<int>(psm => psm.SetValue);

                    var source = new Person { Name = "NotGonnaWork" };

                    mapper.Map(source).ToANew<PublicSetMethod<int>>();
                }
            });
        }

        [Fact]
        public void ShouldApplyASourceExpressionFromAllSourceTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .OnTo<Person>()
                    .Map(x => x.Source.GetType().Name)
                    .To(x => x.Name);

                var personResult = mapper.Map(new Person()).OnTo(new Person());
                var customerResult = mapper.Map(new Customer()).OnTo(new Person());

                personResult.Name.ShouldBe("Person");
                customerResult.Name.ShouldBe("Customer");
            }
        }

        [Fact]
        public void ShouldApplyASourceMemberInARootEnumerable()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicField<string>>()
                    .Map(p => p.Name, pf => pf.Value);

                var source = new[] { new Person { Name = "Mr Thomas" } };
                var result = mapper.Map(source).ToANew<List<PublicField<string>>>();

                source.ShouldBe(result.Select(r => r.Value), p => p.Name);
            }
        }

        [Fact]
        public void ShouldApplyAParentSourceMemberToADerivedSourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Map(ctx => ctx.Source.Id)
                    .To(x => x.Name);

                var source = new Customer { Id = Guid.NewGuid(), Address = new Address() };
                var result = mapper.Map(source).ToANew<PersonViewModel>();

                result.Name.ShouldBe(source.Id.ToString());
            }
        }

        [Fact]
        public void ShouldApplyASourceMemberToABaseSourceTypeOnly()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Customer>().ButNotDerivedTypes
                    .To<CustomerViewModel>()
                    .Map(c => c.Id, x => x.Name);

                var source = new Customer[]
                {
                    new Customer { Id = Guid.NewGuid(), Address = new Address() },
                    new MysteryCustomer { Id = Guid.NewGuid(), Name = "Whaaaat?!?" },
                };

                var result = mapper.Map(source).ToANew<IEnumerable<CustomerViewModel>>();

                result.Count().ShouldBe(2);
                result.First().Name.ShouldBe(source.First().Id.ToString());
                result.Second().Name.ShouldBe("Whaaaat?!?");
            }
        }

        [Fact]
        public void ShouldApplyAnExpression()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<int>>()
                    .To<PublicField<long>>()
                    .Map(ctx => ctx.Source.Value * 10)
                    .To(x => x.Value);

                var source = new PublicProperty<int> { Value = 123 };
                var result = mapper.Map(source).ToANew<PublicField<long>>();

                result.Value.ShouldBe(source.Value * 10);
            }
        }

        [Fact]
        public void ShouldApplyAnExpressionToAMemberEnumerable()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Customer>()
                    .To<PublicSetMethod<string>>()
                    .Map((s, t, i) => (i + 1) + ": " + s.Name)
                    .To<string>(x => x.SetValue);

                var source = new PublicProperty<Customer[]> { Value = new[] { new Customer { Name = "Mr Thomas" } } };
                var result = mapper.Map(source).ToANew<PublicField<IEnumerable<PublicSetMethod<string>>>>();

                result.Value.ShouldHaveSingleItem();
                result.Value.First().Value.ShouldBe("1: Mr Thomas");
            }
        }

        [Fact]
        public void ShouldApplyAnExpressionToAnArray()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicField<int[]>>()
#if FEATURE_STRINGSPLIT_OPTIONS
                    .Map(ctx => ctx.Source.Value.Split(':', StringSplitOptions.None))
#else
                    .Map(ctx => ctx.Source.Value.Split(':'))
#endif
                    .To(x => x.Value);

                var source = new PublicProperty<string> { Value = "8:7:6:5" };
                var result = mapper.Map(source).ToANew<PublicField<int[]>>();

                result.Value.ShouldBe(8, 7, 6, 5);
            }
        }

        [Fact]
        public void ShouldApplyAnExpressionToAMemberNonGenericEnumerableConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Customer>()
                    .ToANew<PersonViewModel>()
                    .If((c, pvm, i) => i > 0)
                    .Map((c, pvm, i) => c.Name + " (" + i + ")")
                    .To(pvm => pvm.Name);

                var source = new PublicProperty<IEnumerable>
                {
                    Value = new[]
                    {
                        new Customer { Name = "Miss G" },
                        new Person { Name = "Mrs G" },
                        new Customer { Name = "Lady G" }
                    }
                };
                var result = mapper.Map(source).ToANew<PublicField<IEnumerable<PersonViewModel>>>();

                result.Value.Count().ShouldBe(3);
                result.Value.First().Name.ShouldBe("Miss G");
                result.Value.Second().Name.ShouldBe("Mrs G");
                result.Value.Third().Name.ShouldBe("Lady G (2)");
            }
        }

        [Fact]
        public void ShouldApplyAnExpressionWithMultipleNestedSourceMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Map((p, pvm) => p.Address.Line1 + ", " + p.Address.Line2)
                    .To(x => x.AddressLine1);

                var source = new Person { Address = new Address { Line1 = "One", Line2 = "Two" } };
                var result = mapper.Map(source).ToANew<PersonViewModel>();

                result.AddressLine1.ShouldBe("One, Two");
            }
        }

        [Fact]
        public void ShouldApplyAnExpressionToADerivedTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .Map(ctx => ctx.Source.Name + "!")
                    .To(x => x.Name);

                var source = new PersonViewModel { Name = "Harry" };
                var result = mapper.Map(source).ToANew<Customer>();

                result.Name.ShouldBe(source.Name + "!");
            }
        }

        [Fact]
        public void ShouldApplyAnExpressionToARootCollectionConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Customer>()
                    .Over<PersonViewModel>()
                    .If((c, pvm) => c.Discount > 0.5m)
                    .Map((c, pvm) => pvm.Name + $" (Big discount! {c.Discount})")
                    .To(pvm => pvm.Name);

                var customerId = Guid.NewGuid();

                var source = new[]
                {
                    new Customer { Id = customerId, Name = "Mr Thomas", Discount = 0.60m },
                    new Person { Name = "Mrs Edison" }
                };

                var target = new Collection<PersonViewModel>
                {
                    new PersonViewModel { Id = customerId, Name = "Mrs Thomas" }
                };

                var result = mapper.Map(source).Over(target);

                result.Count.ShouldBe(2);
                result.First().Name.ShouldBe("Mrs Thomas (Big discount! 0.60)");
                result.Second().Name.ShouldBe("Mrs Edison");
            }
        }

        [Fact]
        public void ShouldApplyAFunction()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Func<IMappingData<Person, PersonViewModel>, string> combineAddressLine1 =
                    ctx => ctx.Source.Name + ", " + ctx.Source.Address.Line1;

                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Map(combineAddressLine1)
                    .To(pvm => pvm.AddressLine1);

                var source = new Person { Name = "Frank", Address = new Address { Line1 = "Over there" } };
                var result = mapper.Map(source).ToANew<PersonViewModel>();

                result.AddressLine1.ShouldBe("Frank, Over there");
            }
        }

        [Fact]
        public void ShouldApplyASourceComplexTypeMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new { Person = new Person { Name = "Anon" } };

                mapper.WhenMapping
                    .From(source)
                    .Over<PublicProperty<Person>>()
                    .Map((s, t) => s.Person)
                    .To(pp => pp.Value);

                var target = new PublicProperty<Person> { Value = new Person { Name = "Someone" } };
                var result = mapper.Map(source).Over(target);

                result.Value.ShouldNotBeSameAs(source.Person);
                result.Value.Name.ShouldBe("Anon");
            }
        }

        [Fact]
        public void ShouldWrapAComplexTypeDataSourceException()
        {
            var mappingEx = Should.Throw<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    var source = new { PersonVm = new PersonViewModel { Name = "Andy", AddressLine1 = "Blah" } };

                    mapper
                        .WhenMapping
                        .From(source)
                        .Over<PublicField<Person>>()
                        .Map((s, pf) => s.PersonVm)
                        .To(pf => pf.Value)
                        .And
                        .Before
                        .CreatingInstancesOf<Address>()
                        .Call(ctx => { throw new InvalidOperationException("I don't like addresses"); });

                    var target = new PublicField<Person> { Value = new Person { Name = "Someone" } };

                    mapper.Map(source).Over(target);
                }
            });

            mappingEx.Message.ShouldContain("-> PublicField<Person>");
            mappingEx.InnerException.ShouldNotBeNull();

            // ReSharper disable once PossibleNullReferenceException
            mappingEx.InnerException.Message.ShouldContain("<PersonViewModel>.PersonVm -> PublicField<Person>");
        }

        [Fact]
        public void ShouldApplyASourceNestedComplexType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<PublicField<Person>>>()
                    .Over<PublicProperty<Person>>()
                    .Map((s, t) => s.Value.Value)
                    .To(pp => pp.Value);

                var source = new PublicProperty<PublicField<Person>>
                {
                    Value = new PublicField<Person>
                    {
                        Value = new Customer { Name = "Someone else" }
                    }
                };
                var target = new PublicProperty<Person> { Value = new Person { Name = "Someone" } };
                var result = mapper.Map(source).Over(target);

                result.Value.ShouldNotBeSameAs(source.Value.Value);
                result.Value.Name.ShouldBe("Someone else");
            }
        }

        [Fact]
        public void ShouldHandleANullNestedComplexTypeSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<PublicField<Person>>>()
                    .Over<PublicProperty<Person>>()
                    .Map((s, t) => s.Value.Value)
                    .To(pp => pp.Value);

                var source = new PublicProperty<PublicField<Person>>
                {
                    Value = new PublicField<Person> { Value = null }
                };
                var target = new PublicProperty<Person> { Value = new Person { Name = "Someone" } };
                var result = mapper.Map(source).Over(target);

                result.Value.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldApplyAComplexTypeMemberConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new
                {
                    Value = new PublicField<int> { Value = 0 },
                    Value1 = new PublicField<int> { Value = 1 },
                    Value2 = new PublicField<int> { Value = 2 }
                };

                mapper.WhenMapping
                    .From(source)
                    .Over<PublicField<PublicField<int>>>()
                    .If((s, t) => t.Value.Value > 2)
                    .Map(ctx => ctx.Source.Value1)
                    .To(t => t.Value);

                mapper.WhenMapping
                    .From(source)
                    .Over<PublicField<PublicField<int>>>()
                    .If((s, t) => t.Value.Value < 0)
                    .Map(ctx => ctx.Source.Value2)
                    .To(t => t.Value);

                var value1MatchingTarget = new PublicField<PublicField<int>> { Value = new PublicField<int> { Value = 3 } };
                var value1MatchingResult = mapper.Map(source).Over(value1MatchingTarget);

                value1MatchingResult.Value.Value.ShouldBe(1);

                var value2MatchingTarget = new PublicField<PublicField<int>> { Value = new PublicField<int> { Value = -1 } };
                var value2MatchingResult = mapper.Map(source).Over(value2MatchingTarget);

                value2MatchingResult.Value.Value.ShouldBe(2);

                var defaultTarget = new PublicField<PublicField<int>> { Value = new PublicField<int> { Value = 1 } };
                var defaultResult = mapper.Map(source).Over(defaultTarget);

                defaultResult.Value.Value.ShouldBe(0);
            }
        }

        [Fact]
        public void ShouldApplyAComplexTypeEnumerableMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new { People = new[] { new Person { Name = "Jimmy" } } };

                mapper.WhenMapping
                    .From(source)
                    .To<PublicProperty<IEnumerable<Person>>>()
                    .Map(s => s.People, pp => pp.Value);

                var result = mapper.Map(source).ToANew<PublicProperty<IEnumerable<Person>>>();

                result.Value.First().Name.ShouldBe("Jimmy");
            }
        }

        [Fact]
        public void ShouldApplyAComplexTypeEnumerableMemberConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new
                {
                    Value = new PublicField<int> { Value = 0 },
                    Value1 = new PublicField<int> { Value = 1 },
                    Value2 = new PublicField<int> { Value = 2 }
                };

                mapper.WhenMapping
                    .From(source)
                    .Over<PublicField<PublicField<int>>>()
                    .If((s, t) => t.Value.Value > 2)
                    .Map(ctx => ctx.Source.Value1)
                    .To(t => t.Value);

                mapper.WhenMapping
                    .From(source)
                    .Over<PublicField<PublicField<int>>>()
                    .If((s, t) => t.Value.Value < 0)
                    .Map(ctx => ctx.Source.Value2)
                    .To(t => t.Value);

                var value1MatchingTarget = new PublicField<PublicField<int>> { Value = new PublicField<int> { Value = 3 } };
                var value1MatchingResult = mapper.Map(source).Over(value1MatchingTarget);

                value1MatchingResult.Value.Value.ShouldBe(1);

                var value2MatchingTarget = new PublicField<PublicField<int>> { Value = new PublicField<int> { Value = -1 } };
                var value2MatchingResult = mapper.Map(source).Over(value2MatchingTarget);

                value2MatchingResult.Value.Value.ShouldBe(2);

                var defaultTarget = new PublicField<PublicField<int>> { Value = new PublicField<int> { Value = 1 } };
                var defaultResult = mapper.Map(source).Over(defaultTarget);

                defaultResult.Value.Value.ShouldBe(0);
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/113
        [Fact]
        public void ShouldApplyAComplexToSimpleTypeEnumerableProjection()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<PublicField<int>[]>>()
                    .To<PublicField<int[]>>()
                    .Map(
                        pfpfi => pfpfi.Value.Select(v => v.Value),
                        pfi => pfi.Value);

                var source = new PublicField<PublicField<int>[]>
                {
                    Value = new[]
                    {
                        new PublicField<int> { Value = 1 },
                        new PublicField<int> { Value = 2 },
                        new PublicField<int> { Value = 3 }
                    }
                };

                var result = mapper.Map(source).ToANew<PublicField<int[]>>();

                result.Value.ShouldNotBeEmpty();
                result.Value.ShouldBe(1, 2, 3);
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

        [Fact]
        public void ShouldApplyASourceAndTargetFunction()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Func<PersonViewModel, Address, string> combineAddressLine1 =
                    (pvm, a) => pvm.Name + ", " + pvm.AddressLine1;

                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Address>()
                    .Map(combineAddressLine1)
                    .To(p => p.Line1);

                var source = new PersonViewModel { Name = "Francis", AddressLine1 = "Over here" };
                var result = mapper.Map(source).ToANew<Person>();

                result.Address.Line1.ShouldBe("Francis, Over here");
            }
        }

        [Fact]
        public void ShouldApplyASourceTargetAndIndexFunction()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Func<PersonViewModel, Person, int?, string> combineAddressLine1 =
                    (pvm, p, i) => $"{i}: {pvm.Name}, {pvm.AddressLine1}";

                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .Map(combineAddressLine1)
                    .To(p => p.Address.Line1);

                var source = new[] { new PersonViewModel { Name = "Jane", AddressLine1 = "Over here!" } };
                var result = mapper.Map(source).ToANew<Person[]>();

                result.ShouldHaveSingleItem();
                result.First().Address.Line1.ShouldBe("0: Jane, Over here!");
            }
        }

        [Fact]
        public void ShouldMapAFunction()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Func<Person, string> combineAddressLine1 =
                    p => p.Name + ", " + p.Address.Line1;

                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicProperty<Func<Person, string>>>()
                    .MapFunc(combineAddressLine1)
                    .To(x => x.Value);

                var source = new Person { Name = "Frank", Address = new Address { Line1 = "Over there" } };
                var target = mapper.Map(source).Over(new PublicProperty<Func<Person, string>>());

                target.Value.ShouldBe(combineAddressLine1);
            }
        }

        [Fact]
        public void ShouldApplyAnExpressionUsingExtensionMethods()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<Customer>()
                    .Map(ctx => (decimal)ctx.Source.Name.First())
                    .To(x => x.Discount);

                var source = new Person { Name = "Bob" };
                var result = mapper.Map(source).ToANew<Customer>();

                result.Discount.ShouldBe((decimal)source.Name.First());
            }
        }

        [Fact]
        public void ShouldRestrictConfiguredConstantApplicationBySourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicSetMethod<int>>()
                    .Map(12345)
                    .To<int>(x => x.SetValue);

                var matchingSource = new PublicField<int> { Value = 938726 };
                var matchingSourceResult = mapper.Map(matchingSource).ToANew<PublicSetMethod<int>>();

                var nonMatchingSource = new PublicProperty<int> { Value = matchingSource.Value };
                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicSetMethod<int>>();

                matchingSourceResult.Value.ShouldBe(12345);
                nonMatchingSourceResult.Value.ShouldBe(nonMatchingSource.Value);
            }
        }

        [Fact]
        public void ShouldRestrictConfiguredConstantApplicationByTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicProperty<int>>()
                    .Map(98765)
                    .To(x => x.Value);

                var source = new PublicField<int> { Value = 938726 };
                var matchingTargetResult = mapper.Map(source).ToANew<PublicProperty<int>>();
                var nonMatchingTargetResult = mapper.Map(source).ToANew<PublicSetMethod<int>>();

                matchingTargetResult.Value.ShouldBe(98765);
                nonMatchingTargetResult.Value.ShouldBe(source.Value);
            }
        }

        [Fact]
        public void ShouldRestrictConfigurationApplicationByMappingRuleSet()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<int>>()
                    .ToANew<PublicProperty<long>>()
                    .Map(9999)
                    .To(x => x.Value);

                var source = new PublicProperty<int> { Value = 64738 };

                var createResult = mapper.Map(source).ToANew<PublicProperty<long>>();
                var updateResult = mapper.Map(source).Over(new PublicProperty<long>());

                createResult.Value.ShouldBe(9999);
                updateResult.Value.ShouldBe(source.Value);
            }
        }

        [Fact]
        public void ShouldDifferentiateConfigurationByMappingRuleSet()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<int>>()
                    .ToANew<PublicProperty<long>>()
                    .Map(999)
                    .To(x => x.Value);

                mapper.WhenMapping
                    .From<PublicProperty<int>>()
                    .Over<PublicProperty<long>>()
                    .Map(999)
                    .To(x => x.Value);

                var source = new PublicProperty<int> { Value = 6478 };

                var createResult = mapper.Map(source).ToANew<PublicProperty<long>>();
                var updateResult = mapper.Map(source).Over(new PublicProperty<long>());

                createResult.Value.ShouldBe(999);
                updateResult.Value.ShouldBe(999);
            }
        }

        [Fact]
        public void ShouldConfigureDifferentRuntimeTypedDataSources()
        {
            var source = new PublicTwoFields<object, object>
            {
                Value1 = new PublicField<object> { Value = 1 },
                Value2 = new PublicProperty<object> { Value = 2 },
            };

            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<object, object>>()
                    .To<PublicField<PublicField<object>>>()
                    .If(ctx => ((PublicField<object>)ctx.Source.Value1).Value.ToString() != "1")
                    .Map(ctx => (PublicField<object>)ctx.Source.Value1)
                    .To(pf => pf.Value);

                mapper.WhenMapping
                    .From<PublicTwoFields<object, object>>()
                    .To<PublicField<PublicField<object>>>()
                    .Map(ctx => (PublicProperty<object>)ctx.Source.Value2)
                    .To(pf => pf.Value);

                var result = mapper.Map(source).ToANew<PublicField<PublicField<object>>>();

                result.Value.Value.ShouldBe(2);
            }
        }

        [Fact]
        public void ShouldSupportMultipleDivergedMappers()
        {
            using (var mapper1 = Mapper.CreateNew())
            using (var mapper2 = Mapper.CreateNew())
            {
                mapper1.WhenMapping
                    .From<PublicField<string>>()
                    .ToANew<PublicProperty<string>>()
                    .Map((pf, pp) => pf.Value + "?")
                    .To(pp => pp.Value);

                mapper2.WhenMapping
                    .From<PublicField<string>>()
                    .ToANew<PublicProperty<string>>()
                    .Map((pf, pp) => pf.Value + "!")
                    .To(pp => pp.Value);

                var source = new PublicField<string> { Value = "Diverged" };
                var result1 = mapper1.Map(source).ToANew<PublicProperty<string>>();
                var result2 = mapper2.Map(source).ToANew<PublicProperty<string>>();

                result1.Value.ShouldBe("Diverged?");
                result2.Value.ShouldBe("Diverged!");
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/14
        [Fact]
        public void ShouldAllowIdAndIdentifierConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<int, int>>()
                    .To<IdTester>()
                    .Map((ptf, id) => ptf.Value1)
                    .To(id => id.ClassId)
                    .And
                    .Map((ptf, id) => ptf.Value2)
                    .To(id => id.ClassIdentifier);

                var source = new PublicTwoFields<int, int>
                {
                    Value1 = 123,
                    Value2 = 987
                };

                var result = mapper.Map(source).ToANew<IdTester>();

                result.ClassId.ShouldBe(123);
                result.ClassIdentifier.ShouldBe(987);
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/146
        [Fact]
        public void ShouldApplyANestedSourceInterfaceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue146.Source.Container>().To<Issue146.Target.Cont>()
                    .Map(ctx => ctx.Source.Empty).To(tgt => tgt.Info);

                var source = new Issue146.Source.Container("12321") { Name = "input" };
                var result = mapper.Map(source).ToANew<Issue146.Target.Cont>();

                result.ShouldNotBeNull();
                result.Name.ShouldBe("input");
                result.Info.ShouldNotBeNull();
                result.Info.Id.ShouldBe("12321");

                // Source has a .Value member, but we don't runtime-type interfaces
                result.Info.Value.ShouldBeNull();
            }
        }

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

                var source = new MysteryCustomer { Name = "Nelly", Address = default(Address) };

                var result = mapper.Map(source).ToANew<PublicTwoFields<string, string>>();

                result.Value1.ShouldBe("Nelly");
                result.Value2.ShouldBeNull();
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

        // See https://github.com/agileobjects/AgileMapper/issues/68
        [Fact]
        public void ShouldSupportConfiguringARootSourceUsingMappingContext()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Model>()
                    .To<ModelDto>()
                    .Map(ctx => ctx.Source.Statistics)
                    .ToTarget();

                var source = new Model
                {
                    SomeOtherProperties = "jyutrgf",
                    Statistics = new Statistics
                    {
                        Ranking = 0.5f,
                        SomeOtherRankingStuff = "uityjtgrf"
                    }
                };

                var result = mapper.Map(source).ToANew<ModelDto>();

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

        // See https://github.com/agileobjects/AgileMapper/issues/125
        [Fact]
        public void ShouldHandleDeepNestedRuntimeTypedMembersWithACachedMappingPlan()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue125.Source.ParamSet>().To<Issue125.Target.ParamSet>()
                    .Map(ctxt => ctxt.Source.ParamObj)
                    .ToCtor<Issue125.Target.ParamObj>()
                    .And.Ignore(t => t.ParamObj);

                // Bug only happens when the mapping plan is cached up-front
                mapper
                    .GetPlanFor<Issue125.Source.ParamSet>()
                    .ToANew<Issue125.Target.ParamSet>();

                var source = new Issue125.Source.ParamSet
                {
                    ParamObj = new Issue125.Source.ParamObj
                    {
                        Name = "Test PO",
                        Collection = new Issue125.Source.ParamCol
                        {
                            Children =
                            {
                                new Issue125.Source.ParamValue
                                {
                                    Definition = Issue125.Source.ParamDef.Create(),
                                    Values = { 1, 2, 3, 4, 5, 6 }
                                }
                            }
                        }
                    },
                    ParamValues =
                    {
                        new Issue125.Source.ParamValue
                        {
                            Definition = Issue125.Source.ParamDef.Create(),
                            Values = { 2, 4, 6, 8, 10, 12 }
                        }
                    }
                };

                var result = source.MapUsing(mapper).ToANew<Issue125.Target.ParamSet>();

                result.ShouldNotBeNull();

                result.ParamObj.ShouldNotBeNull();
                result.ParamObj.Name.ShouldBe("Test PO");
                result.ParamObj.Collection.ShouldNotBeNull();

                result.ParamObj.Collection.ParamObj.ShouldBeNull();
                result.ParamObj.Collection.Children.ShouldNotBeNull();
                result.ParamObj.Collection.Children.ShouldHaveSingleItem();
                result.ParamObj.Collection.Children.First().Definition.ShouldNotBeNull();
                result.ParamObj.Collection.Children.First().Definition.Identification.ShouldBe("Test ParamDef");
                result.ParamObj.Collection.Children.First().Definition.Default.ShouldBe(42);
                result.ParamObj.Collection.Children.First().Definition.Min.ShouldBe(0);
                result.ParamObj.Collection.Children.First().Definition.Max.ShouldBe(100);
                result.ParamObj.Collection.Children.First().Definition.Type.ShouldBe("Integer");
                result.ParamObj.Collection.Children.First().Values.ShouldNotBeNull();
                result.ParamObj.Collection.Children.First().Values.ShouldBe(1, 2, 3, 4, 5, 6);

                result.ParamValues.ShouldNotBeNull();
                result.ParamValues.ShouldHaveSingleItem();
                result.ParamValues.First().Definition.ShouldNotBeNull();
                result.ParamValues.First().Definition.Identification.ShouldBe("Test ParamDef");
                result.ParamValues.First().Definition.Default.ShouldBe(42);
                result.ParamValues.First().Definition.Min.ShouldBe(0);
                result.ParamValues.First().Definition.Max.ShouldBe(100);
                result.ParamValues.First().Definition.Type.ShouldBe("Integer");
                result.ParamValues.First().Values.ShouldNotBeNull();
                result.ParamValues.First().Values.ShouldBe(2, 4, 6, 8, 10, 12);

                result.ParamObj.Collection.Children.First().Definition.ShouldNotBeSameAs(
                    result.ParamValues.First().Definition);
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

        #region Helper Classes

        internal class IdTester
        {
            public int ClassId { get; set; }

            public int ClassIdentifier { get; set; }
        }

        internal class Statistics
        {
            public float Ranking { get; set; }

            public string SomeOtherRankingStuff { get; set; }
        }

        internal class Model
        {
            public string SomeOtherProperties { get; set; }

            public Statistics Statistics { get; set; }
        }

        internal class ModelDto
        {
            public string SomeOtherProperties { get; set; }

            public float Ranking { get; set; }

            public string SomeOtherRankingStuff { get; set; }
        }

        // ReSharper disable CollectionNeverQueried.Local
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
        internal static class Issue125
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public static class Source
            {
                public class ParamSet
                {
                    public ParamSet()
                    {
                        ParamValues = new List<ParamValue>();
                    }

                    public ParamObj ParamObj { get; set; }

                    public IList<ParamValue> ParamValues { get; }
                }

                public class ParamObj
                {
                    public string Name { get; set; }

                    public ParamCol Collection { get; set; }
                }

                public class ParamCol
                {
                    public ParamCol()
                    {
                        Children = new List<ParamValue>();
                    }

                    public IList<ParamValue> Children { get; }
                }

                public class ParamValue
                {
                    public ParamValue()
                    {
                        Values = new List<object>();
                    }

                    public ParamDef Definition { get; set; }

                    public IList<object> Values { get; }
                }

                public class ParamDef
                {
                    public static ParamDef Create()
                    {
                        return new ParamDef
                        {
                            Identification = "Test ParamDef",
                            Default = 42,
                            Min = 0,
                            Max = 100,
                            Type = "Integer"
                        };
                    }

                    public object Default { get; set; }

                    public object Min { get; set; }

                    public object Max { get; set; }

                    public string Type { get; set; }

                    public string Identification { get; set; }
                }
            }
            // ReSharper restore UnusedAutoPropertyAccessor.Local

            // ReSharper disable ClassNeverInstantiated.Local
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public static class Target
            {
                public class ParamSet
                {
                    public ParamSet(ParamObj po)
                    {
                        ParamObj = po;
                        ParamValues = new List<ParamValue>();
                    }

                    public ParamObj ParamObj { get; set; }

                    public IList<ParamValue> ParamValues { get; }
                }

                public class ParamObj
                {
                    public string Name { get; set; }

                    public ParamCol Collection { get; set; }
                }

                public class ParamCol
                {
                    public ParamCol()
                    {
                        Children = new List<ParamValue>();
                    }

                    public ParamObj ParamObj { get; set; }

                    public IList<ParamValue> Children { get; }
                }

                public class ParamValue
                {
                    public ParamValue()
                    {
                        Values = new List<object>();
                    }

                    public ParamDef Definition { get; set; }

                    public IList<object> Values { get; }
                }

                public class ParamDef
                {
                    public object Default { get; set; }

                    public object Min { get; set; }

                    public object Max { get; set; }

                    public string Type { get; set; }

                    public string Identification { get; set; }
                }
            }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
            // ReSharper restore ClassNeverInstantiated.Local
        }
        // ReSharper restore AutoPropertyCanBeMadeGetOnly.Local
        // ReSharper restore MemberCanBePrivate.Local
        // ReSharper restore CollectionNeverQueried.Local

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

        internal static class Issue146
        {
            public static class Source
            {
                public interface IData
                {
                    string Id { get; set; }
                }

                public interface IEmpty : IData { }

                public class Data : IEmpty
                {
                    public string Id { get; set; }

                    public string Value => "Data.Value!";
                }

                public class Container
                {
                    public Container(string infoId)
                    {
                        Empty = new Data { Id = infoId };
                    }

                    public string Name { get; set; }

                    public IEmpty Empty { get; }
                }
            }

            public static class Target
            {
                public class Data
                {
                    public string Id { get; set; }

                    public string Value { get; set; }
                }

                public class Cont
                {
                    public Data Info { get; set; }

                    public string Name { get; set; }
                }
            }
        }

        internal static T ReturnNull<T>()
            where T : class
        {
            return null;
        }

        internal T ReturnInstance<T>()
            where T : class, new()
        {
            ++_returnInstanceCount;

            return new T();
        }

        #endregion
    }
}
