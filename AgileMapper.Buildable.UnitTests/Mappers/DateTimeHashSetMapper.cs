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
using AgileObjects.ReadableExpressions;
using AgileObjects.ReadableExpressions.Extensions;

namespace AgileObjects.AgileMapper.Buildable.UnitTests.Mappers
{
    [GeneratedCode("AgileObjects.AgileMapper.Buildable", "0.1.0.0")]
    public class DateTimeHashSetMapper : MappingExecutionContextBase<HashSet<DateTime>>
    {
        public DateTimeHashSetMapper
        (
            HashSet<DateTime> source
        )
        : base(source)
        {
        }

        public TTarget ToANew<TTarget>()
        {
            if (typeof(TTarget) == typeof(DateTime[]))
            {
                return (TTarget)((object)DateTimeHashSetMapper.CreateNew(this.CreateRootMappingData(default(DateTime[]))));
            }

            throw new NotSupportedException(
                "Unable to perform a 'CreateNew' mapping from source type 'HashSet<DateTime>' to target type '" + typeof(TTarget).GetFriendlyName(null) + "'");
        }

        private static DateTime[] CreateNew
        (
            IObjectMappingData<HashSet<DateTime>, DateTime[]> dthsToDtaData
        )
        {
            try
            {
                var sourceDateTimeHashSet = dthsToDtaData.Source;
                var targetDateTimeList = new List<DateTime>(sourceDateTimeHashSet.Count);
                var i = 0;
                var enumerator = sourceDateTimeHashSet.GetEnumerator();
                try
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }

                        targetDateTimeList.Add(enumerator.Current);
                        ++i;
                    }
                }
                finally
                {
                    enumerator.Dispose();
                }

                return targetDateTimeList.ToArray();
            }
            catch (Exception ex)
            {
                throw MappingException.For(
                    "CreateNew",
                    "HashSet<DateTime>",
                    "DateTime[]",
                    ex);
            }
        }
    }
}