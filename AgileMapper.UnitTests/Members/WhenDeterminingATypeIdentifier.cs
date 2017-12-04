namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using AgileMapper.Members;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenDeterminingATypeIdentifier : MemberTestsBase
    {
        [Fact]
        public void ShouldUseAnIdProperty()
        {
            DefaultMapperContext
                .Naming
                .GetIdentifierOrNull(TypeKey.ForTypeId(new { Id = "blahblahblah" }.GetType()))
                .ShouldNotBeNull();
        }

        [Fact]
        public void ShouldUseAnIdentifierProperty()
        {
            DefaultMapperContext
                .Naming
                .GetIdentifierOrNull(TypeKey.ForTypeId(new { Identifier = "lalalala" }.GetType()))
                .ShouldNotBeNull();
        }

        [Fact]
        public void ShouldUseATypeIdProperty()
        {
            DefaultMapperContext
                .Naming
                .GetIdentifierOrNull(TypeKey.ForTypeId(typeof(Person)))
                .ShouldNotBeNull();
        }

        [Fact]
        public void ShouldReturnNullIfNoIdentifier()
        {
            DefaultMapperContext
                .Naming
                .GetIdentifierOrNull(TypeKey.ForTypeId(new { NoIdHere = true }.GetType()))
                .ShouldBeNull();
        }
    }
}
