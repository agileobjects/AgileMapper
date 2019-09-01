namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.MemberIgnores;
    using AgileMapper.Configuration.Projection;
    using Dictionaries;
#if FEATURE_DYNAMIC
    using Dynamics;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using Projection;
    using ReadableExpressions.Extensions;
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

        private UserConfigurationSet UserConfigurations => MapperContext.UserConfigurations;

        #region IFullMappingInlineConfigurator Members

        public MappingConfigStartingPoint WhenMapping
            => new MappingConfigStartingPoint(MapperContext);

        IProjectionConfigStartingPoint IFullProjectionInlineConfigurator<TSource, TTarget>.WhenMapping
            => WhenMapping;

        public ITargetDictionaryMappingInlineConfigurator<TSource, TTarget> ForDictionaries
            => new TargetDictionaryMappingConfigurator<TSource, TTarget>(ConfigInfo);

#if FEATURE_DYNAMIC
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

            UserConfigurations.Add(depthSettings);
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
        {
            if (typeof(TObject).IsPrimitive())
            {
                throw new MappingConfigurationException(
                    $"Unable to configure the creation of primitive type '{typeof(TObject).GetFriendlyName()}'");
            }

            return new FactorySpecifier<TSource, TTarget, TObject>(ConfigInfo);
        }

        #endregion

        public IFullMappingSettings<TSource, TTarget> SwallowAllExceptions() => PassExceptionsTo(ctx => { });

        public IFullMappingSettings<TSource, TTarget> PassExceptionsTo(Action<IMappingExceptionData<TSource, TTarget>> callback)
        {
            UserConfigurations.Add(new ExceptionCallback(ConfigInfo, callback.ToConstantExpression()));
            return this;
        }

        public IFullMappingSettings<TSource, TTarget> MaintainIdentityIntegrity() => SetMappedObjectCaching(cache: true);

        public IFullMappingSettings<TSource, TTarget> DisableObjectTracking() => SetMappedObjectCaching(cache: false);

        private IFullMappingSettings<TSource, TTarget> SetMappedObjectCaching(bool cache)
        {
            UserConfigurations.Add(new MappedObjectCachingSetting(ConfigInfo, cache));
            return this;
        }

        public IFullMappingSettings<TSource, TTarget> MapNullCollectionsToNull()
        {
            UserConfigurations.Add(new NullCollectionsSetting(ConfigInfo));
            return this;
        }

        public IFullMappingSettings<TSource, TTarget> MapEntityKeys() => SetEntityKeyMapping(mapKeys: true);

        public IFullMappingSettings<TSource, TTarget> IgnoreEntityKeys() => SetEntityKeyMapping(mapKeys: false);

        private IFullMappingSettings<TSource, TTarget> SetEntityKeyMapping(bool mapKeys)
        {
            UserConfigurations.Add(new EntityKeyMappingSetting(ConfigInfo, mapKeys));
            return this;
        }

        public IFullMappingSettings<TSource, TTarget> AutoReverseConfiguredDataSources()
            => SetDataSourceReversal(reverse: true);

        public IFullMappingSettings<TSource, TTarget> DoNotAutoReverseConfiguredDataSources()
            => SetDataSourceReversal(reverse: false);

        private IFullMappingSettings<TSource, TTarget> SetDataSourceReversal(bool reverse)
        {
            UserConfigurations.Add(new DataSourceReversalSetting(ConfigInfo, reverse));
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

        public IMappingConfigContinuation<TSource, TTarget> IgnoreSources(
            Expression<Func<SourceValueFilterSpecifier, bool>> valuesFilter)
        {
            return IgnoreMembersByFilter(
                ConfiguredSourceValueFilter.Create(ConfigInfo, valuesFilter),
                UserConfigurations.Add);
        }

        public IMappingConfigContinuation<TSource, TTarget> IgnoreSource(params Expression<Func<TSource, object>>[] sourceMembers)
        {
            return IgnoreMembers(
                sourceMembers,
                (ci, tm) => new ConfiguredSourceMemberIgnore(ci, tm),
                UserConfigurations.Add);
        }

        public IMappingConfigContinuation<TSource, TTarget> IgnoreSourceMembersOfType<TMember>()
            => IgnoreSourceMembersWhere(member => member.HasType<TMember>());

        public IMappingConfigContinuation<TSource, TTarget> IgnoreSourceMembersWhere(
            Expression<Func<SourceMemberSelector, bool>> memberFilter)
        {
            return IgnoreSourceMembersByFilter(memberFilter);
        }

        private MappingConfigContinuation<TSource, TTarget> IgnoreSourceMembersByFilter(
            Expression<Func<SourceMemberSelector, bool>> memberFilter)
        {
            return IgnoreMembersByFilter(
                new ConfiguredSourceMemberFilter(ConfigInfo, memberFilter),
                UserConfigurations.Add);
        }

        public IMappingConfigContinuation<TSource, TTarget> Ignore(params Expression<Func<TTarget, object>>[] targetMembers)
            => IgnoreTargetMembers(targetMembers);

        IProjectionConfigContinuation<TSource, TTarget> IRootProjectionConfigurator<TSource, TTarget>.Ignore(
            params Expression<Func<TTarget, object>>[] resultMembers)
        {
            return IgnoreTargetMembers(resultMembers);
        }

        private MappingConfigContinuation<TSource, TTarget> IgnoreTargetMembers(
            IEnumerable<Expression<Func<TTarget, object>>> targetMembers)
        {
            return IgnoreMembers(
                targetMembers,
                (ci, tm) => new ConfiguredMemberIgnore(ci, tm),
                UserConfigurations.Add);
        }

        private MappingConfigContinuation<TSource, TTarget> IgnoreMembers<TMember, TConfig>(
            IEnumerable<Expression<Func<TMember, object>>> members,
            Func<MappingConfigInfo, LambdaExpression, TConfig> configuredIgnoreFactory,
            Action<TConfig> configurationsAddMethod)
            where TConfig : UserConfiguredItemBase
        {
            foreach (var member in members)
            {
                var configuredIgnoredMember = configuredIgnoreFactory.Invoke(ConfigInfo, member);

                configurationsAddMethod.Invoke(configuredIgnoredMember);
                ConfigInfo.NegateCondition();
            }

            return new MappingConfigContinuation<TSource, TTarget>(ConfigInfo);
        }

        public IMappingConfigContinuation<TSource, TTarget> IgnoreTargetMembersOfType<TMember>()
            => IgnoreTargetMembersWhere(member => member.HasType<TMember>());

        IProjectionConfigContinuation<TSource, TTarget> IRootProjectionConfigurator<TSource, TTarget>.IgnoreTargetMembersOfType<TMember>()
            => IgnoreTargetMembersByFilter(member => member.HasType<TMember>());

        public IMappingConfigContinuation<TSource, TTarget> IgnoreTargetMembersWhere(
            Expression<Func<TargetMemberSelector, bool>> memberFilter)
        {
            return IgnoreTargetMembersByFilter(memberFilter);
        }

        IProjectionConfigContinuation<TSource, TTarget> IRootProjectionConfigurator<TSource, TTarget>.IgnoreTargetMembersWhere(
            Expression<Func<TargetMemberSelector, bool>> memberFilter)
        {
            return IgnoreTargetMembersByFilter(memberFilter);
        }

        private MappingConfigContinuation<TSource, TTarget> IgnoreTargetMembersByFilter(
            Expression<Func<TargetMemberSelector, bool>> memberFilter)
        {
            return IgnoreMembersByFilter(
                new ConfiguredMemberFilter(ConfigInfo, memberFilter),
                UserConfigurations.Add);
        }

        private MappingConfigContinuation<TSource, TTarget> IgnoreMembersByFilter<TIgnore>(
            TIgnore memberIgnore,
            Action<TIgnore> configurationsAddMethod)
            where TIgnore : UserConfiguredItemBase
        {
            configurationsAddMethod.Invoke(memberIgnore);

            return new MappingConfigContinuation<TSource, TTarget>(ConfigInfo);
        }

        #endregion

        public PreEventMappingConfigStartingPoint<TSource, TTarget> Before
            => new PreEventMappingConfigStartingPoint<TSource, TTarget>(ConfigInfo);

        public PostEventMappingConfigStartingPoint<TSource, TTarget> After
            => new PostEventMappingConfigStartingPoint<TSource, TTarget>(ConfigInfo);

        #region Map Overloads

        public ICustomDataSourceMappingConfigContinuation<TSource, TTarget> Map<TSourceValue, TTargetValue>(
            Expression<Func<TSource, TSourceValue>> valueFactoryExpression,
            Expression<Func<TTarget, TTargetValue>> targetMember)
        {
            return GetValueFactoryTargetMemberSpecifier<TSourceValue>(valueFactoryExpression).To(targetMember);
        }

        public ICustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<IMappingData<TSource, TTarget>, TSourceValue>> valueFactoryExpression)
        {
            return GetValueFactoryTargetMemberSpecifier<TSourceValue>(valueFactoryExpression);
        }

        public ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TSourceValue>> valueFactoryExpression)
        {
            return GetValueFactoryTargetMemberSpecifier<TSourceValue>(valueFactoryExpression);
        }

        public ICustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, TSourceValue>> valueFactoryExpression)
        {
            return GetValueFactoryTargetMemberSpecifier<TSourceValue>(valueFactoryExpression);
        }

        public ICustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, int?, TSourceValue>> valueFactoryExpression)
        {
            return GetValueFactoryTargetMemberSpecifier<TSourceValue>(valueFactoryExpression);
        }

        public ICustomDataSourceTargetMemberSpecifier<TSource, TTarget> MapFunc<TSourceValue>(
            Func<TSource, TSourceValue> valueFunc)
            => GetConstantValueTargetMemberSpecifier(valueFunc);

        public ICustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(TSourceValue value)
            => GetConstantValueTargetMemberSpecifier(value);

        public IMappingConfigContinuation<TSource, TTarget> Map<TSourceValue, TTargetValue>(
            TSourceValue value,
            Expression<Func<TTarget, TTargetValue>> targetMember)
        {
            return GetConstantValueTargetMemberSpecifier(value).To(targetMember);
        }

        ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget> IRootProjectionConfigurator<TSource, TTarget>.Map<TSourceValue>(
            TSourceValue value)
        {
            return GetConstantValueTargetMemberSpecifier(value);
        }

        #region Map Helpers

        private CustomDataSourceTargetMemberSpecifier<TSource, TTarget> GetValueFactoryTargetMemberSpecifier<TSourceValue>(
            LambdaExpression valueFactoryExpression)
        {
            return GetValueFactoryTargetMemberSpecifier(valueFactoryExpression, typeof(TSourceValue));
        }

        internal CustomDataSourceTargetMemberSpecifier<TSource, TTarget> GetValueFactoryTargetMemberSpecifier(
            LambdaExpression valueFactoryExpression,
            Type sourceValueType)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                ConfigInfo.ForSourceValueType(sourceValueType),
                valueFactoryExpression,
                valueCouldBeSourceMember: true);
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
                valueLambda,
                valueCouldBeSourceMember: false);
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

            UserConfigurations.Add(condition);

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