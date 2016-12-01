﻿namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class PublicReadOnlyProperty<T>
    {
        public PublicReadOnlyProperty(T readOnlyValue)
        {
            Value = readOnlyValue;
        }

        public T Value { get; }
    }
}