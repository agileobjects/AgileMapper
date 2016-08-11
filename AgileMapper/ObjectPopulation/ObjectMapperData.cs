namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using DataSources;
    using Extensions;
    using Members;

    internal class ObjectMapperData : MemberMapperData
    {
        #region Cached Items
        // ReSharper disable StaticMemberInGenericType

        private static readonly MethodInfo _mapObjectMethod = typeof(ObjectMapperData)
            .GetMethods(Constants.PublicInstance)
            .First(m => (m.Name == "Map") && (m.GetParameters().Length == 4));

        private static readonly MethodInfo _mapEnumerableElementMethod = typeof(ObjectMapperData)
            .GetMethods(Constants.PublicInstance)
            .First(m => (m.Name == "Map") && (m.GetParameters().Length == 1));

        // ReSharper restore StaticMemberInGenericType
        #endregion

        private readonly IQualifiedMember _sourceElementMember;
        private readonly QualifiedMember _targetElementMember;
        private readonly Dictionary<string, Tuple<QualifiedMember, DataSourceSet>> _dataSourcesByTargetMemberName;
        private ObjectMapperKey _key;

        public ObjectMapperData(
            MapperContext mapperContext,
            MappingRuleSet ruleSet,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            ObjectMapperData parent)
            : base(mapperContext, ruleSet, sourceMember.Type, targetMember.Type, targetMember, parent)
        {
            var mdType = typeof(MappingData<,>).MakeGenericType(sourceMember.Type, targetMember.Type);
            var mdParameter = Parameters.Create(mdType);

            MdParameter = mdParameter;
            SourceObject = Expression.Property(mdParameter, "Source");
            TargetObject = Expression.Property(mdParameter, "Target");
            CreatedObject = Expression.Property(mdParameter, "CreatedObject");
            EnumerableIndex = Expression.Property(mdParameter, "EnumerableIndex");
            NestedAccessFinder = new NestedAccessFinder(mdParameter);

            SourceMember = sourceMember;
            _dataSourcesByTargetMemberName = new Dictionary<string, Tuple<QualifiedMember, DataSourceSet>>();

            var instanceVariableName = targetMember.Type.GetVariableName(f => f.InCamelCase);

            if (targetMember.IsEnumerable)
            {
                _sourceElementMember = sourceMember.Append(sourceMember.Type.CreateElementMember());
                _targetElementMember = targetMember.Append(targetMember.Type.CreateElementMember(targetMember.ElementType));
                EnumerablePopulationBuilder = new EnumerablePopulationBuilder(this);

                InstanceVariable = Expression.Variable(
                    typeof(IEnumerable<>).MakeGenericType(targetMember.ElementType),
                    instanceVariableName);
            }
            else
            {
                InstanceVariable = Expression.Variable(targetMember.Type, instanceVariableName);
            }
        }

        public IObjectMappingContextFactoryBridge CreateChildMapperDataBridge<TDeclaredSource, TDeclaredTarget>(
            MappingInstanceData<TDeclaredSource, TDeclaredTarget> data,
            string targetMemberName,
            int dataSourceIndex)
        {
            var targetMemberAndDataSourcesTuple = _dataSourcesByTargetMemberName[targetMemberName];
            var qualifiedTargetMember = targetMemberAndDataSourcesTuple.Item1;
            var sourceMember = targetMemberAndDataSourcesTuple.Item2[dataSourceIndex].SourceMember;

            return ObjectMapperDataFactoryBridge.Create(
                data,
                sourceMember,
                qualifiedTargetMember);
        }

        public IObjectMappingContextFactoryBridge CreateElementMapperDataBridge<TSourceElement, TTargetElement>(
            MappingInstanceData<TSourceElement, TTargetElement> data)
        {
            return ObjectMapperDataFactoryBridge.Create(
                data,
                _sourceElementMember,
                _targetElementMember);
        }

        #region IMemberMapperData Members

        public override ParameterExpression MdParameter { get; }

        public override ParameterExpression OmdParameter => Parameters.ObjectMapperData;

        public override IQualifiedMember SourceMember { get; }

        public override Expression SourceObject { get; }

        public override Expression TargetObject { get; }

        public override Expression EnumerableIndex { get; }

        public override ParameterExpression InstanceVariable { get; }

        public override NestedAccessFinder NestedAccessFinder { get; }

        public EnumerablePopulationBuilder EnumerablePopulationBuilder { get; }

        #endregion

        #region IObjectMapperData Members

        public Expression CreatedObject { get; }

        public void SetKey(ObjectMapperKey key) => _key = key;

        public MethodCallExpression GetMapCall(
            Expression sourceObject,
            QualifiedMember targetMember,
            int dataSourceIndex)
        {
            var mapCall = Expression.Call(
                OmdParameter,
                _mapObjectMethod.MakeGenericMethod(sourceObject.Type, targetMember.Type),
                sourceObject,
                targetMember.GetAccess(InstanceVariable),
                Expression.Constant(targetMember.Name),
                Expression.Constant(dataSourceIndex));

            return mapCall;
        }

        public MethodCallExpression GetMapCall(Expression sourceElement, Expression existingElement)
        {
            var mapCall = Expression.Call(
                OmdParameter,
                _mapEnumerableElementMethod.MakeGenericMethod(sourceElement.Type, existingElement.Type),
                sourceElement,
                existingElement,
                Parameters.EnumerableIndex);

            return mapCall;
        }

        public void RegisterTargetMemberDataSources(QualifiedMember targetMember, DataSourceSet dataSources)
        {
            // TODO: Apply runtime-typed source members to ObjectMapperKey
            _dataSourcesByTargetMemberName.Add(targetMember.Name, Tuple.Create(targetMember, dataSources));
        }

        #endregion
    }
}