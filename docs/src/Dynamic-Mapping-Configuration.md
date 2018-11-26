[ExpandoObject](https://docs.microsoft.com/en-us/dotnet/api/system.dynamic.expandoobject?view=netframework-4.7.1) mapping can be configured in various ways.

### Configuring Member Names

If your source ExpandoObject member names don't match target member names, you can configure full names or name parts to use instead. For example, this source ExpandoObject:

```C#
dynamic source = new ExpandoObject();
source.ContactName = "Steve";
source.PhoneNums_0 = "01234 567890";
source.PhoneNums_1 = "07890 654321";
```

...can be configured to map to a `ContactDetails` object like so:

```C#
Mapper.WhenMapping
    .FromDynamics
    .To<ContactDetails>()
    .MapFullMemberName("ContactName") // Map the full name 'ContactName'
    .To(cd => cd.Name)                // ...to ContactDetails.Name
    .And
    .MapMemberName("PhoneNums") // Map the name part 'PhoneNums'
    .To(cd => cd.PhoneNumbers); // ...to ContactDetails.PhoneNumbers
```

### Configuring Separators

If the parts of nested member names in your source ExpandoObject aren't separated with underscores, you can configure what to use instead. For example, this source ExpandoObject:

```C#
IDictionary<string, object> source = new ExpandoObject();
source["Name"] = "Sandra";
source["Addresses_0-HouseNumber"] = "123";
source["Addresses_0-StreetName"] = "Dynamic Lane";
```

...uses '-' to separate member name parts, and can be configured to map to `ContactDetails` like so:

```C#
Mapper.WhenMapping
    .FromDynamics
    .To<ContactDetails>() // Optional
    .UseMemberNameSeparator("-");
```

The `To<ContactDetails>()` line in this examples is optional - excluding it sets dynamic mapping to all target types to use a '-' separator. Of course, using this separator actually creates illegal member names - you won't be able to access them with `contactDetails.Addresses_0-HouseNumber` - but because ExpandoObject implements `IDictionary<string, object>`, the mapping works anyway.

To map an object to an ExpandoObject without using separators, use:

```C#
Mapper.WhenMapping
    .From<ContactDetails>()
    .ToDynamics
    .UseFlattenedMemberNames();

// Will now map to keys "Addresses_0HouseNumber", "Addresses_0StreetName", etc.
```

### Configuring Element Indexes

If your member names don't have enumerable member element indexes formatted as '_i', you can configure the pattern to use instead. For example, this source ExpandoObject:

```C#
dynamic source = new ExpandoObject();
source.Name = "Steve";
source.PhoneNumbers_0_ = "01234 567890";
source.PhoneNumbers_1_ = "07890 654321";
```

...can be configured to map to `ContactDetails` like so:

```C#
Mapper.WhenMapping
    .FromDynamics
    .To<ContactDetails>()
    .UseElementKeyPattern("_i_");
```

An element key pattern must contain a single 'i' character as an enumerable index placeholder. Anything - or nothing - can be placed before or after the i and will be inserted into enumerable element value ExpandoObject member names. As before, this actually allows you to create illegal member names, but because ExpandoObject implements `IDictionary<string, object>`, the mapping works anyway.

## Other Configuration

All configurations which can be applied to non-ExpandoObject sources can be applied to ExpandoObjects. For example:

### Conditional Derived Types

An ExpandoObject can be configured to map to various derived types based on a configured condition. For example, this source ExpandoObject:

```C#
dynamic source = new ExpandoObject();
source._0_Name = "Fido";
source._0_Type = AnimalType.Dog; // enum value
source._0_Sound = "Woof";
source._0_ChasesSticks = true;
source._1_Name = "Kitty";
source._1_Type = AnimalType.Cat; // enum value
source._1_Sound = "Meow";
source._1_LovesCatnip = true;
```

Can be configured to map to an `Animal`, `Dog : Animal`, `Cat : Animal` hierarchy like so:

```C#
Mapper.WhenMapping
    .FromDynamics
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

The source ExpandoObject in this configuration - for example in `d["Type"]` - is accessed as an `IDictionary<string, object>`, as expression trees cannot contain dynamic member accesses.

### Conditional Member Population... etc

Custom conditions can be configured which must be satisfied for a member to be mapped. For example:

```C#
Mapper.WhenMapping
    .FromDynamics
    .To<ContactDetails>()
    .If((d, cd) => d["Name"] == "Helen")
    .Ignore(cd => cd.PhoneNumbers);
```

Dynamic mapping configuration also supports custom and conditional [object creation](Configuring-Object-Creation), [mapping callbacks](Configuring-Mapping-Callbacks), [target member values](Configuring-Member-Values), etc.