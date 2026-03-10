# Trilocation.Data

Entity Framework Core integration for the Trilocation geospatial indexing system.

## Features

- **IHasTriIndex** - Interface for entities with a TriLocation index
- **ValueConverter** - Automatic `ulong` to `BIGINT` conversion for database storage
- **Spatial queries** - LINQ extensions for area queries, hierarchy filtering, and proximity search

## Quick Start

```csharp
using Trilocation.Data;

// Implement IHasTriIndex on your entity
public class PointOfInterest : IHasTriIndex
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ulong TriIndex { get; set; }
}

// Configure in DbContext
modelBuilder.Entity<PointOfInterest>().ConfigureTriIndex();

// Spatial queries
var nearby = db.Pois.WithinArea(ancestorLocation).ToList();
var children = db.Pois.ChildrenOf(parentLocation).ToList();
var atLevel = db.Pois.AtResolution(10).ToList();
```

## Dependencies

- Trilocation.Core
- Microsoft.EntityFrameworkCore 8.0+
- Microsoft.EntityFrameworkCore.Relational 8.0+

## License

AGPL-3.0-or-later
