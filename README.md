# Trilocation

**Hierarchical geospatial indexing system based on octahedral triangular subdivision.**

Trilocation divides the Earth's surface into triangular cells using an octahedral projection. The system supports 31 resolution levels (0-30), from 8 base triangles covering the entire globe down to sub-centimeter precision. Every cell at every resolution has a unique 64-bit unsigned integer index.

## Key Properties

- **Hierarchical** - Each triangle subdivides into exactly 4 children
- **Global coverage** - 8 base faces cover the entire sphere without gaps or overlaps
- **Unique indexing** - Every cell has a unique `uint64` index encoding position and resolution
- **Deterministic** - Same coordinates always map to the same index at a given resolution
- **Fast** - Sub-microsecond encoding at low resolutions, ~4 us at maximum resolution

## Resolution Table

| Level | Triangles | Approx. Area | Use Case |
|-------|-----------|-------------|----------|
| 0 | 8 | 63,758,059 km2 | Hemisphere |
| 3 | 512 | 787,460 km2 | Country |
| 6 | 32,768 | 12,227 km2 | Region |
| 9 | 2,097,152 | 191 km2 | Metro area |
| 12 | 134,217,728 | 2.98 km2 | Neighborhood |
| 15 | 8,589,934,592 | 46,636 m2 | City block |
| 18 | 549,755,813,888 | 729 m2 | Building |
| 21 | 35,184,372,088,832 | 11 m2 | Room |
| 24 | 2,251,799,813,685,248 | < 1 m2 | Sub-meter |
| 30 | 9,223,372,036,854,775,808 | < 1 cm2 | Maximum |

## Quick Start (C#)

### Installation

```
dotnet add package Trilocation
```

### Basic Usage

```csharp
using Trilocation.Core;

// Create from coordinates (Helsinki, resolution 15)
var location = new TriLocation(60.1699, 24.9384, 15);

// Get the index
ulong index = location.Index;       // unique 64-bit identifier
int resolution = location.Resolution; // 15
int baseFace = location.BaseFace;    // 0-7

// Convert back to coordinates (centroid)
var (lat, lon) = location.ToLatLon();

// Navigate the hierarchy
TriLocation parent = location.GetParent();
TriLocation[] children = location.GetChildren();
TriLocation ancestor = location.GetAncestor(5);

// Check containment
bool contains = parent.Contains(location); // true

// Find neighbors
TriLocation[] neighbors = location.GetNeighbors();       // 3 edge-sharing
TriLocation[] nearby = location.GetNeighborsWithin(2);    // all within distance 2

// Get geometry
TriCell cell = location.ToCell();
double areaKm2 = cell.AreaKm2;
bool inside = cell.Contains(60.17, 24.94);
```

### Static API

```csharp
// TriIndex provides a static facade for all operations
var loc = TriIndex.FromLatLon(60.1699, 24.9384, 15);
var loc2 = TriIndex.FromIndex(12345678UL);
TriLocation[] area = TriIndex.FromBounds(bounds, 10);

// Grid distance between two locations
int distance = TriIndex.GetGridDistance(loc, loc2);
```

### Coordinate Conversions

```csharp
using Trilocation.Core.Conversions;

// Web Mercator (EPSG:3857)
var loc = TriConvert.FromWebMercator(2776460.0, 8438350.0, 15);
var (x, y) = TriConvert.ToWebMercator(loc);

// UTM
var loc = TriConvert.FromUtm(35, 'V', 385000, 6672000, 15);
var (zone, band, easting, northing) = TriConvert.ToUtm(loc);

// MGRS
var loc = TriConvert.FromMgrs("35VLJ8500072000", 15);
string mgrs = TriConvert.ToMgrs(loc, 5);

// ECEF (Earth-Centered, Earth-Fixed)
var loc = TriConvert.FromEcef(2886590.0, 1338700.0, 5509560.0, 15);
var (ex, ey, ez) = TriConvert.ToEcef(loc);
```

### Interoperability

