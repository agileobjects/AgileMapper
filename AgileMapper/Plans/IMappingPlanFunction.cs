namespace AgileObjects.AgileMapper.Plans
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using ReadableExpressions;

    /// <summary>
    /// Implementing classes will describe a mapping function used to map from one type to another
    /// with a particular rule set.
    /// </summary>
    public interface IMappingPlanFunction
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IMappingPlanFunction"/> describes the
        /// root mapping of its <see cref="IMappingPlan"/>.
        /// </summary>
        bool IsRoot { get; }

        /// <summary>
        /// Gets the source type from which this <see cref="IMappingPlanFunction"/> will perform a
        /// mapping.
        /// </summary>
        Type SourceType { get; }

        /// <summary>
        /// Gets the target type to which this <see cref="IMappingPlanFunction"/> will perform a
        /// mapping.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMappingPlanFunction"/> includes mapping
        /// of the <see cref="SourceType"/> and <see cref="TargetType"/>.
        /// </summary>
        bool HasDerivedTypes { get; }

        /// <summary>
        /// Gets an Expression summarising the <see cref="IMappingPlanFunction"/>.
        /// </summary>
        CommentExpression Summary { get; }

        /// <summary>
        /// Gets an Expression describing the <see cref="IMappingPlanFunction"/>'s mapping.
        /// </summary>
        LambdaExpression Mapping { get; }

        /// <summary>
        /// Gets a C# source-code string translation of this <see cref="IMappingPlanFunction"/>.
        /// </summary>
        /// <returns>A C# source-code string translation of this <see cref="IMappingPlanFunction"/>.</returns>
        string ToCSharp();
    }
}