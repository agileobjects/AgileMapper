namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using Common;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringSequentialDataSources
    {
        // See https://github.com/agileobjects/AgileMapper/issues/184
        [Fact]
        public void ShouldApplySequentialDataSourcesToANestedTarget()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue184.SourcePets>()
                    .ToANew<Issue184.TargetPets>()
                    .Map((src, _) => src.TheCat)
                    .To(tp => tp.PetNames)
                    .Then
                    .Map((src, _) => src.TheDog)
                    .To(tp => tp.PetNames);

                var source = new Issue184.SourcePets
                {
                    TheCat = new Issue184.Cat { CatName = "Tiddles" },
                    TheDog = new Issue184.Dog { DogName = "Rover" }
                };

                var result = mapper.Map(source).ToANew<Issue184.TargetPets>();

                result.PetNames.ShouldNotBeNull();
                result.PetNames.CatName.ShouldBe("Tiddles");
                result.PetNames.DogName.ShouldBe("Rover");
            }
        }

        #region Helper Members

        public static class Issue184
        {
            public class SourcePets
            {
                public Cat TheCat { get; set; }

                public Dog TheDog { get; set; }
            }

            public class Cat
            {
                public string CatName { get; set; }
            }

            public class Dog
            {
                public string DogName { get; set; }
            }

            public class TargetPets
            {
                public PetNames PetNames { get; set; }
            }

            public class PetNames
            {
                public string CatName { get; set; }

                public string DogName { get; set; }
            }
        }

        #endregion
    }
}