```csharp
using Trilocation.Interop;

// Convert between Trilocation and other geospatial systems
ulong triIndex = TriInterop.FromH3(h3Index, resolution);
long h3 = TriInterop.ToH3(triIndex, h3Resolution);

ulong triIndex = TriInterop.FromS2(s2CellId, resolution);
ulong s2 = TriInterop.ToS2(triIndex, s2Level);

ulong triIndex = TriInterop.FromGeohash("u2fhk0", resolution);
string geohash = TriInterop.ToGeohash(triIndex, precision);

// Also supports: Plus Codes, Quadkeys, Maidenhead locators
```

### Database Integration (Entity Framework Core)

```csharp
using Trilocation.Data;

public class PointOfInterest : IHasTriIndex
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ulong TriIndex { get; set; }
}

// In DbContext.OnModelCreating:
modelBuilder.Entity<PointOfInterest>().ConfigureTriIndex();

// Spatial queries
var nearby = db.Pois
    .WithinArea(centerIndex, maxDistance)
    .ToList();

var children = db.Pois
    .ChildrenOf(parentIndex)
    .ToList();
```

### Serialization

```csharp
using Trilocation.Serialization;

// JSON (System.Text.Json)
var options = new JsonSerializerOptions();
options.Converters.Add(new TriLocationJsonConverter());
string json = JsonSerializer.Serialize(location, options);

// Binary (8 bytes, little-endian)
byte[] bytes = TriLocationBinarySerializer.Serialize(location);
TriLocation restored = TriLocationBinarySerializer.Deserialize(bytes);
```

## NuGet Packages

| Package | Description |
|---------|-------------|
| `Trilocation` | Core library: indexing, geometry, hierarchy, neighbors |
| `Trilocation.Data` | Entity Framework Core integration |
| `Trilocation.Interop` | H3, S2, Geohash, Plus Code, Quadkey, Maidenhead conversions |
| `Trilocation.Serialization` | JSON and binary serialization |

## Performance

Benchmarks on .NET 8 (BenchmarkDotNet):

| Operation | Resolution | Time | Allocation |
|-----------|-----------|------|------------|
| Encode (lat/lon to index) | 5 | ~700 ns | 2 KB |
| Encode | 15 | ~2.1 us | 6 KB |
| Encode | 24 | ~3.4 us | 10 KB |
| Encode | 30 | ~4.0 us | 12 KB |
| Decode (index to lat/lon) | 5 | ~660 ns | 2 KB |
| Round-trip | 15 | ~4.3 us | 12 KB |
| GetNeighbors | 10 | ~5.5 us | 16 KB |
| GetNeighborsWithin(2) | 10 | ~23 us | 67 KB |

## Multi-Language Implementations

Trilocation has implementations in 20 languages, each in its own repository. All implementations pass the same specification compliance tests from `spec/test-vectors.json`.

| Language | Repository | Package Manager | Status |
|----------|-----------|----------------|--------|
| **C#** | This repo (`src/Trilocation.Core/`) | NuGet | 625 tests GREEN |
| **TypeScript** | `trilocation-ts` | npm | 8/8 GREEN |
| **Python** | `trilocation-py` | PyPI | 8/8 GREEN |
| **Java** | `trilocation-java` | Maven | 8/8 GREEN |
| **Kotlin** | `trilocation-kotlin` | Maven | 8/8 GREEN |
| **Rust** | `trilocation-rust` | crates.io | 8/8 GREEN |
| **Dart** | `trilocation-dart` | pub.dev | 8/8 GREEN |
| **C++** | `trilocation-cpp` | Header-only (CMake) | 8/8 GREEN |
| **C** | `trilocation-c` | Header-only (C99) | 1303/1303 GREEN |
| **Swift** | `trilocation-swift` | SPM | Written |
| **Go** | `trilocation-go` | Go modules | Written |
| **Objective-C** | `trilocation-objc` | CocoaPods | Written |
| **WASM** | `trilocation-wasm` | npm | Built from Rust |
| **Ruby** | `trilocation-ruby` | RubyGems | Written |
| **PHP** | `trilocation-php` | Packagist | Written |
| **R** | `trilocation-r` | CRAN | Written |
| **Scala** | `trilocation-scala` | Maven | Written |
| **Elixir** | `trilocation-elixir` | Hex | Written |
| **Lua** | `trilocation-lua` | LuaRocks | Written |
| **Julia** | `trilocation-julia` | Julia Pkg | Written |

