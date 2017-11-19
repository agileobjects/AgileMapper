namespace AgileObjects.AgileMapper.Api.Validation
{
    /// <summary>
    /// Provides options for validating the mapping plans cached in a particular mapper.
    /// </summary>
    public interface IMapperValidationSelector
    {
        /// <summary>
        /// Throw an exception upon execution of this statement if any target members in any of the cached 
        /// mapping plans will not be mapped.
        /// </summary>
        void MembersAreNotMapped();
    }
}