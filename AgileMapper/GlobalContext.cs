namespace AgileObjects.AgileMapper
{
    using Members;

    internal class GlobalContext
    {
        public static readonly GlobalContext Default = new GlobalContext();

        private GlobalContext()
        {
            MemberFinder = new MemberFinder();
        }

        public MemberFinder MemberFinder { get; }
    }
}