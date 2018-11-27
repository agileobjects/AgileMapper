AgileMapper matches nested members to source members with flattened names [as you'd expect](/Member-Matching), but it also has a dedicated `.Unflatten()` API which unflattens objects in various ways. It is accessible:

- Via the [static API](/Static-vs-Instance-Mappers), using `Mapper.Unflatten(myObject)`
- Via the instance API, using `myInstanceMapper.Unflatten(myObject)`
- Via an [extension method](/Mapping-Extension-Methods), using `myObject.Unflatten()`

Unflattening produces an object populated using the source's [flattened](/Object-Flattening) members.

For example, this Dictionary:

```cs
var dictionary = new Dictionary<string, object>
{
    ["Name"] = "Mrs Customer"
    ["Dob"] = * DateTime 1985/11/05 *
    ["Address.Line1"] = "1 Street"
    ["Address.Postcode"] = "XY3 8HW"
};
```

...and the `QueryString` object created from this string-formatted string:

```cs
var queryString = 
     "Name=Mrs%20Customer" +
    "&Dob=11%2F5%2F1985%2012%3A00%3A00%20AM" +
    "&Address%2ELine1=1%20Street" +
    "&Address%2EPostcode=XY3%208HW"
    .ToQueryString(); // <- Creates a QueryString object
```

...can both be unflattened to this `Customer` object:

```cs

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

...using `Mapper.Unflatten(dictionary).To<Customer>()`, or `queryString.Unflatten().To<Customer>()`, for example.