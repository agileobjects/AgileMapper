// ------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by AgileObjects.AgileMapper.Buildable.
// Runtime Version: 0.1.0.0
// 
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using AgileObjects.AgileMapper;
using AgileObjects.AgileMapper.ObjectPopulation;
using AgileObjects.AgileMapper.UnitTests.Common.TestClasses;

namespace AgileObjects.AgileMapper.Buildable.UnitTests.Mappers
{
    [GeneratedCode("AgileObjects.AgileMapper.Buildable", "0.1.0.0")]
    public class IntStringIntToTargetValueSourceMapper : MappingExecutionContextBase<ToTargetValueSource<int, string, int>>
    {
        public IntStringIntToTargetValueSourceMapper
        (
            ToTargetValueSource<int, string, int> source
        )
        : base(source)
        {
        }

        public PublicTwoFields<int, int> ToANew<TTarget>()
            where TTarget : PublicTwoFields<int, int>
        {
            return IntStringIntToTargetValueSourceMapper.CreateNew(this.CreateRootMappingData(default(PublicTwoFields<int, int>)));
        }

        public PublicTwoFields<int, int> Over
        (
            PublicTwoFields<int, int> target
        )
        {
            return IntStringIntToTargetValueSourceMapper.Overwrite(this.CreateRootMappingData(target));
        }

        public PublicTwoFields<int, int> OnTo
        (
            PublicTwoFields<int, int> target
        )
        {
            return IntStringIntToTargetValueSourceMapper.Merge(this.CreateRootMappingData(target));
        }

        private static PublicTwoFields<int, int> CreateNew
        (
            IObjectMappingData<ToTargetValueSource<int, string, int>, PublicTwoFields<int, int>> isittvsToIiptfData
        )
        {
            ToTargetValueSource<int, string, int> sourceIntStringIntToTargetValueSource;
            try
            {
                sourceIntStringIntToTargetValueSource = isittvsToIiptfData.Source;

                var intIntPublicTwoFields = new PublicTwoFields<int, int>();
                intIntPublicTwoFields.Value1 = sourceIntStringIntToTargetValueSource.Value1;
                // No data sources for Value2

                if (sourceIntStringIntToTargetValueSource.Value != null)
                {
                    if (sourceIntStringIntToTargetValueSource.Value != null)
                    {
                        intIntPublicTwoFields.Value1 = IntStringIntToTargetValueSourceMapper.GetInt1(sourceIntStringIntToTargetValueSource);
                    }

                    if (sourceIntStringIntToTargetValueSource.Value != null)
                    {
                        intIntPublicTwoFields.Value2 = sourceIntStringIntToTargetValueSource.Value.Value2;
                    }
                }

                return intIntPublicTwoFields;
            }
            catch (Exception ex)
            {
                throw MappingException.For(
                    "CreateNew",
                    "ToTargetValueSource<int, string, int>",
                    "PublicTwoFields<int, int>",
                    ex);
            }
        }

        private static PublicTwoFields<int, int> Overwrite
        (
            IObjectMappingData<ToTargetValueSource<int, string, int>, PublicTwoFields<int, int>> isittvsToIiptfData
        )
        {
            ToTargetValueSource<int, string, int> sourceIntStringIntToTargetValueSource;
            try
            {
                sourceIntStringIntToTargetValueSource = isittvsToIiptfData.Source;

                isittvsToIiptfData.Target.Value1 = sourceIntStringIntToTargetValueSource.Value1;
                // No data sources for Value2

                if (sourceIntStringIntToTargetValueSource.Value != null)
                {
                    if (sourceIntStringIntToTargetValueSource.Value != null)
                    {
                        isittvsToIiptfData.Target.Value1 = IntStringIntToTargetValueSourceMapper.GetInt2(sourceIntStringIntToTargetValueSource);
                    }
                    else
                    {
                        isittvsToIiptfData.Target.Value1 = default(int);
                    }

                    if (sourceIntStringIntToTargetValueSource.Value != null)
                    {
                        isittvsToIiptfData.Target.Value2 = sourceIntStringIntToTargetValueSource.Value.Value2;
                    }
                    else
                    {
                        isittvsToIiptfData.Target.Value2 = default(int);
                    }
                }

                return isittvsToIiptfData.Target;
            }
            catch (Exception ex)
            {
                throw MappingException.For(
                    "Overwrite",
                    "ToTargetValueSource<int, string, int>",
                    "PublicTwoFields<int, int>",
                    ex);
            }
        }

        private static PublicTwoFields<int, int> Merge
        (
            IObjectMappingData<ToTargetValueSource<int, string, int>, PublicTwoFields<int, int>> isittvsToIiptfData
        )
        {
            ToTargetValueSource<int, string, int> sourceIntStringIntToTargetValueSource;
            try
            {
                sourceIntStringIntToTargetValueSource = isittvsToIiptfData.Source;

                if (isittvsToIiptfData.Target.Value1 == default(int))
                {
                    isittvsToIiptfData.Target.Value1 = sourceIntStringIntToTargetValueSource.Value1;
                }

                // No data sources for Value2

                if (sourceIntStringIntToTargetValueSource.Value != null)
                {
                    if (isittvsToIiptfData.Target.Value1 == default(int))
                    {
                        if (sourceIntStringIntToTargetValueSource.Value != null)
                        {
                            isittvsToIiptfData.Target.Value1 = IntStringIntToTargetValueSourceMapper.GetInt3(sourceIntStringIntToTargetValueSource);
                        }
                    }

                    if (isittvsToIiptfData.Target.Value2 == default(int))
                    {
                        if (sourceIntStringIntToTargetValueSource.Value != null)
                        {
                            isittvsToIiptfData.Target.Value2 = sourceIntStringIntToTargetValueSource.Value.Value2;
                        }
                    }
                }

                return isittvsToIiptfData.Target;
            }
            catch (Exception ex)
            {
                throw MappingException.For(
                    "Merge",
                    "ToTargetValueSource<int, string, int>",
                    "PublicTwoFields<int, int>",
                    ex);
            }
        }

        private static int GetInt1
        (
            ToTargetValueSource<int, string, int> sourceIntStringIntToTargetValueSource
        )
        {
            int intValue;
            return int.TryParse(sourceIntStringIntToTargetValueSource.Value.Value1, out intValue)
                ? intValue
                : default(int);
        }

        private static int GetInt2
        (
            ToTargetValueSource<int, string, int> sourceIntStringIntToTargetValueSource
        )
        {
            int intValue;
            return int.TryParse(sourceIntStringIntToTargetValueSource.Value.Value1, out intValue)
                ? intValue
                : default(int);
        }

        private static int GetInt3
        (
            ToTargetValueSource<int, string, int> sourceIntStringIntToTargetValueSource
        )
        {
            int intValue;
            return int.TryParse(sourceIntStringIntToTargetValueSource.Value.Value1, out intValue)
                ? intValue
                : default(int);
        }
    }
}