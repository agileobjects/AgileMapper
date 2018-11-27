Update an object's members with values from another using:

```cs
Mapper.Map(customerViewModel).Over(customer);
```

When updating a collection, objects are matched by id ([configurable](/configuration/Object-Identifiers) if necessary). For example:

```cs
var source = new[]
{
    new CustomerViewModel { Id = 1,    Name = "Rod" },
    new CustomerViewModel { Id = 2,    Name = "Jane" },
    new CustomerViewModel { Id = null, Name = "Freddy" }
};
var target = Collection<Customer>
{
    new Customer { CustomerId = 2, Name = null },
    new Customer { CustomerId = 1, Name = "Bungle" },
    new Customer { CustomerId = 3, Name = "Zippy" }
};
var result = Mapper.Map(source).Over(target);
```

In this case:

* `source[0]` is matched to `target[1]`, so `target[1].Name` is updated to 'Rod'
* `source[1]` is matched to `target[0]`, so `target[0].Name` is set to 'Jane'
* `source[2]` has no match in the target collection, so a new `Customer` is added with `CustomerId` set to `null` and `Name` set to 'Freddy'
* `target[2]` (`CustomerId` = 3) has no match in the `source` collection, so it is removed
* The original `Collection<Customer>` instance is maintained and assigned to the `result` variable