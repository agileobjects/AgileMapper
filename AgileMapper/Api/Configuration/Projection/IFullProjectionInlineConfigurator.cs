﻿namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Provides options for configuring query projections from and to given source and result element Types, inline.
    /// </summary>
    /// <typeparam name="TSourceElement">The source element Type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result element Type to which the configuration should apply.</typeparam>
    public interface IFullProjectionInlineConfigurator<TSourceElement, TResultElement> :
        IFullProjectionConfigurator<TSourceElement, TResultElement>
    {
        /// <summary>
        /// Configure how this mapper performs a projection, inline. Use this property to switch from 
        /// configuration of the root Types on which the projection is being performed to configuration 
        /// of any other Types.
        /// </summary>
        MappingConfigStartingPoint WhenMapping { get; }
    }
}
