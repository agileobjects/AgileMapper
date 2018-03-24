﻿namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using MoreTestClasses;
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
            plan.ShouldContain("mysteryCustomer.Discount = fatsiToMcData.Source.Discount;");
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
                plan.ShouldContain(".Target.Name = sourceF__AnonymousType");
                plan.ShouldContain("_String_String.Name;");
                plan.ShouldContain("address.Line1 = sourceF__AnonymousType");
                plan.ShouldContain("_String_String.AddressLine1;");
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
            plan.ShouldContain(" = publicField_Ints.Value as ICollection<int>) != null");
            plan.ShouldContain("new List<int>(publicField_Ints.Value)");
            plan.ShouldContain("targetInts.Add(sourceIntArray[i])");
        }

        [Fact]
        public void ShouldIncludeASimpleTypeMemberConversion()
        {
            string plan = Mapper
                .GetPlanFor<PublicProperty<Guid>>()
                .ToANew<PublicField<string>>();

            plan.ShouldContain("ppgToPfsData.Source.Value.ToString(");
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
            plan.ShouldContain("ppoToPsmcData.Map(");
            plan.ShouldContain("\"SetValue\"");
        }

        [Fact]
        public void ShouldShowNestedMapChildCalls()
        {
            string plan = Mapper
                .GetPlanFor<PublicProperty<PublicField<object>>>()
                .ToANew<PublicSetMethod<PublicProperty<Order>>>();

            plan.ShouldContain("// Map PublicProperty<PublicField<object>> -> PublicSetMethod<PublicProperty<Order>>");
            plan.ShouldNotContain("// Map PublicField<object> -> PublicProperty<Order>");
            plan.ShouldContain("pfoToPpoData.Map(");
            plan.ShouldContain("\"Value\"");
        }

        [Fact]
        public void ShouldShowMapElementCalls()
        {
            string plan = Mapper
                .GetPlanFor<PublicProperty<object[]>>()
                .ToANew<PublicSetMethod<ICollection<Product>>>();

            plan.ShouldContain("// Map PublicProperty<object[]> -> PublicSetMethod<ICollection<Product>>");
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
        public void ShouldShowObjectTrackingAndRecursionFuncs()
        {
            using (var mapper = Mapper.CreateNew())
            {
                string plan = mapper.GetPlanFor<Parent>().ToANew<Parent>();

                plan.ShouldContain("mappedObjectsCache.Register(sourceParent, parent)");
                plan.ShouldContain("mappedObjectsCache.Register(sourceParent.EldestChild, child)");

                plan.ShouldContain("Recursion Mapper");

                plan.ShouldContain("Parent -> Parent");
                plan.ShouldContain("mappedObjectsCache.TryGet(pToPData2.Source, out parent)");

                plan.ShouldContain("Child -> Child");
                plan.ShouldContain("mappedObjectsCache.TryGet(cToCData2.Source, out child)");
            }
        }

        [Fact]
        public void ShouldNotIncludeChildObjectToTargetMappingPlans()
        {
            string plan = Mapper
                .GetPlanFor<PublicTwoFields<object, object>>()
                .ToANew<PublicTwoParamCtor<Product, Product>>();

            Regex.Matches(plan, "// Map object -> Product").Cast<Match>().ShouldBeEmpty();
        }

        [Fact]
        public void ShouldNotRangeCheckNullableToNonNullableValues()
        {
            string plan = Mapper.GetPlanFor<PublicField<int?>>().ToANew<PublicField<int>>();

            plan.ShouldNotContain("int.MinValue");
            plan.ShouldNotContain("Value.HasValue");
        }

        [Fact]
        public void ShouldNotAttemptUnnecessaryObjectCreationCallbacks()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Over<PublicField<string>>()
                    .Before
                    .CreatingInstances
                    .Call(ctx => Console.WriteLine("Nope!"))
                    .And
                    .After
                    .CreatingInstances
                    .Call(ctx => Console.WriteLine("No way!"));

                string plan = mapper.GetPlanFor<PublicField<string>>().Over<PublicField<string>>();

                plan.ShouldNotContain("Invoke");
            }
        }

        [Fact]
        public void ShouldShowEnumMismatches()
        {
            string plan = Mapper.GetPlanFor<OrderUs>().ToANew<OrderUk>();

            plan.ShouldContain("// WARNING - enum mismatches mapping OrderUs.PaymentType to OrderUk.PaymentType:");
            plan.ShouldContain("//  - PaymentTypeUs.Check matches no PaymentTypeUk");
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
        public void ShouldIncludeUnmappableEntityKeyMemberDetails()
        {
            string plan = Mapper.GetPlanFor<OrderDto>().ToANew<OrderEntity>();

            plan.ShouldContain("Entity key member");
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
        public void ShouldIncludeUnmappableNoChildDataSourcesComplexTypeMemberDetails()
        {
            string plan = Mapper
                .GetPlanFor(new { Int = default(int) })
                .ToANew<PublicField<Address>>();

            plan.ShouldContain("No data source for Value or any of its child members");
        }

        [Fact]
        public void ShouldShowAllCachedMappingPlans()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<PublicField<string>>().ToANew<PublicProperty<int>>();
                mapper.GetPlanFor<Customer>().ToANew<CustomerViewModel>();
                mapper.GetPlansFor(new MegaProduct()).To<ProductDtoMega>();

                var plan = mapper.GetPlansInCache();

                plan.ShouldContain("PublicField<string> -> PublicProperty<int>");
                plan.ShouldContain("Customer -> CustomerViewModel");
                plan.ShouldContain("MegaProduct -> ProductDtoMega");
                plan.ShouldContain("Rule Set: CreateNew");
                plan.ShouldContain("Rule Set: Merge");
                plan.ShouldContain("Rule Set: Overwrite");

                mapper.RootMapperCountShouldBe(5);
            }
        }
    }
}
