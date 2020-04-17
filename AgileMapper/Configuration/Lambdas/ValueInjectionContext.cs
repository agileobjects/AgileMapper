namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;

    internal class ValueInjectionContext
    {
        private readonly ValueInjectionArgs _args;
        private readonly Expression _contextAccess;
        private readonly Expression _sourceAccess;
        private readonly Expression _targetAccess;

        public ValueInjectionContext(ValueInjectionArgs args)
            : this(args, args.MapperData.MappingDataObject)
        {
        }

        public ValueInjectionContext(ValueInjectionArgs args, Expression contextAccess)
        {
            _args = args;
            _contextAccess = contextAccess;
        }

        public ValueInjectionContext(
            ValueInjectionArgs args,
            Expression contextAccess,
            Expression sourceAccess,
            Expression targetAccess)
            : this(args, contextAccess)
        {
            _sourceAccess = sourceAccess;
            _targetAccess = targetAccess;
        }

        #region Target Value Factories

        private Expression GetTargetObjectAccess()
            => MapperData.GetTargetAccess(_contextAccess, _args.ContextTargetType);

        private Expression GetTargetVariableAccess()
        {
            if (!_contextAccess.Type.IsGenericType())
            {
                return GetTargetObjectAccess();
            }

            var targetType = _args.ContextTargetType;

            var mapperData = MapperData
                .GetAppropriateMappingContext(_contextAccess.Type.GetGenericTypeArguments());

            var targetInstanceAccess = mapperData.TargetInstance;

            if (HasCompatibleTypes(targetType, targetInstanceAccess))
            {
                return targetInstanceAccess;
            }

            if (mapperData.TargetMember.IsEnumerable)
            {
                return ((ObjectMapperData)mapperData)
                    .EnumerablePopulationBuilder
                    .GetEnumerableConversion(targetInstanceAccess);
            }

            return targetInstanceAccess.GetConversionTo(targetType);
        }

        private static bool HasCompatibleTypes(Type targetType, Expression targetInstanceAccess)
            => !targetInstanceAccess.Type.IsValueType() && targetInstanceAccess.Type.IsAssignableTo(targetType);

        #endregion

        public bool IsCallback() => IsCallback(Types);

        public bool IsCallback(ICollection<Type> contextTypes) => contextTypes.Count == 2;

        public Type[] Types => _args.ContextTypes;

        private IMemberMapperData MapperData => _args.MapperData;

        public Expression GetMappingDataAccess()
            => MapperData.GetTypedContextAccess(_contextAccess, Types);

        public Expression GetParentAccess() => MapperData.ParentObject;

        public Expression GetSourceAccess()
        {
            return _sourceAccess ?? GetValueAccess(
                MapperData.GetSourceAccess(_contextAccess, _args.ContextSourceType),
                _args.ContextSourceType);
        }

        public Expression GetTargetAccess()
        {
            if (_targetAccess != null)
            {
                return _targetAccess;
            }

            var targetAccess = _args.UseTargetObject
                ? GetTargetObjectAccess() : GetTargetVariableAccess();

            return GetValueAccess(targetAccess, _args.ContextTargetType);
        }

        public Expression GetCreatedObject()
        {
            var neededCreatedObjectType = Types.Last();
            var createdObject = MapperData.CreatedObject;

            if ((Types.Length == 3) && (neededCreatedObjectType == typeof(int?)))
            {
                return createdObject;
            }

            return GetValueAccess(createdObject, neededCreatedObjectType);
        }

        public Expression GetElementIndex() => MapperData.ElementIndex;

        public Expression GetElementKey() => MapperData.ElementKey;

        private static Expression GetValueAccess(Expression valueAccess, Type neededAccessType)
        {
            return (neededAccessType != valueAccess.Type) && valueAccess.Type.IsValueType()
                ? valueAccess.GetConversionTo(neededAccessType)
                : valueAccess;
        }
    }
}