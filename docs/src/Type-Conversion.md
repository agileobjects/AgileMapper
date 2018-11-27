Value conversion is performed according to the following:

- Value types, nullable types and strings are all parsed and converted out of the box using the `TryParse` methods from the BCL.

- `DateTime`s (and nullable `DateTime`s) are converted to strings using `value.ToString(CultureInfo.CurrentCulture.DateTimeFormat)`. Custom formatting strings [can be configured](/configuration/To-String-Formatting) for to-string conversions.

- Implicit or explicit to-String operators are used to convert values to strings where they are available.

- When parsing numerics, the default value is applied in the following circumstances:
  - If a value larger or smaller than the target type can contain is parsed - e.g. `double.MaxValue` being mapped to an int

  - If a mapping would cause a loss of precision - e.g. 123.456 being mapped to a long

- Enum values are parsed using a nested ternary operation with one branch per potential source enum value, e.g.

```cs
target.EnumValue = source.Enum == SourceStatus.Complete
    ? TargetStatus.Complete
    : source.Enum == SourceStatus.InProgress
        ? TargetStatus.InProgress
        : source.Enum == SourceStatus.New
            ? TargetStatus.New
            : default(TargetStatus)
```

&nbsp; &nbsp; &nbsp; &nbsp; ...any configured [enum pairs](/Enum-Mapping#configuring-enum-pairs) are used as the first values in the tree.

- Numerics are mapped to booleans (and vice-versa) with 1 mapping to true, and not-1 mapping to false.