namespace AgileObjects.AgileMapper
{
    using System;
    using System.Reflection;

    public class MappingException : Exception
    {
        internal static readonly ConstructorInfo ConstructorInfo =
            typeof(MappingException).GetConstructor(new[] { typeof(Exception) });

        public MappingException()
        {
        }

        public MappingException(Exception innerException)
            : base("An exception occurred during mapping", innerException)
        {
        }
    }
}