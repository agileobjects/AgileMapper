namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenViewingDictionaryMappingPlans
    {
        [Fact]
        public void ShouldShowATargetObjectMappingPlan()
        {
            string plan = Mapper
                .GetPlanFor<Dictionary<string, string>>()
                .ToANew<CustomerViewModel>();

            plan.ShouldContain("Dictionary<string, string> sourceStringStringDictionary");
            plan.ShouldContain("idKey = sourceStringStringDictionary.Keys.FirstOrDefault(key => key.MatchesKey(\"Id\"");
            plan.ShouldContain("id = sourceStringStringDictionary[idKey]");
            plan.ShouldContain("customerViewModel.Id =");
#if NET35
            plan.ShouldContain("id.ToGuid()");
#else
            plan.ShouldContain("Guid.TryParse(id");
#endif
        }

        [Fact]
        public void ShouldShowATargetComplexTypeCollectionMappingPlan()
        {
            string plan = Mapper
                .GetPlanFor<Dictionary<string, object>>()
                .ToANew<Collection<Address>>();

            plan.ShouldContain("targetElementKey = \"[\" + i + \"]\"");
            plan.ShouldContain("elementKeyExists = sourceStringObjectDictionary.ContainsKey(targetElementKey)");
            plan.ShouldContain("var line1Key = \"[\" + i + \"].Line1\"");
            plan.ShouldContain("line1 = sourceStringObjectDictionary[line1Key]");
            plan.ShouldContain("address.Line1 = line1.ToString()");
        }

        [Fact]
        public void ShouldShowASourceObjectMappingPlan()
        {
            string plan = Mapper
                .GetPlanFor<MysteryCustomer>()
                .ToANew<Dictionary<string, object>>();

            plan.ShouldContain("stringObjectDictionary = new Dictionary<string, object>()");
            plan.ShouldContain("stringObjectDictionary[\"Name\"] = sourceMysteryCustomer.Name");
            plan.ShouldContain("stringObjectDictionary[\"Address.Line1\"] = sourceMysteryCustomer.Address.Line1;");
        }

        [Fact]
        public void ShouldShowASourceComplexTypeEnumerableMappingPlan()
        {
            string plan = Mapper
                .GetPlanFor<IEnumerable<CustomerViewModel>>()
                .ToANew<Dictionary<string, string>>();

            plan.ShouldContain("sourceMysteryCustomerViewModel = enumerator.Current as MysteryCustomerViewModel");
            plan.ShouldContain("stringStringDictionary[\"[\" + i + \"].Report\"] = sourceMysteryCustomerViewModel.Report");
            plan.ShouldContain("stringStringDictionary[\"[\" + i + \"].AddressLine1\"] = enumerator.Current.AddressLine1");
        }

    }
}