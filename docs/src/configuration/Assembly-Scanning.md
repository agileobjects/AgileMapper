By default, the assembly in which a base type is declared is searched for derived types. If there are additional assemblies which should be searched, use:

```cs
Mapper.WhenMapping.LookForDerivedTypesIn(
    typeof(DerivedType1).Assembly,
    typeof(DerivedType2).Assembly);
```