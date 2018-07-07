#if DYNAMIC_SUPPORTED
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    /// <summary>
    /// Provides options for configuring mappings from a <typeparamref name="TSource"/> to an ExpandoObject, inline.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    public interface ITargetDynamicMappingInlineConfigurator<TSource> :
        ITargetDynamicMappingConfigurator<TSource>
    {
    }
}
#endif