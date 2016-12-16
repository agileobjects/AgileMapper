namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    /// <summary>
    /// Provides options for configuring how mappers will perform mappings from dictionaries.
    /// </summary>
    public interface IDictionaryConfigSettings
    {
        /// <summary>
        /// Use the given <paramref name="separator"/> to separate member names when mapping to nested
        /// complex type members. For example, calling UseMemberName("-") will require a dictionary entry 
        /// with the key 'Address-Line1' to map to an Address.Line1 member.
        /// </summary>
        /// <param name="separator">
        /// The separator to use to separate member names when constructing dictionary keys for nested
        /// members.
        /// </param>
        void UseMemberNameSeparator(string separator);
    }
}