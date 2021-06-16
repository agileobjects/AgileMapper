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
using System.Collections.Generic;
using AgileObjects.AgileMapper;
using AgileObjects.AgileMapper.Extensions;
using AgileObjects.AgileMapper.ObjectPopulation;

namespace AgileObjects.AgileMapper.Buildable.UnitTests.Mappers
{
    [GeneratedCode("AgileObjects.AgileMapper.Buildable", "0.1.0.0")]
    public class IntIEnumerableMapper : MappingExecutionContextBase<IEnumerable<int>>
    {
        public IntIEnumerableMapper
        (
            IEnumerable<int> source
        )
        : base(source)
        {
        }

        public ICollection<int> OnTo
        (
            ICollection<int> target
        )
        {
            return IntIEnumerableMapper.Merge(this.CreateRootMappingData(target));
        }

        private static ICollection<int> Merge
        (
            IObjectMappingData<IEnumerable<int>, ICollection<int>> isToIsData
        )
        {
            try
            {
                var sourceInts = isToIsData.Source.Exclude(isToIsData.Target);
                ICollection<int> targetInts = isToIsData.Target.IsReadOnly ? new List<int>(isToIsData.Target) : isToIsData.Target;
                var i = 0;
                var enumerator = sourceInts.GetEnumerator();
                try
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }

                        targetInts.Add(enumerator.Current);
                        ++i;
                    }
                }
                finally
                {
                    enumerator.Dispose();
                }

                return targetInts;
            }
            catch (Exception ex)
            {
                throw MappingException.For(
                    "Merge",
                    "IEnumerable<int>",
                    "ICollection<int>",
                    ex);
            }
        }
    }
}