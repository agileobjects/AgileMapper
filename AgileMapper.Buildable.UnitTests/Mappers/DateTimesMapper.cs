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
    public class DateTimesMapper : MappingExecutionContextBase<HashSet<DateTime>>
    {
        public DateTimesMapper
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
                return (TTarget)((object)DateTimesMapper.CreateNew(this.CreateRootMappingData(default(DateTime[]))));
            }

            throw new NotSupportedException(
                "Unable to perform a 'CreateNew' mapping from source type 'HashSet<DateTime>' to target type '" + typeof(TTarget).GetFriendlyName(null) + "'");
        }

        private static DateTime[] CreateNew
        (
            IObjectMappingData<HashSet<DateTime>, DateTime[]> dtsToDtaData
        )
        {
            try
            {
                var sourceDateTimes = dtsToDtaData.Source;
                var targetDateTimes = new List<DateTime>(sourceDateTimes.Count);
                var i = 0;
                var enumerator = sourceDateTimes.GetEnumerator();
                try
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }

                        targetDateTimes.Add(enumerator.Current);
                        ++i;
                    }
                }
                finally
                {
                    enumerator.Dispose();
                }

                return targetDateTimes.ToArray();
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