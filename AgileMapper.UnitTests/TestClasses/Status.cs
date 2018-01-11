namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System;

    [Flags]
    public enum Status
    {
        New = 1,
        Assigned = 2,
        InProgress = 4,
        Completed = 8,
        Cancelled = 16,
        Removed = 32
    }
}