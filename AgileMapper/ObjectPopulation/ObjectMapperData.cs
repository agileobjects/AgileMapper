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

    internal class ObjectMapperData : BasicMapperData, IMemberMapperData
    {
        private readonly ObjectMapperData _parent;
        private readonly List<ObjectMapperData> _childMapperDatas;
        private readonly MethodInfo _mapObjectMethod;
        private readonly MethodInfo _mapEnumerableElementMethod;
        private readonly Dictionary<string, DataSourceSet> _dataSourcesByTargetMemberName;

        public ObjectMapperData(
            IMappingContext mappingContext,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            ObjectMapperData parent)
            : base(
                  mappingContext.RuleSet,
                  sourceMember.Type,
                  targetMember.Type,
                  targetMember,
                  parent)
        {
            MapperContext = mappingContext.MapperContext;
            _parent = parent;
            _parent?._childMapperDatas.Add(this);
            _childMapperDatas = new List<ObjectMapperData>();

            var mdType = typeof(ObjectMappingData<,>).MakeGenericType(sourceMember.Type, targetMember.Type);
            Parameter = Parameters.Create(mdType, "data");
            SourceMember = sourceMember;
            SourceObject = Expression.Property(Parameter, "Source");
            TargetObject = Expression.Property(Parameter, "Target");
            CreatedObject = Expression.Property(Parameter, "CreatedObject");
            EnumerableIndex = Expression.Property(Parameter, "EnumerableIndex");
            NestedAccessFinder = new NestedAccessFinder(Parameter);

            _mapObjectMethod = GetMapMethod(mdType, 4);
            _mapEnumerableElementMethod = GetMapMethod(mdType, 3);

            _dataSourcesByTargetMemberName = new Dictionary<string, DataSourceSet>();

            if (targetMember.IsEnumerable)
            {
                if (!targetMember.ElementType.IsSimple())
                {
                    SourceElementMember = sourceMember.Append(sourceMember.Type.CreateElementMember());
                    TargetElementMember = targetMember.Append(targetMember.Type.CreateElementMember(targetMember.ElementType));
                }

                EnumerablePopulationBuilder = new EnumerablePopulationBuilder(this);
                InstanceVariable = EnumerablePopulationBuilder.TargetVariable;
            }
            else
            {
                TargetTypeHasNotYetBeenMapped = IsTargetTypeFirstMapping();
                TargetTypeWillNotBeMappedAgain = IsTargetTypeLastMapping();

                InstanceVariable = Expression
                    .Variable(TargetType, TargetType.GetVariableName(f => f.InCamelCase));
            }

            ReturnLabelTarget = Expression.Label(TargetType, "Return");
        }

        #region Setup

        private bool IsTargetTypeFirstMapping()
        {
            if (_parent == null)
            {
                return true;
            }

            var parent = _parent;

            while (parent != null)
            {
                if (parent.HasTypeBeenMapped(TargetType, this))
                {
                    return false;
                }

                parent = parent._parent;
            }

            return false;
        }

        private bool IsTargetTypeLastMapping() => !DoesTypeHaveACompatibleChildMember(TargetType, TargetType);

        private static bool DoesTypeHaveACompatibleChildMember(Type targetType, Type parentType)
        {
            var childTargetMembers = GlobalContext.Instance.MemberFinder.GetWriteableMembers(parentType);

            foreach (var childMember in childTargetMembers)
            {
                if (childMember.IsSimple)
                {
                    continue;
                }

                if (childMember.IsComplex)
                {
                    if (childMember.Type.IsAssignableFrom(targetType))
                    {
                        return true;
                    }

                    return DoesTypeHaveACompatibleChildMember(targetType, childMember.Type);
                }

                if (childMember.ElementType.IsComplex() && childMember.ElementType.IsAssignableFrom(targetType))
                {
                    return true;
                }

                return DoesTypeHaveACompatibleChildMember(targetType, childMember.ElementType);
            }

            return false;
        }

        private static MethodInfo GetMapMethod(Type mappingDataType, int numberOfArguments)
        {
            return mappingDataType
                .GetPublicInstanceMethods()
                .First(m => (m.Name == "Map") && (m.GetParameters().Length == numberOfArguments));
        }

        #endregion

        public MapperContext MapperContext { get; }

        ObjectMapperData IMemberMapperData.Parent => _parent;

        public bool RequiresChildMapping => _dataSourcesByTargetMemberName.Count > 0;

        public bool RequiresElementMapping => TargetElementMember != null;

        private bool HasTypeBeenMapped(Type targetType, IBasicMapperData requestingMapperData)
        {
            var mappedType = TargetMember.IsEnumerable ? TargetMember.ElementType : TargetType;

            if (mappedType.IsAssignableFrom(targetType))
            {
                return true;
            }

            foreach (var childMapperData in _childMapperDatas)
            {
                if (childMapperData == requestingMapperData)
                {
                    break;
                }

                if (childMapperData.HasTypeBeenMapped(targetType, this))
                {
                    return true;
                }
            }

            if ((_parent != null) && (requestingMapperData != _parent))
            {
                return _parent.HasTypeBeenMapped(targetType, this);
            }

            return false;
        }

        public IQualifiedMember GetSourceMemberFor(string targetMemberName, int dataSourceIndex)
            => _dataSourcesByTargetMemberName[targetMemberName][dataSourceIndex].SourceMember;

        public QualifiedMember GetTargetMemberFor(string targetMemberName) => TargetMember.GetChildMember(targetMemberName);

        public ParameterExpression Parameter { get; }

        public IQualifiedMember SourceMember { get; }

        public IQualifiedMember SourceElementMember { get; }

        public QualifiedMember TargetElementMember { get; }

        public bool TargetTypeHasNotYetBeenMapped { get; }

        public bool TargetTypeWillNotBeMappedAgain { get; }

        public Expression SourceObject { get; }

        public Expression TargetObject { get; }

        public Expression EnumerableIndex { get; }

        public ParameterExpression InstanceVariable { get; }

        public NestedAccessFinder NestedAccessFinder { get; }

        public EnumerablePopulationBuilder EnumerablePopulationBuilder { get; }

        public Expression CreatedObject { get; }

        public LabelTarget ReturnLabelTarget { get; }

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
                Expression.Constant(targetMember.GetRegistrationName()),
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

        public void RegisterTargetMemberDataSourcesIfRequired(QualifiedMember targetMember, DataSourceSet dataSources)
        {
            if (targetMember.IsSimple)
            {
                return;
            }

            _dataSourcesByTargetMemberName.Add(targetMember.GetRegistrationName(), dataSources);
        }
    }
}