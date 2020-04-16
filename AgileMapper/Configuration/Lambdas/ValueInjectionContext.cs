namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System;
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
        private Expression _mappingDataAccess;
        private Expression _sourceAccess;
        private Expression _targetAccess;
        private Expression _createdObject;

        public ValueInjectionContext(ValueInjectionArgs args)
            : this(args, args.MapperData.MappingDataObject)
        {
        }

        public ValueInjectionContext(
            ValueInjectionArgs args,
            Expression contextAccess)
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

        public Expression GetTargetMemberAccess()
            => MapperData.GetTargetAccess(_contextAccess, _args.ContextTargetType);

        public Expression GetTargetInstanceAccess()
        {
            if (!_contextAccess.Type.IsGenericType())
            {
                return GetTargetMemberAccess();
            }

            var targetType = _args.ContextTargetType;

            var targetInstanceAccess = MapperData
                .GetAppropriateMappingContext(_contextAccess.Type.GetGenericTypeArguments())
                .TargetInstance;

            if (HasCompatibleTypes(targetType, targetInstanceAccess))
            {
                return targetInstanceAccess;
            }

            if (MapperData.TargetMember.IsEnumerable)
            {
                return ((ObjectMapperData)MapperData)
                    .EnumerablePopulationBuilder
                    .GetEnumerableConversion(targetInstanceAccess);
            }

            return targetInstanceAccess.GetConversionTo(targetType);
        }

        private static bool HasCompatibleTypes(Type targetType, Expression targetInstanceAccess)
            => !targetInstanceAccess.Type.IsValueType() && targetInstanceAccess.Type.IsAssignableTo(targetType);

        #endregion

        public bool IsCallback => Types.Length == 2;

        public Type[] Types => _args.ContextTypes;

        public IMemberMapperData MapperData => _args.MapperData;

        public Expression MappingDataAccess
            => _mappingDataAccess ??= _mappingDataAccess = GetMappingDataAccess();

        private Expression GetMappingDataAccess()
            => _args.GetTypedContextAccess(_contextAccess);

        public Expression SourceAccess => _sourceAccess ??= _sourceAccess = GetSourceAccess();

        private Expression GetSourceAccess()
        {
            return GetValueAccess(
                MapperData.GetSourceAccess(_contextAccess, _args.ContextSourceType),
                _args.ContextSourceType);
        }

        public Expression TargetAccess => _targetAccess ??= _targetAccess = GetTargetAccess();

        private Expression GetTargetAccess()
        {
            var targetAccess = (_args.InvocationPosition == InvocationPosition.Before)
                ? GetTargetMemberAccess() : GetTargetInstanceAccess();

            return GetValueAccess(targetAccess, _args.ContextTargetType);
        }

        public Expression CreatedObject => _createdObject ??= GetCreatedObject();

        private Expression GetCreatedObject()
        {
            var neededCreatedObjectType = Types.Last();
            var createdObject = MapperData.CreatedObject;

            if ((Types.Length == 3) && (neededCreatedObjectType == typeof(int?)))
            {
                return createdObject;
            }

            return GetValueAccess(createdObject, neededCreatedObjectType);
        }

        public Expression ElementIndex => MapperData.ElementIndex;

        public Expression ElementKey => MapperData.ElementKey;

        private static Expression GetValueAccess(Expression valueAccess, Type neededAccessType)
        {
            return (neededAccessType != valueAccess.Type) && valueAccess.Type.IsValueType()
                ? valueAccess.GetConversionTo(neededAccessType)
                : valueAccess;
        }
    }
}