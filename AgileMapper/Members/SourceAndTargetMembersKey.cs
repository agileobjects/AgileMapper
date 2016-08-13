namespace AgileObjects.AgileMapper.Members
{
    internal class SourceAndTargetMembersKey
    {
        private readonly int _hashCode;

        public SourceAndTargetMembersKey(IQualifiedMember sourceMember, IQualifiedMember targetMember)
        {
            _hashCode = (sourceMember.Signature + ">" + targetMember.Signature).GetHashCode();
        }

        public override int GetHashCode() => _hashCode;
    }
}