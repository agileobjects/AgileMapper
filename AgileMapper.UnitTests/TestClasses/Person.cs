namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System;

    internal class Person
    {
        public Guid Id
        {
            get;
            set;
        }

        public Title Title
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public Address Address
        {
            get;
            set;
        }
    }
}
