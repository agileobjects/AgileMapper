Mapper source code generation can be configured in the following ways.

## Change the Output Project

To output your mappers to a different project, add the following to your mapper configuration project:

```xml
<PropertyGroup>
  <MappersOutputProject>[PathToTargetCsProjFile]</MappersOutputProject>
</PropertyGroup>
```

Where `PathToTargetCsProjFile` is the fully-qualified or relative path to the `.csproj` file to which
mappers should be added.

