namespace AgileObjects.AgileMapper.UnitTests.Extensions.Internal
{
    using System.Collections.Generic;
    using AgileMapper.Extensions.Internal;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    [Trait("Category", "Checked")]
    public class WhenGeneratingVariableNames
    {
        [Fact]
        public void ShouldNameAShortCollectionTypeVariable()
        {
            typeof(ICollection<Person>).GetShortVariableName().ShouldBe("pic");
        }

        [Fact]
        public void ShouldNameAMultiLetterShortCollectionTypeVariable()
        {
            typeof(ICollection<CustomerViewModel>).GetShortVariableName().ShouldBe("cvmic");
        }
    }
}
