namespace AgileObjects.AgileMapper
{
    using System;

    public class MappingException : Exception
    {
        public MappingException()
        {
        }

        public MappingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}