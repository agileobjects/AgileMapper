namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Projection;
    using Dictionaries;
#if DYNAMIC_SUPPORTED
    using Dynamics;
#endif
    using Extensions.Internal;
    using Members;
    using Projection;
    using Validation;

    internal class MappingConfigurator<TSource, TTarget> :
        IFullMappingInlineConfigurator<TSource, TTarget>,
        IFullProjectionInlineConfigurator<TSource, TTarget>,
        IConditionalRootMappingConfigurator<TSource, TTarget>,
        IConditionalRootProjectionConfigurator<TSource, TTarget>
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

        IProjectionConfigStartingPoint IFullProjectionInlineConfigurator<TSource, TTarget>.WhenMapping
            => WhenMapping;

        public ITargetDictionaryMappingInlineConfigurator<TSource, TTarget> ForDictionaries
            => new TargetDictionaryMappingConfigurator<TSource, TTarget>(ConfigInfo);

#if DYNAMIC_SUPPORTED
        public ITargetDynamicMappingInlineConfigurator<TSource> ForDynamics
            => new TargetDynamicMappingConfigurator<TSource>(ConfigInfo);
#endif
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

        #region IFullProjectionInlineConfigurator Members

        public IFullProjectionInlineConfigurator<TSource, TTarget> RecurseToDepth(int recursionDepth)
        {
            var depthSettings = new RecursionDepthSettings(ConfigInfo, recursionDepth);

            ConfigInfo.MapperContext.UserConfigurations.Add(depthSettings);
            return this;
        }

        public IConditionalRootProjectionConfigurator<TSource, TTarget> If(Expression<Func<TSource, bool>> condition)
            => SetCondition(condition);

        #region Naming

        IFullProjectionInlineConfigurator<TSource, TTarget> IFullProjectionInlineConfigurator<TSource, TTarget>.UseNamePrefix(
            string prefix) => ((IFullProjectionInlineConfigurator<TSource, TTarget>)this).UseNamePrefixes(prefix);

        IFullProjectionInlineConfigurator<TSource, TTarget> IFullProjectionInlineConfigurator<TSource, TTarget>.UseNamePrefixes(
            params string[] prefixes)
        {
            MapperContext.Naming.AddNamePrefixes(prefixes);
            return this;
        }

        IFullProjectionInlineConfigurator<TSource, TTarget> IFullProjectionInlineConfigurator<TSource, TTarget>.UseNameSuffix(
            string suffix) => ((IFullProjectionInlineConfigurator<TSource, TTarget>)this).UseNameSuffixes(suffix);

        IFullProjectionInlineConfigurator<TSource, TTarget> IFullProjectionInlineConfigurator<TSource, TTarget>.UseNameSuffixes(
            params string[] suffixes)
        {
            MapperContext.Naming.AddNameSuffixes(suffixes);
            return this;
        }

        IFullProjectionInlineConfigurator<TSource, TTarget> IFullProjectionInlineConfigurator<TSource, TTarget>.UseNamePattern(
            string pattern) => ((IFullProjectionInlineConfigurator<TSource, TTarget>)this).UseNamePatterns(pattern);

        IFullProjectionInlineConfigurator<TSource, TTarget> IFullProjectionInlineConfigurator<TSource, TTarget>.UseNamePatterns(
            params string[] patterns)
        {
            MapperContext.Naming.AddNameMatchers(patterns);
            return this;
        }

        #endregion

        #endregion

        #region If Overloads

        public IConditionalRootMappingConfigurator<TSource, TTarget> If(
            Expression<Func<IMappingData<TSource, TTarget>, bool>> condition)
        {
            return SetCondition(condition);
        }

        public IConditionalRootMappingConfigurator<TSource, TTarget> If(Expression<Func<TSource, TTarget, bool>> condition)
            => SetCondition(condition);

        public IConditionalRootMappingConfigurator<TSource, TTarget> If(Expression<Func<TSource, TTarget, int?, bool>> condition)
            => SetCondition(condition);

        private MappingConfigurator<TSource, TTarget> SetCondition(LambdaExpression conditionLambda)
        {
            ConfigInfo.AddConditionOrThrow(conditionLambda);
            return this;
        }

        #endregion

        #region Instance Creation

        public IMappingConfigContinuation<TSource, TTarget> CreateInstancesUsing(
            Expression<Func<IMappingData<TSource, TTarget>, TTarget>> factory)
        {
            return RegisterFactory(factory);
        }

        public IProjectionConfigContinuation<TSource, TTarget> CreateInstancesUsing(
            Expression<Func<TSource, TTarget>> factory)
        {
            return RegisterFactory(factory);
        }

        private MappingConfigContinuation<TSource, TTarget> RegisterFactory(LambdaExpression factory)
        {
            CreateFactorySpecifier<TTarget>().Using(factory);

            return new MappingConfigContinuation<TSource, TTarget>(ConfigInfo);
        }

        public IMappingConfigContinuation<TSource, TTarget> CreateInstancesUsing<TFactory>(TFactory factory)
            where TFactory : class
        {
            CreateFactorySpecifier<TTarget>().Using(factory);

            return new MappingConfigContinuation<TSource, TTarget>(ConfigInfo);
        }

        public IMappingFactorySpecifier<TSource, TTarget, TObject> CreateInstancesOf<TObject>()
            => CreateFactorySpecifier<TObject>();

        IProjectionFactorySpecifier<TSource, TTarget, TObject> IRootProjectionConfigurator<TSource, TTarget>.CreateInstancesOf<TObject>()
            => CreateFactorySpecifier<TObject>();

        private FactorySpecifier<TSource, TTarget, TObject> CreateFactorySpecifier<TObject>()
            => new FactorySpecifier<TSource, TTarget, TObject>(ConfigInfo);

        #endregion

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

        public IMappingEnumPairSpecifier<TSource, TTarget> PairEnum<TFirstEnum>(TFirstEnum enumMember)
            where TFirstEnum : struct
        {
            return EnumPairSpecifier<TSource, TTarget, TFirstEnum>.For(ConfigInfo, enumMember);
        }

        IProjectionEnumPairSpecifier<TSource, TTarget> IFullProjectionSettings<TSource, TTarget>.PairEnum<TFirstEnum>(
            TFirstEnum enumMember)
        {
            return EnumPairSpecifier<TSource, TTarget, TFirstEnum>.For(ConfigInfo, enumMember);
        }

        IFullMappingConfigurator<TSource, TTarget> IFullMappingSettings<TSource, TTarget>.And => this;

        IFullProjectionConfigurator<TSource, TTarget> IFullProjectionSettings<TSource, TTarget>.And => this;

        #region Ignoring Members

        public IMappingConfigContinuation<TSource, TTarget> IgnoreTargetMembersOfType<TMember>()
            => IgnoreMembersByFilter(member => member.HasType<TMember>());

        IProjectionConfigContinuation<TSource, TTarget> IRootProjectionConfigurator<TSource, TTarget>.IgnoreTargetMembersOfType<TMember>()
            => IgnoreMembersByFilter(member => member.HasType<TMember>());

        public IMappingConfigContinuation<TSource, TTarget> IgnoreTargetMembersWhere(
            Expression<Func<TargetMemberSelector, bool>> memberFilter)
        {
            return IgnoreMembersByFilter(memberFilter);
        }

        IProjectionConfigContinuation<TSource, TTarget> IRootProjectionConfigurator<TSource, TTarget>.IgnoreTargetMembersWhere(
            Expression<Func<TargetMemberSelector, bool>> memberFilter)
        {
            return IgnoreMembersByFilter(memberFilter);
        }

        private MappingConfigContinuation<TSource, TTarget> IgnoreMembersByFilter(
            Expression<Func<TargetMemberSelector, bool>> memberFilter)
        {
#if NET35
            var configuredIgnoredMember = new ConfiguredIgnoredMember(ConfigInfo, memberFilter.ToDlrExpression());
#else
            var configuredIgnoredMember = new ConfiguredIgnoredMember(ConfigInfo, memberFilter);
#endif
            MapperContext.UserConfigurations.Add(configuredIgnoredMember);

            return new MappingConfigContinuation<TSource, TTarget>(ConfigInfo);
        }

        public IMappingConfigContinuation<TSource, TTarget> Ignore(params Expression<Func<TTarget, object>>[] targetMembers)
            => IgnoreMembers(targetMembers);

        IProjectionConfigContinuation<TSource, TTarget> IRootProjectionConfigurator<TSource, TTarget>.Ignore(
            params Expression<Func<TTarget, object>>[] resultMembers)
        {
            return IgnoreMembers(resultMembers);
        }

        private MappingConfigContinuation<TSource, TTarget> IgnoreMembers(
            IEnumerable<Expression<Func<TTarget, object>>> targetMembers)
        {
            foreach (var targetMember in targetMembers)
            {
#if NET35
                var configuredIgnoredMember = new ConfiguredIgnoredMember(ConfigInfo, targetMember.ToDlrExpression());
#else
                var configuredIgnoredMember = new ConfiguredIgnoredMember(ConfigInfo, targetMember);
#endif
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

        public ICustomMappingDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<IMappingData<TSource, TTarget>, TSourceValue>> valueFactoryExpression)
        {
            return GetValueFactoryTargetMemberSpecifier<TSourceValue>(valueFactoryExpression);
        }

        public ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TSourceValue>> valueFactoryExpression)
        {
            return GetValueFactoryTargetMemberSpecifier<TSourceValue>(valueFactoryExpression);
        }

        public ICustomMappingDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, TSourceValue>> valueFactoryExpression)
        {
            return GetValueFactoryTargetMemberSpecifier<TSourceValue>(valueFactoryExpression);
        }

        public ICustomMappingDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, int?, TSourceValue>> valueFactoryExpression)
        {
            return GetValueFactoryTargetMemberSpecifier<TSourceValue>(valueFactoryExpression);
        }

        public ICustomMappingDataSourceTargetMemberSpecifier<TSource, TTarget> MapFunc<TSourceValue>(
            Func<TSource, TSourceValue> valueFunc)
            => GetConstantValueTargetMemberSpecifier(valueFunc);

        public ICustomMappingDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(TSourceValue value)
            => GetConstantValueTargetMemberSpecifier(value);

        ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget> IRootProjectionConfigurator<TSource, TTarget>.Map<TSourceValue>(
            TSourceValue value)
        {
            return GetConstantValueTargetMemberSpecifier(value);
        }

        #region Map Helpers

        private CustomDataSourceTargetMemberSpecifier<TSource, TTarget> GetValueFactoryTargetMemberSpecifier<TSourceValue>(
            LambdaExpression valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                ConfigInfo.ForSourceValueType<TSourceValue>(),
                valueFactoryExpression);
        }

        private CustomDataSourceTargetMemberSpecifier<TSource, TTarget> GetConstantValueTargetMemberSpecifier<TSourceValue>(
            TSourceValue value)
        {
            var valueLambdaInfo = ConfiguredLambdaInfo.ForFunc(value, typeof(TSource), typeof(TTarget));

            if (valueLambdaInfo != null)
            {
                return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                    ConfigInfo.ForSourceValueType(valueLambdaInfo.ReturnType),
                    valueLambdaInfo);
            }
