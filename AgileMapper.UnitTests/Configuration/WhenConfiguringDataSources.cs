namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AgileMapper.Members;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDataSources
    {
        [Fact]
        public void ShouldApplyAConfiguredConstant()
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
        public void ShouldApplyAConfiguredConstantFromAllSourceTypes()
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
        public void ShouldConditionallyApplyAConfiguredConstant()
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
        public void ShouldApplyAConfiguredConstantToANestedMember()
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
        public void ShouldApplyAConfiguredMember()
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
        public void ShouldApplyMultipleConfiguredMembersBySourceType()
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
                    .Map((p, t) => p.Discount)
                    .To(x => x.Value);

                var personSource = new Person { Name = "Wilma" };
                var personResult = mapper.Map(personSource).ToANew<PublicProperty<string>>();

                personResult.Value.ShouldBe("Wilma");

                var customerSource = new Customer { Name = "Betty", Discount = 10.0m };
                var customerResult = mapper.Map(customerSource).ToANew<PublicProperty<string>>();

                customerResult.Value.ShouldBe("10.0");
            }
        }

        [Fact]
        public void ShouldConditionallyApplyAConfiguredMember()
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
        public void ShouldConditionallyApplyMultipleConfiguredMembers()
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
        public void ShouldWrapAConfiguredDataSourceException()
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
        public void ShouldApplyAConfiguredMemberFromAllSourceTypes()
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
        public void ShouldApplyAConfiguredMemberInARootEnumerable()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicField<string>>()
                    .Map(ctx => ctx.Source.Name)
                    .To(x => x.Value);

                var source = new[] { new Person { Name = "Mr Thomas" } };
                var result = mapper.Map(source).ToANew<List<PublicField<string>>>();

                source.ShouldBe(result.Select(r => r.Value), p => p.Name);
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredMemberFromADerivedSourceType()
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
        public void ShouldApplyAConfiguredExpression()
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
        public void ShouldWrapAnExceptionThrownInAConfiguredExpression()
        {
            Should.Throw<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicGetMethod<string>>()
                        .To<PublicField<short>>()
                        .Map((s, t) => int.Parse(s.GetValue()) / 0)
                        .To(x => x.Value);

                    var source = new PublicGetMethod<string>("1234");
                    var result = mapper.Map(source).ToANew<PublicField<short>>();

                    result.Value.ShouldBeDefault();
                }
            });
        }

        [Fact]
        public void ShouldApplyAConfiguredExpressionInAMemberEnumerable()
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
        public void ShouldApplyAConfiguredExpressionInAMemberNonGenericEnumerableConditionally()
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
        public void ShouldApplyAConfiguredExpressionWithMultipleNestedSourceMembers()
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
        public void ShouldApplyAConfiguredExpressionToADerivedTargetType()
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
        public void ShouldApplyAConfiguredExpressionInARootCollectionConditionally()
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
        public void ShouldApplyAConfiguredFunction()
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
        public void ShouldApplyAConfiguredComplexType()
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
        public void ShouldWrapAConfiguredComplexTypeDataSourceException()
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
        public void ShouldApplyAConfiguredNestedComplexType()
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
        public void ShouldHandleANullAConfiguredNestedComplexType()
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
        public void ShouldApplyAConfiguredComplexTypeConditionally()
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
        public void ShouldApplyAConfiguredComplexTypeEnumerable()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new { People = new[] { new Person { Name = "Jimmy" } } };

                mapper.WhenMapping
                    .From(source)
                    .To<PublicProperty<IEnumerable<Person>>>()
                    .Map((s, pp) => s.People)
                    .To(pp => pp.Value);

                var result = mapper.Map(source).ToANew<PublicProperty<IEnumerable<Person>>>();

                result.Value.First().Name.ShouldBe("Jimmy");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredComplexTypeEnumerableConditionally()
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
        public void ShouldApplyAConfiguredSourceAndTargetFunction()
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
        public void ShouldApplyAConfiguredSourceTargetAndIndexFunction()
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
        public void ShouldMapAConfiguredFunction()
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
        public void ShouldApplyAConfiguredExpressionUsingExtensionMethods()
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

        // ReSharper disable once ClassNeverInstantiated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class IdTester
        {
            public int ClassId { get; set; }

            public int ClassIdentifier { get; set; }
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }
}
