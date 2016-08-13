namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Members;
    using ObjectPopulation;

    internal class MapCall
    {
        private static readonly MethodInfo _createChildMapperMethod = typeof(MapCall)
            .GetMethod("CreateChildMapper", Constants.NonPublicStatic);

        private MapCall(Expression inlineMapperCall, IObjectMapper inlineMapper)
            : this(inlineMapperCall)
        {
            InlineMapper = inlineMapper;
        }

        private MapCall(Expression runtimeMapCall)
        {
            Call = runtimeMapCall;
        }

        #region Factory Method

        public static MapCall For(
            IQualifiedMember bestMatchingSourceMember,
            int dataSourceIndex,
            IMemberMapperCreationData data)
        {
            var sourceMember = bestMatchingSourceMember ?? data.SourceMember;

            if (data.TargetMember.Type.IsSealed)
            {
                return GetInlineMapCall(sourceMember, dataSourceIndex, data);
            }

            var relativeMember = sourceMember.RelativeTo(data.SourceMember);
            var relativeMemberAccess = relativeMember.GetQualifiedAccess(data.MapperData.SourceObject);

            var runtimeMapCall = data.MapperData.GetMapCall(relativeMemberAccess, dataSourceIndex);

            return new MapCall(runtimeMapCall);
        }

        private static MapCall GetInlineMapCall(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            IMemberMapperCreationData data)
        {
            var key = new CreateChildMapperCallKey(sourceMember, data.TargetMember);

            data.MapperData.Parent.Register(data.TargetMember, sourceMember, dataSourceIndex);

            var childMapperFactory = GlobalContext.Instance.Cache.GetOrAdd(key, k =>
            {
                var createChildMapperMethod = _createChildMapperMethod
                    .MakeGenericMethod(sourceMember.Type, data.TargetMember.Type);

                var sourceMemberParameter = Parameters.Create<IQualifiedMember>("sourceMember");

                var createChildInstanceDataMethod = typeof(IMemberMapperCreationData)
                    .GetMethod("CreateChildMappingInstanceData", Constants.PublicInstance)
                    .MakeGenericMethod(sourceMember.Type, data.TargetMember.Type);

                var createChildInstanceDataCall = Expression.Call(
                    Parameters.MemberMapperCreationData,
                    createChildInstanceDataMethod,
                    sourceMemberParameter);

                var mapperData = Expression.Property(
                    Parameters.MemberMapperCreationData,
                    "MapperData");

                var createChildMapperCall = Expression.Call(
                    createChildMapperMethod,
                    createChildInstanceDataCall,
                    Parameters.DataSourceIndex,
                    mapperData);

                var createChildMapperLambda = Expression
                    .Lambda<Func<IQualifiedMember, int, IMemberMapperCreationData, IObjectMapper>>(
                        createChildMapperCall,
                        sourceMemberParameter,
                        Parameters.DataSourceIndex,
                        Parameters.MemberMapperCreationData);

                return createChildMapperLambda.Compile();
            });

            var childMapper = childMapperFactory.Invoke(sourceMember, dataSourceIndex, data);

            var createChildMappingDataCall = data.MapperData
                .GetCreateChildMappingDataCall(sourceMember, dataSourceIndex);

            var childMapperInvoke = Expression.Invoke(
                childMapper.MapperVariable,
                createChildMappingDataCall);

            return new MapCall(childMapperInvoke, childMapper);
        }

        // ReSharper disable once UnusedMember.Local
        private static IObjectMapper CreateChildMapper<TSource, TTarget>(
            MappingInstanceData<TSource, TTarget> instanceData,
            int dataSourceIndex,
            MemberMapperData mapperData)
        {
            var childMapperCreationData = mapperData.Parent.CreateChildMapperCreationData(
                instanceData,
                mapperData.TargetMember.Name,
                dataSourceIndex);

            var childMapper = mapperData
                .MapperContext
                .ObjectMapperFactory
                .CreateFor<TSource, TTarget>(childMapperCreationData);

            return childMapper;
        }

        private class CreateChildMapperCallKey
        {
            private readonly int _hashCode;

            public CreateChildMapperCallKey(IQualifiedMember sourceMember, IQualifiedMember targetMember)
            {
                _hashCode = (sourceMember.Signature + ">" + targetMember.Signature).GetHashCode();
            }

            public override int GetHashCode() => _hashCode;
        }

        #endregion

        public Expression Call { get; }

        public IObjectMapper InlineMapper { get; }
    }
}