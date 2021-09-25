namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching;
    using Extensions.Internal;
    using MapperKeys;
    using NetStandardPolyfills;
    using RepeatedMappings;

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper
    {
        private readonly ObjectMapperKeyBase _mapperKey;
        private MapperFunc<TSource, TTarget> _mapperFunc;
        private readonly object _lazyMapperFuncSync;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _subMappersByKey;
        private readonly ICache<ObjectMapperKeyBase, IRepeatedMapperFunc> _repeatedMappingFuncsByKey;
        private bool _mapperFuncCompiled;
        private Action _resetCallback;

        public ObjectMapper(Expression mapping, IObjectMappingData mappingData)
        {
            _mapperKey = mappingData.MapperKey;
            Mapping = mapping;

            var mapperData = MapperData = mappingData.MapperData;
            var mapperDataContext = mapperData.Context;

            if (mapperDataContext.Compile)
            {
                if (mappingData.MappingContext.PlanSettings.LazyCompile)
                {
                    _mapperFunc = LazyCompileMapperFunc;
                    _lazyMapperFuncSync = new object();
                }
                else
                {
                    _mapperFunc = CompileMapperFunc();
                }
            }
            else if (mapperDataContext.NeedsRuntimeTypedMapping)
            {
                mapperData.Mapper = this;
            }

            if (mapperDataContext.NeedsRuntimeTypedMapping)
            {
                _subMappersByKey = Cache.CreateNew<ObjectMapperKeyBase, IObjectMapper>();
            }

            if (mapperData.HasRepeatedMapperFuncs)
            {
                _repeatedMappingFuncsByKey = Cache.CreateNew<ObjectMapperKeyBase, IRepeatedMapperFunc>();
                mapperData.Mapper = this;

                CacheRepeatedMappingFuncs();
            }
        }

        #region Setup

        private TTarget LazyCompileMapperFunc(
            TSource source,
            TTarget target,
            IMappingExecutionContext context)
        {
            lock (_lazyMapperFuncSync)
            {
                if (_mapperFuncCompiled)
                {
                    goto DoMapping;
                }

                _mapperFunc = CompileMapperFunc();
                _mapperFuncCompiled = true;
            }

        DoMapping:
            return Map(source, target, context);
        }

        private MapperFunc<TSource, TTarget> CompileMapperFunc()
            => GetMappingLambda().Compile();

        public void CacheRepeatedMappingFuncs()
        {
            // Using a for loop here because creation of a repeated mapping func can
            // cause additions to MapperData.RequiredMapperFuncKeys
            for (var i = _repeatedMappingFuncsByKey.Count; i < MapperData.RepeatedMapperFuncKeys.Count; i++)
            {
                var mapperKey = MapperData.RepeatedMapperFuncKeys[i];

                var typesKey = new SourceAndTargetTypesKey(
                    mapperKey.MapperData.SourceType,
                    mapperKey.MapperData.TargetType);

                var mapperFuncCreator = GlobalContext.Instance.Cache.GetOrAddWithHashCodes(typesKey, key =>
                {
                    var mapperFuncType = typeof(RepeatedMapperFunc<,>).MakeGenericType(key.SourceType, key.TargetType);
                    var mapperDataParameter = Parameters.Create<IObjectMappingData>("mappingData");
                    var lazyLoadParameter = Parameters.Create<bool>("lazyLoadFuncs");

                    var mapperFuncCreation = Expression.New(
                        mapperFuncType.GetPublicInstanceConstructor(typeof(IObjectMappingData), typeof(bool)),
                        mapperDataParameter,
                        lazyLoadParameter);

                    var mapperCreationLambda = Expression.Lambda<Func<IObjectMappingData, bool, IRepeatedMapperFunc>>(
                        mapperFuncCreation,
                        mapperDataParameter,
                        lazyLoadParameter);

                    return mapperCreationLambda.Compile();
                });

                var mapperFunc = mapperFuncCreator.Invoke(
                    mapperKey.MappingData,
                    mapperKey.MappingData.MappingContext.PlanSettings.LazyLoadRepeatMappingFuncs);

                _repeatedMappingFuncsByKey.GetOrAdd(mapperKey, _ => mapperFunc);
            }
        }

        #endregion

        public Expression Mapping { get; }

        LambdaExpression IObjectMapper.GetMappingLambda() => GetMappingLambda();

        private Expression<MapperFunc<TSource, TTarget>> GetMappingLambda() 
            => Mapping.ToMappingLambda<TSource, TTarget>(MapperData);

        public ObjectMapperData MapperData { get; }

        private CacheSet Cache => MapperData.MapperContext.Cache;

        public IEnumerable<IRepeatedMapperFunc> RepeatedMappingFuncs => _repeatedMappingFuncsByKey.Values;

        public bool IsStaticallyCacheable()
        {
            if (_mapperKey.HasTypeTester)
            {
                return false;
            }

            return _subMappersByKey?.Values.All(subMapperByKey => subMapperByKey.IsStaticallyCacheable()) == true;
        }

        public object Map(MappingExecutionContextBase2 context)
            => Map((TSource)context.Source, (TTarget)context.Target, context);

        public TTarget Map(TSource source, TTarget target, IMappingExecutionContext context)
            => _mapperFunc.Invoke(source, target, context);

        public object MapSubObject(MappingExecutionContextBase2 context)
        {
            var mapper = _subMappersByKey.GetOrAdd(
                context.GetMapperKey(),
                key => key.CreateMappingData().GetOrCreateMapper());

            return mapper.Map(context);
        }

        public object MapRepeated(MappingExecutionContextBase2 context)
        {
            var mapperFunc = _repeatedMappingFuncsByKey
                .GetOrAdd(context.GetMapperKey(), null);

            return mapperFunc.Map(context);
        }

        public ObjectMapper<TSource, TTarget> WithResetCallback(Action callback)
        {
            _resetCallback = callback;
            return this;
        }

        public void Reset() => _resetCallback?.Invoke();
    }
}