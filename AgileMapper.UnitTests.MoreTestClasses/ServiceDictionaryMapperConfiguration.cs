namespace AgileObjects.AgileMapper.UnitTests.MoreTestClasses
{
    using System.Collections.Generic;
    using Configuration;

    public class ServiceDictionaryMapperConfiguration : MapperConfiguration
    {
        protected override void Configure()
        {
            var mappersByName = GetServiceOrThrow<Dictionary<string, IMapper>>();

            var mapper1 = CreateNewMapper();

            mapper1.WhenMapping
                .From<Dog>()
                .To<Dog>()
                .Map((s, t) => "BARK 1!")
                .To(t => t.WoofSound);

            mappersByName.Add("DogMapper1", mapper1);

            var mapper2 = CreateNewMapper();

            mapper2.WhenMapping
                .From<Dog>()
                .To<Dog>()
                .Map((s, t) => "BARK 2!")
                .To(t => t.WoofSound);

            mappersByName.Add("DogMapper2", mapper2);
        }

        public static bool VerifyConfigured(Dictionary<string, IMapper> mappersByName)
        {
            if (!mappersByName.ContainsKey("DogMapper1") || !mappersByName.ContainsKey("DogMapper2"))
            {
                return false;
            }

            var result1 = mappersByName["DogMapper1"].DeepClone(new Dog());

            if (result1.WoofSound != "BARK 1!")
            {
                return false;
            }

            var result2 = mappersByName["DogMapper2"].DeepClone(new Dog());

            if (result2.WoofSound != "BARK 2!")
            {
                return false;
            }

            return true;
        }
    }
}
