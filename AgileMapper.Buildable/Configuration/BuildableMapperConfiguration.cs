namespace AgileObjects.AgileMapper.Buildable.Configuration
{
    using AgileMapper.Configuration;

    /// <summary>
    /// Base class for multiple, buildable mapper configuration. Derived classes will be executed
    /// to set up mapping configurations for which Mapper source code should be generated.
    /// </summary>
    public abstract class BuildableMapperConfiguration : MapperConfiguration
    {
    }
}
