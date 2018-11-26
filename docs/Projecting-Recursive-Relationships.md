If your [`IQueryProvider`](https://docs.microsoft.com/en-us/dotnet/api/system.linq.iqueryprovider) [supports](Entity-Framework#recursion) it, you can project recursive relationships to a specified depth. For example, with these classes:

```C#
// Entity:
public class Category
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; }
}

// DTO:
public class CategoryDto
{
    public string Name { get; set; }
    public CategoryDto ParentCategory { get; set; }
    public ICollection<CategoryDto> SubCategories { get; set; }
}
```

...a `Category` has a many-to-one relationship with its `ParentCategory`, and a one-to-many relationship with its `SubCategories`. This forms a tree of objects, for example:

```
-- Musical Instruments
  -- Strings
    -- Guitars
      -- Electric
        -- Stratocaster
        -- Bass
      -- Acoustic
        -- Ukulele
        -- Mandolin
    -- Violins
  -- Wind
    -- Horns
  -- Keys
    -- Pianos
      -- Grand
      -- Upright
    -- Keyboards
  -- Percussive
    -- Drums
      -- Acoustic
      -- Electronic
```

To project a particular node of the tree to DTOs, the mapper has to know up-front how many levels deep you want to go in the tree. For example, without a specified recursion depth:

```C#
// Using an EF Core DbContext:
var stringsDto = await context
   .Categories
   .Project().To<CategoryDto>()
   .FirstOrDefaultAsync(c => c.Name == "Strings");
// StringDto will have
// -- Strings
//     -- Guitars
//     -- Violins
```

...or with a recursion depth of 1:

```C#
var stringsDto = await context
   .Categories
   .Project().To<CategoryDto>(cfg => cfg.RecurseToDepth(1))
   .FirstOrDefaultAsync(c => c.Name == "Strings");
// StringDto will have
// -- Strings
//     -- Guitars
//       -- Electric
//       -- Acoustic
//     -- Violins
```

...or with a recursion depth of 2:

```C#
var stringsDto = await context
   .Categories
   .Project().To<CategoryDto>(cfg => cfg.RecurseToDepth(2))
   .FirstOrDefaultAsync(c => c.Name == "Strings");
// StringDto will have
// -- Strings
//     -- Guitars
//       -- Electric
//         -- Stratocaster
//         -- Bass
//       -- Acoustic
//         -- Ukulele
//         -- Mandolin
//     -- Violins
```

...and so on.

Note that it is not possible to use [object tracking](Mapped-Object-Tracking) in query projections, so [identity integrity](Mapped-Object-Tracking#identity-integrity) is not maintained.