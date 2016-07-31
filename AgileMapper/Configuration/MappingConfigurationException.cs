namespace AgileObjects.AgileMapper.Configuration
{
    using System;

    public class MappingConfigurationException : Exception
    {
        public MappingConfigurationException()
        {
        }

        public MappingConfigurationException(string message)
            : base(message)
        {
        }
    }
}