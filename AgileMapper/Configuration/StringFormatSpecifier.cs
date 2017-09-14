namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using TypeConversion;

    /// <summary>
    /// Enables specification of a formatting string to use when mapping from a particular source type to
    /// a string.
    /// </summary>
    public class StringFormatSpecifier
    {
        private readonly MapperContext _mapperContext;
        private readonly Type _sourceType;

        internal StringFormatSpecifier(MapperContext mapperContext, Type sourceType)
        {
            _mapperContext = mapperContext;
            _sourceType = sourceType;
        }

        /// <summary>
        /// Specify the formatting string to use when mapping from the specified source type to a string.
        /// </summary>
        /// <param name="format">
        /// The formatting string to use when mapping from the specified source type to a string.
        /// </param>
        public void FormatUsing(string format)
        {
            _mapperContext.ValueConverters.Add(new ToFormattedStringConverter(_sourceType, format));
        }
    }
}