## How It Works

### Octahedral Projection

The Earth is projected onto a regular octahedron inscribed in the unit sphere. This produces 8 base triangular faces (4 northern, 4 southern hemisphere), each covering approximately 63.8 million km2.

```
        North Pole
        /  |  \  \
       / 0 | 1 \  \
      /    |    \ 2 \  3
Eq0 ---Eq90---Eq180---Eq270---Eq0
      \ 4  | 5  / 6 /  7
       \   |   /   /
        \  |  /   /
        South Pole
```

### Quaternary Subdivision

Each triangle is recursively subdivided into 4 children by connecting the midpoints of each edge (normalized to the unit sphere). This produces:

- **Child 0 (Apex):** Original vertex A + two midpoints
- **Child 1 (Left):** Original vertex B + two midpoints
- **Child 2 (Right):** Original vertex C + two midpoints
- **Child 3 (Center):** Three midpoints (the central triangle)

### Cumulative Indexing

All cells across all levels are numbered with a single contiguous index space:

```
Level 0:  indices 1 - 8          (8 base faces)
Level 1:  indices 9 - 40         (32 children)
Level 2:  indices 41 - 168       (128 children)
...
Level n:  S(n) = 8 * (4^(n+1) - 1) / 3
```

The index encodes the resolution level, the base face, and the full hierarchical path from root to cell.

### Neighbor Finding

Neighbors are found using the probe-reflection method: for each edge of a triangle, compute a probe point by reflecting the centroid across the edge midpoint, then encode the probe point at the same resolution. This naturally handles face boundaries.

## Specification

The formal specification is in `spec/SPECIFICATION.md`. Machine-readable test vectors are in:

- `spec/test-vectors.json` - 268 test vectors across 7 categories
- `spec/resolution-table.json` - Level boundaries and cell counts for all 31 levels

## Project Structure

```
Trilocation/
  src/
    Trilocation.Core/          Core library (indexing, geometry, algorithms)
    Trilocation.Data/          Entity Framework Core integration
    Trilocation.Interop/       H3, S2, Geohash, Plus Code interoperability
    Trilocation.Serialization/ JSON and binary serialization
  tests/
    Trilocation.Core.Tests/    625 unit tests
    Trilocation.Data.Tests/    EF Core integration tests
    Trilocation.Interop.Tests/ Interop conversion tests
    Trilocation.Spec.Tests/    Spec compliance tests
  benchmarks/
    Trilocation.Benchmarks/    BenchmarkDotNet performance tests
  spec/                        Formal specification and test vectors
  tools/
    TestVectorGenerator/       Generates test-vectors.json from C# reference
  demos/
    Trilocation.WebDemo/       Interactive map explorer (ASP.NET + Leaflet.js)
```

## Building

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run benchmarks
dotnet run --project benchmarks/Trilocation.Benchmarks -c Release

# Pack NuGet
dotnet pack -c Release
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on adding new language implementations and contributing to existing ones.

## Comparison with Other Systems

| Feature | Trilocation | H3 | S2 | Geohash |
|---------|-------------|----|----|---------|
| Cell shape | Triangle | Hexagon | Quad | Rectangle |
| Base mesh | Octahedron (8) | Icosahedron (122) | Cube (6) | None |
| Children per cell | 4 | 7 | 4 | 32 |
| Max resolution | 30 | 15 | 30 | 12 |
| Index type | uint64 | uint64 | uint64 | string |
| Equal-area cells | Approximate | Approximate | Approximate | No |
| Hierarchical | Yes | Approximate | Yes | Yes |
| Edge neighbors | 3 | 6 | 4 | 4-8 |

## License

AGPL-3.0-or-later

The specification (`spec/`) is licensed under Apache 2.0 to allow independent implementations.
