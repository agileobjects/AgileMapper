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
using System.Collections.ObjectModel;
using AgileObjects.AgileMapper;
using AgileObjects.AgileMapper.ObjectPopulation;

namespace AgileObjects.AgileMapper.Buildable.UnitTests.Mappers
{
    [GeneratedCode("AgileObjects.AgileMapper.Buildable", "0.1.0.0")]
    public class StringCollectionMapper : MappingExecutionContextBase<Collection<string>>
    {
        public StringCollectionMapper
        (
            Collection<string> source
        )
        : base(source)
        {
        }

        public List<string> Over
        (
            List<string> target
        )
        {
            return StringCollectionMapper.Overwrite(this.CreateRootMappingData(target));
        }

        private static List<string> Overwrite
        (
            IObjectMappingData<Collection<string>, List<string>> scToSlData
        )
        {
            try
            {
                var sourceStringCollection = scToSlData.Source;
                var targetStringList = scToSlData.Target;
                targetStringList.Clear();
                targetStringList.AddRange(sourceStringCollection);

                return targetStringList;
            }
            catch (Exception ex)
            {
                throw MappingException.For(
                    "Overwrite",
                    "Collection<string>",
                    "List<string>",
                    ex);
            }
        }
    }
}