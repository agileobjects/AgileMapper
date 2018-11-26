Dictionary mapping can be configured in various ways.

### Configuring Keys

If your source dictionary keys don't match target member names, you can configure full keys or member name key parts to use instead. For example, this source dictionary:

```C#
var source = new Dictionary<string, string>
{
    ["ContactName"] = "Steve",
    ["PhoneNums[0]"] = "01234 567890",
    ["PhoneNums[1]"] = "07890 654321"
};
```

...can be configured to map to a `ContactDetails` object like so:

```C#
Mapper.WhenMapping
    .FromDictionaries
    .To<ContactDetails>()
    .MapKey("ContactName")      // Map the full key 'ContactName'
    .To(cd => cd.Name)          // ...to ContactDetails.Name
    .And
    .MapMemberName("PhoneNums") // Map the key part 'PhoneNums'
    .To(cd => cd.PhoneNumbers); // ...to ContactDetails.PhoneNumbers
```

### Configuring Separators

If the parts of nested member names in your source dictionary keys aren't separated with dots, you can configure what to use instead. For example, this source dictionary:

```C#
var source = new Dictionary<string, string>
{
    ["Name"] = "Steve",
    ["Addresses[0]-HouseNumber"] = "123",
    ["Addresses[0]-StreetName"] = "Dictionary Street"
};
```

...uses '-' to separate member name parts, and can be configured to map to `ContactDetails` like so:

```C#
Mapper.WhenMapping
    .FromDictionaries
    .To<ContactDetails>() // Optional
    .UseMemberNameSeparator("-");
```

The `To<ContactDetails>()` line in this examples is optional - excluding it sets dictionary mapping to all target types to use a '-' separator.

To map an object to a dictionary without using separators, use:

```C#
Mapper.WhenMapping
    .From<ContactDetails>()
    .ToDictionaries
    .UseFlattenedMemberNames();

// Will now map to keys "Addresses[0]HouseNumber", "Addresses[0]StreetName", etc.
```

### Configuring Element Indexes

If your keys don't have enumerable member element indexes formatted as '[i]', you can configure the pattern to use instead. For example, this source dictionary:

```C#
var source = new Dictionary<string, string>
{
    ["Name"] = "Steve",
    ["PhoneNumbers0"] = "01234 567890",
    ["PhoneNumbers1"] = "07890 654321"
};
```

...can be configured to map to `ContactDetails` like so:

```C#
Mapper.WhenMapping
    .FromDictionaries
    .To<ContactDetails>()
    .UseElementKeyPattern("i");
```

An element key pattern must contain a single 'i' character as an enumerable index placeholder. Anything - or as in this example, nothing - can be placed before or after the i and will be inserted into enumerable element value dictionary keys.

## Other Configuration

All configurations which can be applied to non-dictionary sources can be applied to dictionaries. For example:

### Conditional Derived Types

A dictionary can be configured to map to various derived types based on a configured condition. For example, this source dictionary:

```C#
var source = new Dictionary<string, object>
{
    ["[0].Name"] = "Fido",
    ["[0].Type"] = AnimalType.Dog, // enum value
    ["[0].Sound"] = "Woof",
    ["[0].ChasesSticks"] = true,
    ["[1].Name"] = "Kitty",
    ["[1].Type"] = AnimalType.Cat, // enum value
    ["[1].Sound"] = "Meow",
    ["[1].LovesCatnip"] = true
};
```

Can be configured to map to an `Animal`, `Dog : Animal`, `Cat : Animal` hierarchy like so:

```C#
Mapper.WhenMapping
    .FromDictionaries
    .To<Animal>()
    .If((d, a) => d["Type"] == AnimalType.Dog)
    .MapTo<Dog>()
    .And
    .If((d, a) => d["Type"] == AnimalType.Cat)
    .MapTo<Cat>();

var animals = Mapper.Map(source).ToANew<List<Animal>>();
// animals[0] is of type Dog
// animals[1] is of type Cat
```

### Conditional Member Population... etc

Custom conditions can be configured which must be satisfied for a member to be mapped. For example:

```C#
Mapper.WhenMapping
    .FromDictionaries
    .To<ContactDetails>()
    .If((d, cd) => d["Name"] == "Steve")
    .Ignore(cd => cd.PhoneNumbers);
```

Dictionary mapping configuration also supports custom and conditional [object creation](Configuring-Object-Creation), [mapping callbacks](Configuring-Mapping-Callbacks), [target member values](Configuring-Member-Values), etc.