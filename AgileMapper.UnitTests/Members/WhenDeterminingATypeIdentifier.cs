namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenDeterminingATypeIdentifier : MemberTestsBase
    {
        [Fact]
        public void ShouldUseAnIdProperty()
        {
            MemberFinder.GetIdentifierOrNull(new { Id = "blahblahblah" }.GetType()).ShouldNotBeNull();
        }

        [Fact]
        public void ShouldUseAnIdentifierProperty()
        {
            MemberFinder.GetIdentifierOrNull(new { Identifier = "lalalala" }.GetType()).ShouldNotBeNull();
        }

        [Fact]
        public void ShouldUseATypeIdProperty()
        {
            MemberFinder.GetIdentifierOrNull(typeof(Person)).ShouldNotBeNull();
        }

        [Fact]
        public void ShouldReturnNullIfNoIdentifier()
        {
            MemberFinder.GetIdentifierOrNull(new { NoIdHere = true }.GetType()).ShouldBeNull();
        }
    }
}
