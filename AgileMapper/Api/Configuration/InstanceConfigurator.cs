namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Configuration;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using System.Linq.Expressions;
#if NET35
    using static Microsoft.Scripting.Ast.Expression;
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using UnaryExpression = Microsoft.Scripting.Ast.UnaryExpression;
#else
    using static System.Linq.Expressions.Expression;
    using Expression = System.Linq.Expressions.Expression;
    using ExpressionType = System.Linq.Expressions.ExpressionType;
    using UnaryExpression = System.Linq.Expressions.UnaryExpression;
#endif

    /// <summary>
    /// Provides options for configuring mappings of the type specified by the type argument.
    /// </summary>
    /// <typeparam name="TObject">The type of object to which the configuration should apply.</typeparam>
    public class InstanceConfigurator<TObject> where TObject : class
    {
        private readonly MappingConfigInfo _configInfo;

        internal InstanceConfigurator(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        /// <summary>
        /// Use the given <paramref name="idExpression"/> to uniquely identify instances of the type being configured.
        /// </summary>
        /// <typeparam name="TId">
        /// The type of the expression to use to uniquely identify instances of the type being configured.
        /// </typeparam>
        /// <param name="idExpression">
        /// The expression to use to uniquely identify instances of the type being configured.
        /// </param>
        public void IdentifyUsing<TId>(Expression<Func<TObject, TId>> idExpression)
        {
            _configInfo.MapperContext.UserConfigurations.Identifiers.Add(
                typeof(TObject),
                idExpression
#if NET35
                .ToDlrExpression()
#endif
            );
        }

        /// <summary>
        /// Use a composite identifier composed of the given <paramref name="idExpressions"/> to
        /// uniquely identify instances of the type being configured.
        /// </summary>
        /// <param name="idExpressions">
        /// The expressions to use to uniquely identify instances of the type being configured.
        /// </param>
        public void IdentifyUsing(params Expression<Func<TObject, object>>[] idExpressions)
        {
            if (idExpressions.NoneOrNull())
            {
                throw new MappingConfigurationException(
                    "Two or more composite identifier values must be specified.",
                    new ArgumentException(nameof(idExpressions)));
            }

            if (idExpressions.Any(a => a == null))
            {
                throw new MappingConfigurationException(
                    "All supplied composite identifier values must be non-null.",
                    new ArgumentNullException(nameof(idExpressions)));
            }

            var idParts = idExpressions
#if NET35
                .ProjectToArray(id => id.ToDlrExpression());
#else
                ;
#endif
            var compositeIdParts = new List<Expression>((idParts.Length * 2) - 1);
            var entityParameter = idParts.First().Parameters.First();

            compositeIdParts.Add(GetIdPart(idParts.First().Body));

            for (var i = 1; i < idParts.Length;)
            {
                var idPart = GetIdPart(idParts[i++].ReplaceParameterWith(entityParameter));

                compositeIdParts.Add(StringExpressionExtensions.Underscore);
                compositeIdParts.Add(idPart);
            }

            var compositeId = compositeIdParts.GetStringConcatCall();

            var compositeIdLambda = Lambda<Func<TObject, string>>(compositeId, entityParameter);

            _configInfo.MapperContext.UserConfigurations.Identifiers.Add(typeof(TObject), compositeIdLambda);
        }

        private Expression GetIdPart(Expression idPart)
        {
            if ((idPart.NodeType == ExpressionType.Convert))
            {
                return GetStringIdPart(((UnaryExpression)idPart).Operand);
            }

            if (idPart.Type.IsSimple())
            {
                return GetStringIdPart(idPart);
            }

            var typeIdentifier = _configInfo
                .MapperContext
                .GetIdentifierOrNull(idPart);

            if (typeIdentifier != null)
            {
                return GetStringIdPart(typeIdentifier);
            }

            throw new MappingConfigurationException();
        }

        private Expression GetStringIdPart(Expression idPart)
        {
            return (idPart.Type != typeof(string))
                ? _configInfo.MapperContext.GetValueConversion(idPart, typeof(string))
                : idPart;
        }

        /// <summary>
        /// Use the given <paramref name="factory"/> expression to create instances of the type being configured.
        /// The factory expression is passed a context object containing the current mapping's source and target 
        /// objects in untyped properties.
        /// </summary>
        /// <param name="factory">
        /// The factory expression to use to create instances of the type being configured.
        /// </param>
        public void CreateUsing(Expression<Func<IMappingData<object, object>, TObject>> factory)
            => new FactorySpecifier<object, object, TObject>(_configInfo).Using(factory);

        /// <summary>
        /// Use the given <paramref name="factory"/> function to create instances of the type being configured.
        /// The following factory function signatures are supported:
        /// <para>
        /// Func&lt;TObject&gt; - parameterless.
        /// </para>
        /// <para>
        /// Func&lt;IMappingData&lt;object, object&gt;, TObject&gt; - taking a context object containing the 
        /// current mapping's source and target objects in untyped properties.
        /// </para>
        /// <para>
        /// Func&lt;object, object, TObject&gt; - taking the source and target objects as untyped arguments.
        /// </para>
        /// <para>
        /// Func&lt;object, object, int?, TObject&gt; - taking the source and target objects as untyped 
        /// arguments and the current enumerable index, if applicable.
        /// </para>
        /// </summary>
        /// <param name="factory">
        /// The factory function to use to create instances of the type being configured.
        /// </param>
        public void CreateUsing<TFactory>(TFactory factory) where TFactory : class
            => new FactorySpecifier<object, object, TObject>(_configInfo).Using(factory);
    }
}