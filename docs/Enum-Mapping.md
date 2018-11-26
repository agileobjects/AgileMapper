Enums can be mapped from:

 - Other enums, strings or objects (via a ToString() call), matched by enum member name case-insensitively
 - Any numeric value (int, long, decimal, etc), matched by enum member value

### Flags Enums

Flags enums are mapped automatically from numeric, string, enum and character source values. Strings can be a combination of comma-separated, numeric values and enum member names - the combination will be parsed and mapped correctly.

### Enum Mapping Mismatches

[Mapping plans](Using-Execution-Plans) warn you of enum mapping mismatches when mapping from one enum type to another. For example, a mapping between the following enums:

```C#
// Source enum:
enum PaymentTypeUs { Cash, Card, Check }
// Target enum:
enum PaymentTypeUk { Cash, Card, Cheque }
```

...has the following warning in its mapping plan:

```C#
// WARNING - enum mismatches mapping PaymentTypeUs to PaymentTypeUk:
//  - PaymentTypeUs.Check matches no PaymentTypeUk
```

`PaymentTypeUk.Cheque` also mismatches, but the mapping is going from `PaymentTypeUs` -> `PaymentTypeUk`, and it only matters that all *source* values can be mapped to target values. This mismatch would cause an exception if you use [mapping validation](Validating-Mappings).

## Configuring Enum Pairs

To have `PaymentTypeUs.Check` map to `PaymentTypeUk.Cheque`, use:

```C#
Mapper.WhenMapping
    .PairEnum(PaymentTypeUs.Check).With(PaymentTypeUk.Cheque);
```

...which removes the enum mapping mismatch warning from the mapping plan, and stops the validation exception.

Enum pairing can also be configured [inline](Inline-Configuration):

```C#
Mapper.Map(usTransaction).ToANew<UkTransaction>(cfg => cfg
    .PairEnum(PaymentTypeUs.Check).With(PaymentTypeUk.Cheque));
```