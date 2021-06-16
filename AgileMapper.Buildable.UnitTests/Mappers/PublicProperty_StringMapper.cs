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
    public class PublicProperty_StringMapper : MappingExecutionContextBase<PublicProperty<string>>
    {
        public PublicProperty_StringMapper
        (
            PublicProperty<string> source
        )
        : base(source)
        {
        }

        public PublicField<int> ToANew<TTarget>()
            where TTarget : PublicField<int>
        {
            return PublicProperty_StringMapper.CreateNew(this.CreateRootMappingData(default(PublicField<int>)));
        }

        private static PublicField<int> CreateNew
        (
            IObjectMappingData<PublicProperty<string>, PublicField<int>> ppsToPfiData
        )
        {
            try
            {
                var publicField_Int = new PublicField<int>();
                publicField_Int.Value = PublicProperty_StringMapper.GetInt(ppsToPfiData);

                return publicField_Int;
            }
            catch (Exception ex)
            {
                throw MappingException.For(
                    "CreateNew",
                    "PublicProperty<string>",
                    "PublicField<int>",
                    ex);
            }
        }

        private static int GetInt
        (
            IObjectMappingData<PublicProperty<string>, PublicField<int>> ppsToPfiData
        )
        {
            int intValue;
            return int.TryParse(ppsToPfiData.Source.Value, out intValue) ? intValue : default(int);
        }
    }
}