#if NET35
            var valueConstant = Expression.Constant(value, typeof(TSourceValue));
#else
            var valueConstant = value.ToConstantExpression();
#endif
            var valueLambda = Expression.Lambda<Func<TSourceValue>>(valueConstant);

            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                ConfigInfo.ForSourceValueType(valueConstant.Type),
                valueLambda);
        }

        #endregion

        public IMappingConfigContinuation<TSource, TTarget> MapTo<TDerivedTarget>()
            where TDerivedTarget : TTarget
        {
            var derivedTypePair = new DerivedPairTargetTypeSpecifier<TSource, TSource, TTarget>(ConfigInfo);

            return derivedTypePair.To<TDerivedTarget>();
        }

        IProjectionConfigContinuation<TSource, TTarget> IConditionalRootProjectionConfigurator<TSource, TTarget>.MapTo<TDerivedResult>()
        {
            IProjectionDerivedPairTargetTypeSpecifier<TSource, TTarget> derivedTypePair =
                new DerivedPairTargetTypeSpecifier<TSource, TSource, TTarget>(ConfigInfo);

            return derivedTypePair.To<TDerivedResult>();
        }

        public IMappingConfigContinuation<TSource, TTarget> MapToNull()
            => RegisterMapToNullCondition();

        IProjectionConfigContinuation<TSource, TTarget> IConditionalRootProjectionConfigurator<TSource, TTarget>.MapToNull()
            => RegisterMapToNullCondition();

        private MappingConfigContinuation<TSource, TTarget> RegisterMapToNullCondition()
        {
            var condition = new MapToNullCondition(ConfigInfo);

            MapperContext.UserConfigurations.Add(condition);

            return new MappingConfigContinuation<TSource, TTarget>(ConfigInfo);
        }

        public IMappingDerivedPairTargetTypeSpecifier<TSource, TTarget> Map<TDerivedSource>()
            where TDerivedSource : TSource
        {
            return new DerivedPairTargetTypeSpecifier<TSource, TDerivedSource, TTarget>(ConfigInfo);
        }

        #endregion
    }
}