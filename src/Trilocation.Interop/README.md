# Trilocation.Interop

Interoperability between Trilocation and other geospatial indexing systems.

## Supported Systems

| System | Convert To | Convert From |
|--------|-----------|-------------|
| H3 (Uber) | TriLocation -> H3 | H3 -> TriLocation |
| S2 (Google) | TriLocation -> S2CellId | S2CellId -> TriLocation |
| Geohash | TriLocation -> Geohash | Geohash -> TriLocation |
| Plus Code | TriLocation -> Plus Code | Plus Code -> TriLocation |
| Quadkey (Bing) | TriLocation -> Quadkey | Quadkey -> TriLocation |
| Maidenhead | TriLocation -> Grid | Grid -> TriLocation |

## Quick Start

```csharp
using Trilocation.Interop;

// Convert between systems via TriInterop facade
ulong triIndex = TriInterop.FromGeohash("u2fhk0", 15);
string geohash = TriInterop.ToGeohash(triIndex, 6);

ulong triIndex = TriInterop.FromH3(h3Index, 15);
long h3 = TriInterop.ToH3(triIndex, 9);
```

## Dependencies

- Trilocation.Core
- H3Lib 3.7.2
- S2Geometry 1.0.3

## License

AGPL-3.0-or-later
