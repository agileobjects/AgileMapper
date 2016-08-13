namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System.Collections.Generic;

    internal class FacebookUser : Person
    {
        public List<FacebookUser> Friends
        {
            get;
            set;
        }
    }
}