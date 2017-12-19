﻿namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using AgileMapper.Configuration;
    using Extensions.Internal;
    using Members;
    using Validation;

    internal class MappingConfigurator<TSource, TTarget> :
        IFullMappingInlineConfigurator<TSource, TTarget>,
        IConditionalRootMappingConfigurator<TSource, TTarget>
    {
        public MappingConfigurator(MappingConfigInfo configInfo)
        {
            ConfigInfo = configInfo;

            if ((ConfigInfo.TargetType ?? typeof(object)) == typeof(object))
            {
                ConfigInfo.ForTargetType<TTarget>();
            }
        }

        protected MappingConfigInfo ConfigInfo { get; }

        protected MapperContext MapperContext => ConfigInfo.MapperContext;

        #region IFullMappingInlineConfigurator Members

        public MappingConfigStartingPoint WhenMapping
            => new MappingConfigStartingPoint(MapperContext);

        public void ThrowNowIfMappingPlanIsIncomplete() => MappingValidator.Validate(ConfigInfo);

        public IFullMappingInlineConfigurator<TSource, TTarget> LookForDerivedTypesIn(params Assembly[] assemblies)
        {
            MappingConfigStartingPoint.SetDerivedTypeAssemblies(assemblies);
            return this;
        }

        #region Naming

        public IFullMappingInlineConfigurator<TSource, TTarget> UseNamePrefix(string prefix) => UseNamePrefixes(prefix);

        public IFullMappingInlineConfigurator<TSource, TTarget> UseNamePrefixes(params string[] prefixes)
        {
            MapperContext.Naming.AddNamePrefixes(prefixes);
            return this;
        }

        public IFullMappingInlineConfigurator<TSource, TTarget> UseNameSuffix(string suffix) => UseNameSuffixes(suffix);

        public IFullMappingInlineConfigurator<TSource, TTarget> UseNameSuffixes(params string[] suffixes)
        {
            MapperContext.Naming.AddNameSuffixes(suffixes);
            return this;
        }

        public IFullMappingInlineConfigurator<TSource, TTarget> UseNamePattern(string pattern) => UseNamePatterns(pattern);

        public IFullMappingInlineConfigurator<TSource, TTarget> UseNamePatterns(params string[] patterns)
        {
            MapperContext.Naming.AddNameMatchers(patterns);
            return this;
        }

        #endregion

        #endregion

        #region If Overloads

        public IConditionalRootMappingConfigurator<TSource, TTarget> If(
            Expression<Func<IMappingData<TSource, TTarget>, bool>> condition)
            => SetCondition(condition);

        public IConditionalRootMappingConfigurator<TSource, TTarget> If(Expression<Func<TSource, TTarget, bool>> condition)
            => SetCondition(condition);

        public IConditionalRootMappingConfigurator<TSource, TTarget> If(Expression<Func<TSource, TTarget, int?, bool>> condition)
            => SetCondition(condition);

        private IConditionalRootMappingConfigurator<TSource, TTarget> SetCondition(LambdaExpression conditionLambda)
        {
            ConfigInfo.AddConditionOrThrow(conditionLambda);
            return this;
        }

        #endregion

        public MappingConfigContinuation<TSource, TTarget> CreateInstancesUsing(
            Expression<Func<IMappingData<TSource, TTarget>, TTarget>> factory)
        {
            new FactorySpecifier<TSource, TTarget, TTarget>(ConfigInfo).Using(factory);

            return new MappingConfigContinuation<TSource, TTarget>(ConfigInfo);
        }

        public MappingConfigContinuation<TSource, TTarget> CreateInstancesUsing<TFactory>(TFactory factory) where TFactory : class
        {
            new FactorySpecifier<TSource, TTarget, TTarget>(ConfigInfo).Using(factory);

            return new MappingConfigContinuation<TSource, TTarget>(ConfigInfo);
        }

        public IFactorySpecifier<TSource, TTarget, TObject> CreateInstancesOf<TObject>() where TObject : class
            => new FactorySpecifier<TSource, TTarget, TObject>(ConfigInfo);

        public IFullMappingSettings<TSource, TTarget> SwallowAllExceptions() => PassExceptionsTo(ctx => { });

        public IFullMappingSettings<TSource, TTarget> PassExceptionsTo(Action<IMappingExceptionData<TSource, TTarget>> callback)
        {
            var exceptionCallback = new ExceptionCallback(ConfigInfo, callback.ToConstantExpression());

            MapperContext.UserConfigurations.Add(exceptionCallback);
            return this;
        }

        public IFullMappingSettings<TSource, TTarget> MaintainIdentityIntegrity() => SetMappedObjectCaching(cache: true);

        public IFullMappingSettings<TSource, TTarget> DisableObjectTracking() => SetMappedObjectCaching(cache: false);

        private IFullMappingSettings<TSource, TTarget> SetMappedObjectCaching(bool cache)
        {
            var settings = new MappedObjectCachingSettings(ConfigInfo, cache);

            MapperContext.UserConfigurations.Add(settings);
            return this;
        }

        public IFullMappingSettings<TSource, TTarget> MapNullCollectionsToNull()
        {
            var nullSetting = new NullCollectionsSetting(ConfigInfo);

            MapperContext.UserConfigurations.Add(nullSetting);
            return this;
        }

        public EnumPairSpecifier<TSource, TTarget, TFirstEnum> PairEnum<TFirstEnum>(TFirstEnum enumMember)
            where TFirstEnum : struct
            => EnumPairSpecifier<TSource, TTarget, TFirstEnum>.For(ConfigInfo, new[] { enumMember });

        IFullMappingConfigurator<TSource, TTarget> IFullMappingSettings<TSource, TTarget>.And => this;

        #region Ignoring Members

        public MappingConfigContinuation<TSource, TTarget> IgnoreTargetMembersOfType<TMember>()
        {
            return IgnoreTargetMembersWhere(member => member.HasType<TMember>());
        }

        public MappingConfigContinuation<TSource, TTarget> IgnoreTargetMembersWhere(
            Expression<Func<TargetMemberSelector, bool>> memberFilter)
        {
            var configuredIgnoredMember = new ConfiguredIgnoredMember(ConfigInfo, memberFilter);

            MapperContext.UserConfigurations.Add(configuredIgnoredMember);

            return new MappingConfigContinuation<TSource, TTarget>(ConfigInfo);
        }

        public MappingConfigContinuation<TSource, TTarget> Ignore(params Expression<Func<TTarget, object>>[] targetMembers)
        {
            foreach (var targetMember in targetMembers)
            {
                var configuredIgnoredMember =
                    new ConfiguredIgnoredMember(ConfigInfo, targetMember);

                MapperContext.UserConfigurations.Add(configuredIgnoredMember);
                ConfigInfo.NegateCondition();
            }

            return new MappingConfigContinuation<TSource, TTarget>(ConfigInfo);
        }

        #endregion

        public PreEventMappingConfigStartingPoint<TSource, TTarget> Before
            => new PreEventMappingConfigStartingPoint<TSource, TTarget>(ConfigInfo);

        public PostEventMappingConfigStartingPoint<TSource, TTarget> After
            => new PostEventMappingConfigStartingPoint<TSource, TTarget>(ConfigInfo);

        #region Map Overloads

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<IMappingData<TSource, TTarget>, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                ConfigInfo.ForSourceValueType<TSourceValue>(),
                valueFactoryExpression);
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                ConfigInfo.ForSourceValueType<TSourceValue>(),
                valueFactoryExpression);
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, int?, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                ConfigInfo.ForSourceValueType<TSourceValue>(),
                valueFactoryExpression);
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> MapFunc<TSourceValue>(
            Func<TSource, TSourceValue> valueFunc)
            => GetConstantValueTargetMemberSpecifier(valueFunc);

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(TSourceValue value)
        {
            var valueLambdaInfo = ConfiguredLambdaInfo.ForFunc(value, typeof(TSource), typeof(TTarget));

            return (valueLambdaInfo != null)
                ? new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                    ConfigInfo.ForSourceValueType(valueLambdaInfo.ReturnType),
                    valueLambdaInfo)
                : GetConstantValueTargetMemberSpecifier(value);
        }

        #region Map Helpers

        private CustomDataSourceTargetMemberSpecifier<TSource, TTarget> GetConstantValueTargetMemberSpecifier<TSourceValue>(
            TSourceValue value)
        {
            var valueConstant = value.ToConstantExpression();
            var valueLambda = Expression.Lambda<Func<TSourceValue>>(valueConstant);

            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                ConfigInfo.ForSourceValueType(valueConstant.Type),
                valueLambda);
        }

        #endregion

        public MappingConfigContinuation<TSource, TTarget> MapTo<TDerivedTarget>()
            where TDerivedTarget : TTarget
        {
            var derivedTypePair = new DerivedPairTargetTypeSpecifier<TSource, TSource, TTarget>(ConfigInfo);

            return derivedTypePair.To<TDerivedTarget>();
        }

        public MappingConfigContinuation<TSource, TTarget> MapToNull()
        {
            var condition = new MapToNullCondition(ConfigInfo);

            MapperContext.UserConfigurations.Add(condition);

            return new MappingConfigContinuation<TSource, TTarget>(ConfigInfo);
        }

        public DerivedPairTargetTypeSpecifier<TSource, TDerivedSource, TTarget> Map<TDerivedSource>() where TDerivedSource : TSource
            => new DerivedPairTargetTypeSpecifier<TSource, TDerivedSource, TTarget>(ConfigInfo);

        #endregion
    }
}