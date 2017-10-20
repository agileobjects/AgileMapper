namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text.RegularExpressions;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenViewingMappingPlans
    {
        [Fact]
        public void ShouldIncludeASimpleTypeMemberMapping()
        {
            string plan = Mapper
                .GetPlanFor<PublicField<string>>()
                .ToANew<PublicProperty<string>>();

            plan.ShouldContain("publicProperty_String.Value = pfsToPpsData.Source.Value;");
        }

        [Fact]
        public void ShouldGenerateAllRuleSets()
        {
            string plan = Mapper
                .GetPlansFor<PublicField<string>>()
                .To<PublicProperty<string>>();

            plan.ShouldContain("Rule Set: CreateNew");
            plan.ShouldContain("Rule Set: Merge");
            plan.ShouldContain("Rule Set: Overwrite");
        }

        [Fact]
        public void ShouldSupportAnonymousSourceTypesFromTheStaticApi()
        {
            string plan = Mapper
                .GetPlanFor(new { Name = default(string), Discount = default(int) })
                .ToANew<MysteryCustomer>();

            plan.ShouldContain("Map AnonymousType<string, int> -> MysteryCustomer");
            plan.ShouldContain("mysteryCustomer.Name = fatsiToMcData.Source.Name;");
            plan.ShouldContain("mysteryCustomer.Discount = (decimal)fatsiToMcData.Source.Discount;");
            plan.ShouldContain("// No data source for Report");
        }

        [Fact]
        public void ShouldSupportAnonymousSourceTypesFromTheInstanceApi()
        {
            using (var mapper = Mapper.CreateNew())
            {
                string plan = mapper
                    .GetPlanFor(new { Name = default(string), AddressLine1 = default(string) })
                    .OnTo<Customer>();

                plan.ShouldContain("Map AnonymousType<string, string> -> Customer");
                plan.ShouldContain("customer.Name = sourceF__AnonymousType29_String_String.Name;");
                plan.ShouldContain("address.Line1 = sourceF__AnonymousType29_String_String.AddressLine1;");
            }
        }

        [Fact]
        public void ShouldSupportStructsFromTheStaticApi()
        {
            string plan = Mapper
                .GetPlanFor<PublicTwoFieldsStruct<int, int>>()
                .Over<PublicTwoFieldsStruct<string, string>>();

            plan.ShouldContain("publicTwoFieldsStruct_String_String.Value1 = ptfsiiToPtfsssData.Source.Value1.ToString();");
        }

        [Fact]
        public void ShouldSupportStructMergePlansFromTheStaticApi()
        {
            string plan = Mapper
                .GetPlanFor<PublicTwoFieldsStruct<int, int>>()
                .OnTo<PublicTwoFieldsStruct<string, string>>();

            plan.ShouldContain("publicTwoFieldsStruct_String_String.Value1 = ptfsiiToPtfsssData.Source.Value1.ToString();");
        }

        [Fact]
        public void ShouldSupportStructsFromTheInstanceApi()
        {
            using (var mapper = Mapper.CreateNew())
            {
                string plan = mapper
                    .GetPlanFor<PublicPropertyStruct<string>>()
                    .ToANew<PublicCtorStruct<string>>();

                plan.ShouldContain("Map PublicPropertyStruct<string> -> PublicCtorStruct<string>");
                plan.ShouldContain("return new PublicCtorStruct");
                plan.ShouldContain("ppssToPcssData.Source.Value");
            }
        }

        [Fact]
        public void ShouldIncludeAComplexTypeMemberMapping()
        {
            string plan = Mapper
                .GetPlanFor<PersonViewModel>()
                .ToANew<Person>();

            plan.ShouldContain("person.Name = sourcePersonViewModel.Name;");
            plan.ShouldContain("address.Line1 = sourcePersonViewModel.AddressLine1;");
        }

        [Fact]
        public void ShouldIncludeASimpleTypeEnumerableMemberMapping()
        {
            string plan = Mapper
                .GetPlanFor<PublicProperty<int[]>>()
                .ToANew<PublicField<IEnumerable<int>>>();

            plan.ShouldContain("sourceIntArray = ");
            plan.ShouldContain("ICollection<int> targetInts = ");
            plan.ShouldContain("publicField_Ints.Value is ICollection<int>");
            plan.ShouldContain("new List<int>(publicField_Ints.Value)");
            plan.ShouldContain("targetInts.Add(sourceIntArray[i])");
        }

        [Fact]
        public void ShouldIncludeASimpleTypeMemberConversion()
        {
            string plan = Mapper
                .GetPlanFor<PublicProperty<Guid>>()
                .ToANew<PublicField<string>>();

            plan.ShouldContain("data.Source.Value.ToString(");
        }

        [Fact]
        public void ShouldIncludeAConfiguredExpression()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .From<Person>()
                    .Over<PersonViewModel>()
                    .Map((p, pvm) => p.Title + " " + p.Name)
                    .To(pvm => pvm.Name);

                string plan = mapper
                    .GetPlanFor<Person>()
                    .Over<PersonViewModel>();

                plan.ShouldContain("pToPvmData.Target.Name = sourcePerson.Title + \" \" + sourcePerson.Name");
            }
        }

        [Fact]
        public void ShouldIncludeARootComplexTypeEnumerableMapping()
        {
            string plan = Mapper
                .GetPlanFor<IEnumerable<Person>>()
                .OnTo<IEnumerable<PersonViewModel>>();

            plan.ShouldContain("collectionData.Intersection.ForEach((person, personViewModel, i) =>");
            plan.ShouldContain("persons = collectionData.NewSourceItems");
        }

        [Fact]
        public void ShouldIncludeAComplexTypeEnumerableMemberMapping()
        {
            string plan = Mapper
                .GetPlanFor<IList<PersonViewModel>>()
                .Over<IEnumerable<Person>>();

            plan.ShouldContain("personViewModels = collectionData.NewSourceItems");
            plan.ShouldContain("collectionData.Intersection.ForEach((personViewModel, person, i) =>");
            plan.ShouldContain("collectionData.AbsentTargetItems.ForEach(persons.Remove)");

            plan.ShouldContain("IList<PersonViewModel> -> IEnumerable<Person>");
            plan.ShouldNotContain("PersonViewModel -> Person");  // <- because the mapping is inlined
            plan.ShouldNotContain("PersonViewModel -> Address"); // <- because the mapping is inlined
        }

        [Fact]
        public void ShouldIncludeAMemberWithNoDataSource()
        {
            string plan = Mapper
                .GetPlanFor<PersonViewModel>()
                .OnTo<Person>();

            plan.ShouldContain("// No data source for Title");
        }

        [Fact]
        public void ShouldIncludeAnIgnoredMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .To<PersonViewModel>()
                    .Ignore(pvm => pvm.AddressLine1);

                string plan = mapper
                    .GetPlanFor<Person>()
                    .ToANew<PersonViewModel>();

                plan.ShouldContain("// AddressLine1 is ignored");
            }
        }

        [Fact]
        public void ShouldNotIncludeASourceMemberWithTheSameConditionAsAConfiguredMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .From<PublicField<PublicField<string>>>()
                    .ToANew<PublicProperty<string>>()
                    .Map((pf, pp) => pf.Value.Value)
                    .To(pp => pp.Value);

                string plan = mapper
                    .GetPlanFor<PublicField<PublicField<string>>>()
                    .ToANew<PublicProperty<string>>();

                plan.ShouldContain("data.Source.Value.Value");
                plan.ShouldNotContain("data.Source.Value.ToString()");
            }
        }

        [Fact]
        public void ShouldShowInlineMappers()
        {
            string plan = Mapper
                .GetPlanFor<PublicField<PublicField<PublicField<int>>>>()
                .ToANew<PublicField<PublicSealed<PublicSealed<long>>>>();

            var numberOfHeaders = Regex.Matches(plan, "// Map ").Count;

            numberOfHeaders.ShouldBe(1);
        }

        [Fact]
        public void ShouldShowMapChildCalls()
        {
            string plan = Mapper
                .GetPlanFor<PublicProperty<object>>()
                .ToANew<PublicSetMethod<Customer>>();

            plan.ShouldContain("// Map PublicProperty<object> -> PublicSetMethod<Customer>");
            plan.ShouldContain("// Map object -> Customer");
            plan.ShouldContain("// Map object -> Address");
            plan.ShouldContain("ppoToPsmcData.Map(");
        }

        [Fact]
        public void ShouldShowNestedMapChildCalls()
        {
            string plan = Mapper
                .GetPlanFor<PublicProperty<PublicField<object>>>()
                .ToANew<PublicSetMethod<PublicProperty<Order>>>();

            plan.ShouldContain("// Map PublicProperty<PublicField<object>> -> PublicSetMethod<PublicProperty<Order>>");
            plan.ShouldNotContain("// Map PublicField<object> -> PublicProperty<Order>");
            plan.ShouldContain("// Map object -> Order");
            plan.ShouldContain("pfoToPpoData.Map(");
        }

        // See https://github.com/agileobjects/AgileMapper/issues/13
        [Fact]
        public void ShouldShowMapChildObjectCalls()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicTwoFields<string, object>>()
                    .Map((pp, ptf) => pp.Value)
                    .To(ptf => ptf.Value1);


                string plan = mapper
                    .GetPlanFor<PublicProperty<string>>()
                    .ToANew<PublicTwoFields<string, object>>();

                plan.ShouldContain("// Map PublicProperty<string> -> PublicTwoFields<string, object>");
                plan.ShouldContain(".Value1 = ppsToPtfsoData.Source.Value");
                plan.ShouldContain("// No data source for Value2");
            }
        }

        [Fact]
        public void ShouldShowMapElementCalls()
        {
            string plan = Mapper
                .GetPlanFor<PublicProperty<object[]>>()
                .ToANew<PublicSetMethod<ICollection<Product>>>();

            plan.ShouldContain("// Map PublicProperty<object[]> -> PublicSetMethod<ICollection<Product>>");
            plan.ShouldContain("// Map object -> Product");
            plan.ShouldContain("products.Add(oaToPsData.Map(objectArray[i]");
        }

        // See https://github.com/agileobjects/AgileMapper/issues/24
        [Fact]
        public void ShouldShowDictionaryElementMapping()
        {
            string plan = Mapper
                .GetPlanFor<List<PublicField<Dictionary<int, string>>>>()
                .ToANew<List<PublicField<Dictionary<int, string>>>>();

            plan.ShouldContain("// Map List<PublicField<Dictionary<int, string>>> -> List<PublicField<Dictionary<int, string>>>");
        }

        [Fact]
        public void ShouldShowObjectTracking()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.TrackMappedObjects();

                string plan = mapper.GetPlanFor<Parent>().ToANew<Parent>();

                plan.ShouldContain("pToPData.TryGet(sourceChild.EldestParent, out parent)");
                plan.ShouldContain("pToPData.Register(sourceChild.EldestParent, parent)");
            }
        }

        [Fact]
        public void ShouldNotDuplicateChildMappingPlans()
        {
            string plan = Mapper
                .GetPlanFor<PublicTwoFields<object, object>>()
                .ToANew<PublicTwoParamCtor<Product, Product>>();

            var numberOfObjectToProductPlans = Regex.Matches(plan, "// Map object -> Product").Count;

            numberOfObjectToProductPlans.ShouldBe(1);
        }

        [Fact]
        public void ShouldNotRangeCheckNullableToNonNullableValues()
        {
            string plan = Mapper.GetPlanFor<PublicField<int?>>().ToANew<PublicField<int>>();

            plan.ShouldNotContain("int.MinValue");
        }

        [Fact]
        public void ShouldShowEnumMismatches()
        {
            string plan = Mapper
                .GetPlanFor<OrderUs>()
                .ToANew<OrderUk>();

            plan.ShouldContain("// WARNING - enum mismatches mapping OrderUs.PaymentType to OrderUk.PaymentType:");
            plan.ShouldContain("//  - PaymentTypeUs.Check matches no PaymentTypeUk");
            plan.ShouldContain("//  - PaymentTypeUk.Cheque is matched by no PaymentTypeUs");
        }

        [Fact]
        public void ShouldShowMultipleEnumSourceMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<PaymentTypeUk, PaymentTypeUs>>()
                    .To<OrderUs>()
                    .Map((s, o) => s.Value1).To(o => o.PaymentType)
                    .But
                    .If((s, o) => s.Value1 == PaymentTypeUk.Cheque)
                    .Map((s, o) => s.Value2).To(o => o.PaymentType);

                string plan = mapper
                    .GetPlanFor<PublicTwoFields<PaymentTypeUk, PaymentTypeUs>>()
                    .ToANew<OrderUs>();

                plan.ShouldContain("PublicTwoFields<PaymentTypeUk, PaymentTypeUs>.Value1 to OrderUs.PaymentType");
                plan.ShouldContain("Value1 == PaymentTypeUk.Cheque");
                plan.ShouldContain("PaymentTypeUk.Cheque matches no PaymentTypeUs");
                plan.ShouldContain("PaymentTypeUs.Check is matched by no PaymentTypeUk");
            }
        }

        [Fact]
        public void ShouldShowNestedEnumMismatches()
        {
            string plan = Mapper
                .GetPlanFor<PublicField<OrderUs>>()
                .ToANew<PublicProperty<OrderUk>>();

            plan.ShouldContain("WARNING");
            plan.ShouldContain("PublicField<OrderUs>.Value.PaymentType to PublicProperty<OrderUk>.Value.PaymentType");
        }

        [Fact]
        public void ShouldNotAssignATargetMemberToItself()
        {
            string plan = Mapper.GetPlanFor<PublicField<string>>().OnTo<PublicField<string>>();

            plan.ShouldNotContain("publicField_String.Value = publicField_String.Value");
        }

        [Fact]
        public void ShouldIncludeMemberFilterExpressions()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<Address>()
                    .IgnoreTargetMembersWhere(member => member.IsPropertyMatching(p => p.Name == "Line2"));

                string plan = mapper.GetPlanFor<Address>().ToANew<Address>();

                plan.ShouldContain("member.IsPropertyMatching(p => p.Name == \"Line2\")");
            }
        }

        [Fact]
        public void ShouldIncludeUnmappableStructComplexTypeMemberDetails()
        {
            using (var mapper = Mapper.CreateNew())
            {
                string plan = mapper
                    .GetPlanFor<PublicTwoFields<Person, string>>()
                    .ToANew<PublicTwoFieldsStruct<Person, int>>();

                plan.ShouldContain("int.TryParse(ptfpsToPtfspiData.Source.Value2");
                plan.ShouldContain("Person member on a struct");
            }
        }

        [Fact]
        public void ShouldIncludeUnmappableReadOnlyArrayMemberDetails()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string[]>>()
                    .To<PublicReadOnlyField<int[]>>()
                    .Map(ctx => ctx.Source.Value)
                    .ToCtor<int[]>();

                string plan = mapper
                    .GetPlanFor<PublicField<string[]>>()
                    .ToANew<PublicReadOnlyField<int[]>>();

                plan.ShouldContain("readonly array");
            }
        }

        [Fact]
        public void ShouldIncludeUnmappableReadOnlyReadOnlyCollectionMemberDetails()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string[]>>()
                    .To<PublicReadOnlyField<ReadOnlyCollection<int>>>()
                    .Map(ctx => ctx.Source.Value)
                    .ToCtor("readOnlyValue");

                string plan = mapper
                    .GetPlanFor<PublicField<string[]>>()
                    .ToANew<PublicReadOnlyField<ReadOnlyCollection<int>>>();

                plan.ShouldContain("readonly ReadOnlyCollection<int>");
            }
        }

        [Fact]
        public void ShouldIncludeUnmappableReadOnlyIntMemberDetails()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string>>()
                    .To<PublicReadOnlyField<int>>()
                    .Map(ctx => ctx.Source.Value)
                    .ToCtor("readOnlyValue");

                string plan = mapper
                    .GetPlanFor<PublicField<string>>()
                    .ToANew<PublicReadOnlyField<int>>();

                plan.ShouldContain("readonly int");
            }
        }

        [Fact]
        public void ShouldShowAllCachedMappingPlans()
        {
            Mapper.GetPlanFor<PublicField<string>>().ToANew<PublicProperty<int>>();
            Mapper.GetPlanFor<Customer>().ToANew<CustomerViewModel>();
            Mapper.GetPlansFor<MegaProduct>().To<ProductDtoMega>();

            var plan = Mapper.GetPlansInCache();

            plan.ShouldContain("PublicField<string> -> PublicProperty<int>");
            plan.ShouldContain("Customer -> CustomerViewModel");
            plan.ShouldContain("MegaProduct -> ProductDtoMega");
            plan.ShouldContain("Rule set: CreateNew");
            plan.ShouldContain("Rule set: Merge");
            plan.ShouldContain("Rule set: Overwrite");
        }
    }
}
