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
using AgileObjects.AgileMapper.UnitTests.Common.TestClasses;

namespace AgileObjects.AgileMapper.Buildable.UnitTests.Mappers
{
    [GeneratedCode("AgileObjects.AgileMapper.Buildable", "0.1.0.0")]
    public static class Mapper
    {
        public static AddressAddressPublicTwoFieldsMapper Map
        (
            PublicTwoFields<Address, Address> source
        )
        {
            return new AddressAddressPublicTwoFieldsMapper(source);
        }

        public static PublicTwoFields<Address, Address> DeepClone
        (
            PublicTwoFields<Address, Address> source
        )
        {
            return new AddressAddressPublicTwoFieldsMapper(source).ToANew<PublicTwoFields<Address, Address>>();
        }

        public static AddressMapper Map
        (
            Address source
        )
        {
            return new AddressMapper(source);
        }

        public static CharArrayMapper Map
        (
            char[] source
        )
        {
            return new CharArrayMapper(source);
        }

        public static ChildMapper Map
        (
            Child source
        )
        {
            return new ChildMapper(source);
        }

        public static Child DeepClone
        (
            Child source
        )
        {
            return new ChildMapper(source).ToANew<Child>();
        }

        public static DateTimeHashSetMapper Map
        (
            HashSet<DateTime> source
        )
        {
            return new DateTimeHashSetMapper(source);
        }

        public static DecimalArrayMapper Map
        (
            decimal[] source
        )
        {
            return new DecimalArrayMapper(source);
        }

        public static IntAddressPublicTwoFieldsMapper Map
        (
            PublicTwoFields<int, Address> source
        )
        {
            return new IntAddressPublicTwoFieldsMapper(source);
        }

        public static IntArrayMapper Map
        (
            int[] source
        )
        {
            return new IntArrayMapper(source);
        }

        public static IntIEnumerableMapper Map
        (
            IEnumerable<int> source
        )
        {
            return new IntIEnumerableMapper(source);
        }

        public static IntStringIntToTargetValueSourceMapper Map
        (
            ToTargetValueSource<int, string, int> source
        )
        {
            return new IntStringIntToTargetValueSourceMapper(source);
        }

        public static ProductArrayMapper Map
        (
            Product[] source
        )
        {
            return new ProductArrayMapper(source);
        }

        public static ProductDtoArrayMapper Map
        (
            ProductDto[] source
        )
        {
            return new ProductDtoArrayMapper(source);
        }

        public static ProductDtoListMapper Map
        (
            List<ProductDto> source
        )
        {
            return new ProductDtoListMapper(source);
        }

        public static ProductMapper Map
        (
            Product source
        )
        {
            return new ProductMapper(source);
        }

        public static StringCollectionMapper Map
        (
            Collection<string> source
        )
        {
            return new StringCollectionMapper(source);
        }

        public static StringListMapper Map
        (
            List<string> source
        )
        {
            return new StringListMapper(source);
        }

        public static StringMapper Map
        (
            string source
        )
        {
            return new StringMapper(source);
        }

        public static StringPublicFieldMapper Map
        (
            PublicField<string> source
        )
        {
            return new StringPublicFieldMapper(source);
        }

        public static StringPublicPropertyMapper Map
        (
            PublicProperty<string> source
        )
        {
            return new StringPublicPropertyMapper(source);
        }

        public static StringStringDictionaryMapper Map
        (
            Dictionary<string, string> source
        )
        {
            return new StringStringDictionaryMapper(source);
        }
    }
}