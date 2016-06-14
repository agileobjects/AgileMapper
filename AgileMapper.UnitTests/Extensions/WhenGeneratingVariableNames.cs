namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using System.Collections.Generic;
    using AgileMapper.Extensions;
    using TestClasses;
    using Xunit;

    public class WhenGeneratingVariableNames
    {
        [Fact]
        public void ShouldNameAShortVariableForACollectionType()
        {
            typeof(ICollection<Person>).GetShortVariableName().ShouldBe("ps");
        }

        [Fact]
        public void ShouldNameAMultiLetterShortVariableForACollectionType()
        {
            typeof(ICollection<CustomerViewModel>).GetShortVariableName().ShouldBe("cvms");
        }

        [Fact]
        public void ShouldNameAVariableForACollectionTypeEndingInX()
        {
            typeof(ICollection<Box>).GetVariableName(f => f.InCamelCase).ShouldBe("boxes");
        }

        [Fact]
        public void ShouldNameAVariableForAnEnumerableTypeEndingInZ()
        {
            typeof(IEnumerable<Fuzz>).GetVariableName(f => f.InPascalCase).ShouldBe("Fuzzes");
        }

        [Fact]
        public void ShouldNameAVariableForAnEnumerableTypeEndingInDoubleS()
        {
            typeof(IEnumerable<Glass>).GetVariableName(f => f.InPascalCase).ShouldBe("Glasses");
        }

        [Fact]
        public void ShouldNameAVariableForAListTypeEndingInCh()
        {
            typeof(List<Church>).GetVariableName(f => f.InCamelCase).ShouldBe("churches");
        }

        [Fact]
        public void ShouldNameAVariableForAListTypeEndingInSh()
        {
            typeof(List<Hush>).GetVariableName(f => f.InCamelCase).ShouldBe("hushes");
        }

        [Fact]
        public void ShouldNameAVariableForAListTypeEndingInVowelY()
        {
            typeof(List<Journey>).GetVariableName(f => f.InCamelCase).ShouldBe("journeys");
        }

        [Fact]
        public void ShouldNameAVariableForAnIListTypeEndingInConsonantY()
        {
            typeof(IList<Body>).GetVariableName(f => f.InPascalCase).ShouldBe("Bodies");
        }

        private class Box { }

        private class Fuzz { }

        private class Glass { }

        private class Church { }

        private class Hush { }

        private class Journey { }

        private class Body { }
    }
}
