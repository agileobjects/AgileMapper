AgileMapper matches flattened member names [as you'd expect](Member-Matching), but it also has a dedicated `.Flatten()` API which flattens objects in various ways. It is accessible:

- Via the [static API](Static-vs-Instance-Mappers), using `Mapper.Flatten(myObject)`
- Via the instance API, using `myInstanceMapper.Flatten(myObject)`
- Via an [extension method](Mapping-Extension-Methods), using `myObject.Flatten()`

Flattening produces an object including all the source object's string or value-type members. For example:

```C#
var customer = new Customer
{
    Name = "Mrs Customer",
    Dob = new DateTime(1985, 11, 05),
    Address = new Address
    {
        Line1 = "1 Street",
        Postcode = "XY3 8HW"
    }
};
```

...can be flattened to a Dictionary using:

```C#
var flat = Mapper.Flatten(myObject).ToDictionary();
// flat is a Dictionary<string, object> containing:
// ["Name"] = "Mrs Customer"
// ["Dob"] = * DateTime 1985/11/05 *
// ["Address.Line1"] = "1 Street"
// ["Address.Postcode"] = "XY3 8HW"
```

...or a dynamic using:

```C#
dynamic flat = myInstanceMapper.Flatten(myObject).ToDynamic();
// flat is an ExpandoObject with members:
// flat.Name = "Mrs Customer"
// flat.Dob = * DateTime 1985/11/05 *
// flat.Address_Line1 = "1 Street"
// flat.Address.Postcode = "XY3 8HW"
```

...or a query string-formatted string using:

```C#
var flat = myObject.Flatten().ToQueryString();
// flat is a query-string-formatted string with:
//  Name=Mrs%20Customer
// &Dob=11%2F5%2F1985%2012%3A00%3A00%20AM
// &Address%2ELine1=1%20Street
// &Address%2EPostcode=XY3%208HW
```

