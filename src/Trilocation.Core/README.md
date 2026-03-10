# Trilocation.Core

Hierarchical geospatial indexing system based on octahedral triangular subdivision.

Trilocation divides the Earth's surface into triangular cells using an octahedral projection. The system supports 31 resolution levels (0-30), from 8 base triangles covering the entire globe down to sub-centimeter precision. Every cell at every resolution has a unique 64-bit unsigned integer index.

## Features

- **Hierarchical indexing** - 8 base faces, quaternary subdivision, 31 resolution levels
- **Coordinate conversion** - WGS84, Web Mercator (EPSG:3857), UTM, MGRS, ECEF
- **Hierarchy navigation** - Parent, children, ancestors, containment checks
- **Neighbor finding** - Edge-sharing neighbors, ring queries, distance-based search
- **Geometry** - Triangle vertices, centroids, area calculations, GeoJSON export
- **High performance** - Sub-microsecond encoding, optimized with AggressiveInlining

## Quick Start

```csharp
using Trilocation.Core;

// Create from coordinates (Helsinki, resolution 15)
var location = new TriLocation(60.1699, 24.9384, 15);

// Get the unique index
ulong index = location.Index;

// Convert back to coordinates (centroid)
var (lat, lon) = location.ToLatLon();

// Navigate hierarchy
TriLocation parent = location.GetParent();
TriLocation[] children = location.GetChildren();

// Find neighbors
TriLocation[] neighbors = location.GetNeighbors();
TriLocation[] nearby = location.GetNeighborsWithin(2);

// Check containment
bool contains = parent.Contains(location); // true

// Get geometry
TriCell cell = location.ToCell();
double areaKm2 = cell.AreaKm2;
```

## Related Packages

| Package | Description |
|---------|-------------|
| **Trilocation.Core** | Core library (this package) |
| Trilocation.Data | Entity Framework Core integration |
| Trilocation.Interop | H3, S2, Geohash, Plus Code interop |
| Trilocation.Serialization | JSON and binary serialization |

## License

AGPL-3.0-or-later
