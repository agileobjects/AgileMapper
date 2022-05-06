namespace AgileObjects.AgileMapper.Configuration.Lambdas;

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

internal class ValueReplacementContext
{
    private readonly ValueReplacementArgs _args;
    private readonly IMemberMapperData _contextMapperData;
    private readonly Expression _contextAccess;
    private readonly Expression _sourceAccess;
    private readonly Expression _targetAccess;

    public ValueReplacementContext(ValueReplacementArgs args)
        : this(args, args.MapperData, args.MapperData.MappingDataObject)
    {
    }

    public ValueReplacementContext(
        ValueReplacementArgs args,
        IMemberMapperData contextMapperData,
        Expression contextAccess)
    {
        _args = args;
        _contextMapperData = contextMapperData;
        _contextAccess = contextAccess;
    }

    public ValueReplacementContext(
        ValueReplacementArgs args,
        Expression sourceAccess,
        Expression targetAccess)
        : this(args, args.MapperData, args.MapperData.MappingDataObject)
    {
        _sourceAccess = sourceAccess;
        _targetAccess = targetAccess;
    }

    #region Target Value Factories

    private Expression GetTargetObjectAccess()
        => MapperData.GetTargetAccess(_contextMapperData, _contextAccess, _args.ContextTargetType);

    private Expression GetTargetVariableAccess()
    {
        if (!_contextAccess.Type.IsGenericType())
        {
            return GetTargetObjectAccess();
        }

        var targetType = _args.ContextTargetType;

        var mapperData = MapperData
            .GetMapperDataFor(_contextAccess.Type.GetGenericTypeArguments());

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

    public bool IsCallback() => Types.AreForCallback();

    public Type[] Types => _args.ContextTypes;

    public IMemberMapperData MapperData => _args.MapperData;

    public Expression GetToMappingDataCall()
        => MapperData.GetToMappingDataCall(Types);

    public Expression GetParentAccess()
    {
        if (MapperData.IsRoot)
        {
            return typeof(IMappingData).ToDefaultExpression();
        }

        return MapperData.Parent.GetToMappingDataCall(Types);
    }

    public Expression GetSourceAccess()
    {
        return _sourceAccess ?? GetValueAccess(
            MapperData.GetSourceAccess(_contextMapperData, _contextAccess, _args.ContextSourceType),
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

    public Expression ElementIndex => MapperData.ElementIndex;

    public Expression ElementKey => MapperData.ElementKey;

    public Expression ServiceProvider => Constants.ExecutionContextParameter;

    private static Expression GetValueAccess(Expression valueAccess, Type neededAccessType)
    {
        return (neededAccessType != valueAccess.Type) && valueAccess.Type.IsValueType()
            ? valueAccess.GetConversionTo(neededAccessType)
            : valueAccess;
    }
}