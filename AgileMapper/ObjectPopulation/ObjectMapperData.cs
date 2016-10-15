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
        private readonly Dictionary<string, DataSourceSet> _dataSourcesByTargetMemberName;

        public ObjectMapperData(
            IMappingContext mappingContext,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
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
            SourceObject = Expression.Property(parameter, "Source");
            TargetObject = Expression.Property(parameter, "Target");
            CreatedObject = Expression.Property(parameter, "CreatedObject");
            EnumerableIndex = Expression.Property(parameter, "EnumerableIndex");
            NestedAccessFinder = new NestedAccessFinder(parameter);

            _mapObjectMethod = GetMapMethod(mdType, 4);
            _mapEnumerableElementMethod = GetMapMethod(mdType, 3);

            _dataSourcesByTargetMemberName = new Dictionary<string, DataSourceSet>();

            var instanceVariableName = targetMember.Type.GetVariableName(f => f.InCamelCase);

            if (targetMember.IsEnumerable)
            {
                if (!targetMember.ElementType.IsSimple())
                {
                    SourceElementMember = sourceMember.Append(sourceMember.Type.CreateElementMember());
                    TargetElementMember = targetMember.Append(targetMember.Type.CreateElementMember(targetMember.ElementType));
                }

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

        #region Setup

        private static MethodInfo GetMapMethod(Type mappingDataType, int numberOfArguments)
        {
            return mappingDataType
                .GetPublicInstanceMethods()
                .First(m => (m.Name == "Map") && (m.GetParameters().Length == numberOfArguments));
        }

        #endregion

        public bool RequiresChildMapping => _dataSourcesByTargetMemberName.Count > 0;

        public bool RequiresElementMapping => TargetElementMember != null;

        public IQualifiedMember GetSourceMemberFor(string targetMemberName, int dataSourceIndex)
            => _dataSourcesByTargetMemberName[targetMemberName][dataSourceIndex].SourceMember;

        public QualifiedMember GetTargetMemberFor(string targetMemberName) => TargetMember.GetChildMember(targetMemberName);

        public override ParameterExpression Parameter { get; }

        public override IQualifiedMember SourceMember { get; }

        public IQualifiedMember SourceElementMember { get; }

        public QualifiedMember TargetElementMember { get; }

        public override Expression SourceObject { get; }

        public override Expression TargetObject { get; }

        public override Expression EnumerableIndex { get; }

        public override ParameterExpression InstanceVariable { get; }

        public override NestedAccessFinder NestedAccessFinder { get; }

        public EnumerablePopulationBuilder EnumerablePopulationBuilder { get; }

        public Expression CreatedObject { get; }

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
            => _dataSourcesByTargetMemberName.Add(targetMember.Name, dataSources);
    }
}