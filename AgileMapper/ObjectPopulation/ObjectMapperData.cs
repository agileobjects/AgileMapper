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
        private readonly MethodInfo _mapObjectMethod;
        private readonly MethodInfo _mapEnumerableElementMethod;
        private readonly IQualifiedMember _sourceElementMember;
        private readonly QualifiedMember _targetElementMember;
        private readonly Dictionary<string, Tuple<QualifiedMember, DataSourceSet>> _dataSourcesByTargetMemberName;

        public ObjectMapperData(
            MappingContext mappingContext,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            bool runtimeTypesAreTheSame,
            ObjectMapperKey key,
            ObjectMapperData parent)
            : base(
                  mappingContext.MapperContext,
                  mappingContext.RuleSet,
                  sourceMember.Type,
                  targetMember.Type,
                  targetMember,
                  parent)
        {
            var mdType = typeof(ObjectMappingData<,>).MakeGenericType(sourceMember.Type, targetMember.Type);
            var parameter = Parameters.Create(mdType, "data");

            Parameter = parameter;
            SourceMember = sourceMember;
            RuntimeTypesAreTheSame = runtimeTypesAreTheSame;
            SourceObject = Expression.Property(parameter, "Source");
            TargetObject = Expression.Property(parameter, "Target");
            CreatedObject = Expression.Property(parameter, "CreatedObject");
            EnumerableIndex = Expression.Property(parameter, "EnumerableIndex");
            NestedAccessFinder = new NestedAccessFinder(parameter);

            _mapObjectMethod = GetMapMethod(mdType, 4);
            _mapEnumerableElementMethod = GetMapMethod(mdType, 3);

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

            key.RemoveInstanceData();
            MapperKey = key;
        }

        #region Setup

        private MethodInfo GetMapMethod(Type mappingDataType, int numberOfArguments)
        {
            return mappingDataType
                .GetMethods(Constants.PublicInstance)
                .First(m => (m.Name == "Map") && (m.GetParameters().Length == numberOfArguments));
        }

        #endregion

        public IObjectMapperCreationData CreateChildMapperCreationData<TDeclaredSource, TDeclaredTarget>(
            MappingInstanceData<TDeclaredSource, TDeclaredTarget> instanceData,
            string targetMemberName,
            int dataSourceIndex)
        {
            var childBridge = CreateChildMappingDataBridge(instanceData, targetMemberName, dataSourceIndex);
            var childMapperData = childBridge.GetCreationData();

            return childMapperData;
        }

        private IObjectMapperDataBridge CreateChildMappingDataBridge<TDeclaredSource, TDeclaredTarget>(
            MappingInstanceData<TDeclaredSource, TDeclaredTarget> instanceData,
            string targetMemberName,
            int dataSourceIndex)
        {
            var targetMemberAndDataSourcesTuple = _dataSourcesByTargetMemberName[targetMemberName];
            var targetMember = targetMemberAndDataSourcesTuple.Item1;
            var sourceMember = targetMemberAndDataSourcesTuple.Item2[dataSourceIndex].SourceMember;

            return ObjectMapperDataBridge.Create(
                instanceData,
                sourceMember,
                targetMember,
                this);
        }

        public IObjectMapperCreationData CreateElementMapperCreationData<TSourceElement, TTargetElement>(
            MappingInstanceData<TSourceElement, TTargetElement> instanceData)
        {
            var elementBridge = ObjectMapperDataBridge.Create(
                instanceData,
                _sourceElementMember,
                _targetElementMember,
                this);

            var elementMapperData = elementBridge.GetCreationData();

            return elementMapperData;
        }

        public ObjectMapperKey MapperKey { get; }

        public override ParameterExpression Parameter { get; }

        public override IQualifiedMember SourceMember { get; }

        public override Expression SourceObject { get; }

        public override Expression TargetObject { get; }

        public override Expression EnumerableIndex { get; }

        public override ParameterExpression InstanceVariable { get; }

        public override NestedAccessFinder NestedAccessFinder { get; }

        public EnumerablePopulationBuilder EnumerablePopulationBuilder { get; }

        public Expression CreatedObject { get; }

        public bool RuntimeTypesAreTheSame { get; }

        public MethodCallExpression GetMapCall(
            Expression sourceObject,
            QualifiedMember targetMember,
            int dataSourceIndex)
        {
            var mapCall = Expression.Call(
                Parameter,
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
                Parameter,
                _mapEnumerableElementMethod.MakeGenericMethod(sourceElement.Type, existingElement.Type),
                sourceElement,
                existingElement,
                Parameters.EnumerableIndex);

            return mapCall;
        }

        public void RegisterTargetMemberDataSources(QualifiedMember targetMember, DataSourceSet dataSources)
        {
            _dataSourcesByTargetMemberName.Add(targetMember.Name, Tuple.Create(targetMember, dataSources));
        }
    }
}