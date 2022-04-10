namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenDeterminingATypeIdentifier : MemberTestsBase
    {
        [Fact, Trait("Category", "Checked")]
        public void ShouldUseAnIdProperty()
        {
            DefaultMapperContext
                .Naming
                .GetIdentifierOrNull(new { Id = "blahblahblah" }.GetType())
                .ShouldNotBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldUseAnIdentifierProperty()
        {
            DefaultMapperContext
                .Naming
                .GetIdentifierOrNull(new { Identifier = "lalalala" }.GetType())
                .ShouldNotBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldUseATypeIdProperty()
        {
            DefaultMapperContext
                .Naming
                .GetIdentifierOrNull(typeof(Person))
                .ShouldNotBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldReturnNullIfNoIdentifier()
        {
            DefaultMapperContext
                .Naming
                .GetIdentifierOrNull(new { NoIdHere = true }.GetType())
                .ShouldBeNull();
        }
    }
}
