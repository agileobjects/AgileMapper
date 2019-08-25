namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides options to configure conditions under which a source value should not be used to
    /// populate a target member.
    /// </summary>
    public class SourceValueIgnoreSpecifier
    {
        /// <summary>
        /// Ignore any source values which match the given <paramref name="valueFilter"/>.
        /// </summary>
        /// <param name="valueFilter">
        /// The matching function with which to test a source value to determine if it should be ignored.
        /// </param>
        /// <returns>
        /// A boolean value, in order to enable composition of an ignore predicate with multiple clauses.
        /// </returns>
        public bool If(Expression<Func<object, bool>> valueFilter) => If<object>(valueFilter);

        /// <summary>
        /// Ignore any source values of type <typeparamref name="TMember"/> which match the given
        /// <paramref name="valueFilter"/>.
        /// </summary>
        /// <typeparam name="TMember">
        /// The type of source member to which the <paramref name="valueFilter"/> should be applied.
        /// </typeparam>
        /// <param name="valueFilter">
        /// The matching function with which to test a source value to determine if it should be ignored.
        /// </param>
        /// <returns>
        /// A boolean value, in order to enable composition of an ignore predicate with multiple clauses.
        /// </returns>
        public bool If<TMember>(Expression<Func<TMember, bool>> valueFilter)
        {
            return true;
        }
    }
}