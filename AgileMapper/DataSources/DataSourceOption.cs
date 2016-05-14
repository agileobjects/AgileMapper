namespace AgileObjects.AgileMapper.DataSources
{
    using System;

    [Flags]
    internal enum DataSourceOption
    {
        None = 0,
        ExcludeConfigured = 1,
        ExcludeComplexTypeMapping = 2
    }
}