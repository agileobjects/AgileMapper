namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Text.RegularExpressions;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenViewingMappingPlans
    {
        [Fact]
        public void ShouldShowASimpleTypeMemberMapping()
        {
            string plan = Mapper
                .GetPlanFor<PublicField<string>>()
                .ToANew<PublicProperty<string>>();

            plan.ShouldContain("stringPublicProperty.Value = spfToSppData.Source.Value;");
        }

        [Fact]
        public void ShouldSurfaceAMappingPlanExpression()
        {
            Expression plan = Mapper
                .GetPlanFor<PublicField<string>>()
                .ToANew<PublicProperty<string>>();

            plan.ShouldNotBeNull();
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
            plan.ShouldContain("mysteryCustomer.Name = siatToMcData.Source.Name;");
            plan.ShouldContain("mysteryCustomer.Discount = siatToMcData.Source.Discount;");
            plan.ShouldContain("// No data sources for Report");
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
                plan.ShouldContain(".Target.Name = ssatToCData.Source.Name");
                plan.ShouldContain("address.Line1 = ssatToCData.Source.AddressLine1");
            }
        }

        [Fact]
        public void ShouldSupportStructsFromTheStaticApi()
        {
            string plan = Mapper
                .GetPlanFor<PublicTwoFieldsStruct<int, int>>()
                .Over<PublicTwoFieldsStruct<string, string>>();

            plan.ShouldContain("stringStringPublicTwoFieldsStruct.Value1 = iiptfsToSsptfsData.Source.Value1.ToString();");
        }

        [Fact]
        public void ShouldSupportStructMergePlansFromTheStaticApi()
        {
            string plan = Mapper
                .GetPlanFor<PublicTwoFieldsStruct<int, int>>()
                .OnTo<PublicTwoFieldsStruct<string, string>>();

            plan.ShouldContain("stringStringPublicTwoFieldsStruct.Value1 = iiptfsToSsptfsData.Source.Value1.ToString();");
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
                plan.ShouldContain("new PublicCtorStruct<string>(sppsToSpcsData.Source.Value)");
                plan.ShouldContain("return stringPublicCtorStruct");
            }
        }

        [Fact]
        public void ShouldShowAComplexTypeMemberMapping()
        {
            string plan = Mapper
                .GetPlanFor<PersonViewModel>()
                .ToANew<Person>();

            plan.ShouldContain("person.Name = sourcePersonViewModel.Name;");
            plan.ShouldContain("address.Line1 = sourcePersonViewModel.AddressLine1;");
        }

        [Fact]
        public void ShouldShowASimpleTypeEnumerableMemberMapping()
        {
            string plan = Mapper
                .GetPlanFor<PublicProperty<int[]>>()
                .ToANew<PublicField<IEnumerable<int>>>();

            plan.ShouldContain("sourceIntArray = ");
            plan.ShouldContain("ICollection<int> targetIntICollection = ");
            plan.ShouldContain(" = intIEnumerablePublicField.Value as ICollection<int>) != null");
            plan.ShouldContain("new List<int>(intIEnumerablePublicField.Value)");
            plan.ShouldContain("targetIntICollection.Add(sourceIntArray[i])");
        }

        [Fact]
        public void ShouldShowASimpleTypeMemberConversion()
        {
            string plan = Mapper
                .GetPlanFor<PublicProperty<Guid>>()
                .ToANew<PublicField<string>>();

            plan.ShouldContain("gppToSpfData.Source.Value.ToString(");
        }

        [Fact]
        public void ShouldShowARootComplexTypeEnumerableMapping()
        {
            string plan = Mapper
                .GetPlanFor<IEnumerable<Person>>()
                .OnTo<IEnumerable<PersonViewModel>>();

            plan.ShouldContain("collectionData.Intersection.ForEach((existingPerson, existingPersonViewModel, idx) =>");
            plan.ShouldContain("personIEnumerable = collectionData.NewSourceItems");
        }

        [Fact]
        public void ShouldShowAComplexTypeEnumerableMemberMapping()
        {
            string plan = Mapper
                .GetPlanFor<IList<PersonViewModel>>()
                .Over<IEnumerable<Person>>();

            plan.ShouldContain("personViewModelIEnumerable = collectionData.NewSourceItems");
            plan.ShouldContain("collectionData.Intersection.ForEach((existingPersonViewModel, existingPerson, idx) =>");
            plan.ShouldContain("collectionData.AbsentTargetItems.ForEach(p => personICollection.Remove(p)");

            plan.ShouldContain("IList<PersonViewModel> -> IEnumerable<Person>");
            plan.ShouldNotContain("PersonViewModel -> Person");
            plan.ShouldNotContain("PersonViewModel -> Address");
        }

        [Fact]
        public void ShouldShowAMemberWithNoDataSource()
        {
            string plan = Mapper
                .GetPlanFor<PersonViewModel>()
                .OnTo<Person>();

            plan.ShouldContain("// No data sources for Title");
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
            plan.ShouldContain("oppToCpsmData.Map(");
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
            plan.ShouldContain("opfToOppData.Map(");
            plan.ShouldContain("\"Value\"");
        }

        [Fact]
        public void ShouldShowMapElementCalls()
        {
            string plan = Mapper
                .GetPlanFor<PublicProperty<object[]>>()
                .ToANew<PublicSetMethod<ICollection<Product>>>();

            plan.ShouldContain("// Map PublicProperty<object[]> -> PublicSetMethod<ICollection<Product>>");
            RemoveWhiteSpace(plan).ShouldContain("productList.Add(oaToPicData.Map(objectArray[i]");
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
        public void ShouldShowObjectTrackingAndRepeatedMappingFuncs()
        {
            using (var mapper = Mapper.CreateNew())
            {
                string plan = mapper.GetPlanFor<Parent>().ToANew<Parent>();

                plan.ShouldContain("pToPData =>");

                plan.ShouldContain("pToPData.Register(sourceParent, parent)");
                plan.ShouldContain("pToPData.Register(sourceParent.EldestChild, child)");

                plan.ShouldContain("Repeated Mapping Mapper");

                plan.ShouldContain("Parent -> Parent");
                plan.ShouldContain("pToPData2.TryGet(pToPData2.Source, out parent)");

                plan.ShouldContain("Child -> Child");
                plan.ShouldContain("cToCData2.TryGet(cToCData2.Source, out child)");
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
        public void ShouldNotNullCheckStringSplitCallResults()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string>>()
                    .ToANew<PublicField<string[]>>()
                    .Map((i, l) => i.Value.Split(new[] { ',' }))
                    .To(l => l.Value);

                string plan = mapper.GetPlanFor<PublicField<string>>().ToANew<PublicField<string[]>>();

                plan.ShouldNotContain("Value.Split(',') != null");
            }
        }

        [Fact]
        public void ShouldNotNullCheckLinqMethodCallResults()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int[]>>()
                    .ToANew<PublicField<long[]>>()
                    .Map((i, l) => i.Value.Select(v => v * 2).ToArray())
                    .To(l => l.Value);

                string plan = mapper.GetPlanFor<PublicField<int[]>>().ToANew<PublicField<long[]>>();

                plan.ShouldNotContain("Select(v => v * 2) != null");
                plan.ShouldNotContain("ToArray() != null");
            }
        }

        [Fact]
        public void ShouldCacheComplexTypeGetMethodResults()
        {
            string plan = Mapper
                .GetPlanFor<PublicField<PublicGetMethod<Address>>>()
                .ToANew<PublicProperty<PublicProperty<Address>>>();

            plan.ShouldContain("Value.GetValue()");
            Regex.Matches(plan, @"Value.GetValue\(\)").Cast<Match>().ShouldHaveSingleItem();
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

        // See https://github.com/agileobjects/AgileMapper/issues/146
        [Fact]
        public void ShouldUseBaseInterfaceTypeSourceMembersWithoutRuntimeTyping()
        {
            string plan = Mapper
                .GetPlanFor<Issue146.Source.Container>()
                .ToANew<Issue146.Target.Cont>();

            plan.ShouldContain("data.Id = cToCData.Source.Info.Id;");
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
        public void ShouldShowUnmappableEntityKeyMemberDetails()
        {
            string plan = Mapper.GetPlanFor<OrderDto>().ToANew<OrderEntity>();

            plan.ShouldContain("Entity key member");
        }

        [Fact]
        public void ShouldShowUnmappableStructComplexTypeMemberDetails()
        {
            using (var mapper = Mapper.CreateNew())
            {
                string plan = mapper
                    .GetPlanFor<PublicTwoFields<Person, string>>()
                    .ToANew<PublicTwoFieldsStruct<Person, int>>();

                plan.ShouldContain("int.TryParse(psptfToPiptfsData.Source.Value2");
                plan.ShouldContain("Person member on a struct");
            }
        }

        [Fact]
        public void ShouldShowUnmappableNoChildDataSourcesComplexTypeMemberDetails()
        {
            string plan = Mapper
                .GetPlanFor(new { Int = default(int) })
                .ToANew<PublicField<Address>>();

            plan.ShouldContain("No data sources for Value or any of its child members");
        }

        [Fact]
        public void ShouldShowAllCachedMappingPlans()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<PublicField<string>>().ToANew<PublicProperty<int>>();
                mapper.GetPlanFor<Customer>().ToANew<CustomerViewModel>();
                mapper.GetPlansFor(new MegaProduct()).To<ProductDtoMega>();

                string plan = mapper.GetPlansInCache();

                plan.ShouldContain("PublicField<string> -> PublicProperty<int>");
                plan.ShouldContain("Customer -> CustomerViewModel");
                plan.ShouldContain("MegaProduct -> ProductDtoMega");
                plan.ShouldContain("Rule Set: CreateNew");
                plan.ShouldContain("Rule Set: Merge");
                plan.ShouldContain("Rule Set: Overwrite");

                mapper.RootMapperCountShouldBe(5);
            }
        }

        [Fact]
        public void ShouldShowAllCachedMappingPlanExpressions()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<PublicField<string>>().ToANew<PublicProperty<int>>();
                mapper.GetPlanFor<Customer>().ToANew<CustomerViewModel>();
                mapper.GetPlansFor(new MegaProduct()).To<ProductDtoMega>();

                var expressions = mapper.GetPlanExpressionsInCache();

                expressions.ShouldNotBeNull().Any().ShouldBeTrue();
            }
        }

        #region Helper Members

        private static string RemoveWhiteSpace(string plan)
            => plan.Replace(Environment.NewLine, null).Replace(" ", null);

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
                }

                public class Container
                {
                    public Container(string infoId)
                    {
                        Info = new Data { Id = infoId };
                    }

                    public string Name { get; set; }

                    public IEmpty Info { get; }
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

        #endregion
    }
}
