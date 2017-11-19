namespace AgileObjects.AgileMapper.Api.Validation
{
    /// <summary>
    /// Provides options for inline validation of a particular mapping.
    /// </summary>
    public interface IMappingValidationSelector
    {
        /// <summary>
        /// Throw an exception upon execution of this statement if any target members in the mapping to be 
        /// executed will not be mapped.
        /// </summary>
        void MembersAreNotMapped();
    }
}