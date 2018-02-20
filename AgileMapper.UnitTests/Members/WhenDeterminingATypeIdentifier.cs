namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using TestClasses;
    using Xunit;

    public class WhenDeterminingATypeIdentifier : MemberTestsBase
    {
        [Fact]
        public void ShouldUseAnIdProperty()
        {
            DefaultMapperContext
                .Naming
                .GetIdentifierOrNull(new { Id = "blahblahblah" }.GetType())
                .ShouldNotBeNull();
        }

        [Fact]
        public void ShouldUseAnIdentifierProperty()
        {
            DefaultMapperContext
                .Naming
                .GetIdentifierOrNull(new { Identifier = "lalalala" }.GetType())
                .ShouldNotBeNull();
        }

        [Fact]
        public void ShouldUseATypeIdProperty()
        {
            DefaultMapperContext
                .Naming
                .GetIdentifierOrNull(typeof(Person))
                .ShouldNotBeNull();
        }

        [Fact]
        public void ShouldReturnNullIfNoIdentifier()
        {
            DefaultMapperContext
                .Naming
                .GetIdentifierOrNull(new { NoIdHere = true }.GetType())
                .ShouldBeNull();
        }
    }
}
