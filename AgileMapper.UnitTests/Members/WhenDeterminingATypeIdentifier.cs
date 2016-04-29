namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using AgileMapper.Members;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenDeterminingATypeIdentifier
    {
        private static readonly MemberFinder _memberFinder = new MemberFinder();

        [Fact]
        public void ShouldUseAnIdProperty()
        {
            _memberFinder.GetIdentifierOrNull(new { Id = "blahblahblah" }.GetType()).ShouldNotBeNull();
        }

        [Fact]
        public void ShouldUseAnIdentifierProperty()
        {
            _memberFinder.GetIdentifierOrNull(new { Identifier = "lalalala" }.GetType()).ShouldNotBeNull();
        }

        [Fact]
        public void ShouldUseATypeIdProperty()
        {
            _memberFinder.GetIdentifierOrNull(typeof(Person)).ShouldNotBeNull();
        }

        [Fact]
        public void ShouldReturnNullIfNoIdentifier()
        {
            _memberFinder.GetIdentifierOrNull(new { NoIdHere = true }.GetType()).ShouldBeNull();
        }
    }
}
