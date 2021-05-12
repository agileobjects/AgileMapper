﻿namespace AgileObjects.AgileMapper
{
#if FEATURE_ARRAY_EMPTY
    using System;
#endif
    using System.Collections.Generic;

    /// <summary>
    /// Provides typed enumerable operations.
    /// </summary>
    /// <typeparam name="TElement">The type of enumerable element.</typeparam>
    public static class Enumerable<TElement>
    {
        /// <summary>
        /// Gets a singleton empty <typeparamref name="TElement"/> array instance.
        /// </summary>
        public static readonly TElement[] EmptyArray = 
#if FEATURE_ARRAY_EMPTY
            Array.Empty<TElement>();
#else
            { };
#endif

        /// <summary>
        /// Gets a singleton empty <typeparamref name="TElement"/> array instance.
        /// </summary>
        public static readonly IEnumerable<TElement> Empty = EmptyArray;
    }
}