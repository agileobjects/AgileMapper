Many aspects of mapping can be configured, but no up-front type registration is required - a mapping function is created and cached the first time two types are mapped.

Various configuration options are available:

- [Static](Static-vs-Instance-Mappers) configuration (`Mapper.WhenMapping`) configures a default, instance-scoped mapper behind the scenes. This configures the mapper used when you call `Mapper.Map(source)`

- [Instance](Static-vs-Instance-Mappers) configuration (`mapper.WhenMapping`) configures that particular instance

- [Inline](Inline-configuration) configuration combines the configuration of the mapper performing the mapping with the inline configuration you supply

- [Class](Configuration-Classes) configuration splits configuration up into dedicated configuration classes.

The same API is available in all four contexts.

