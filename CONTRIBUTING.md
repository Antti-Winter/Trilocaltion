# Contributing to Trilocation

Thank you for your interest in contributing to the Trilocation project! This document provides guidelines for adding new language implementations and contributing to existing ones.

## Adding a New Language Implementation

### Required Files

Every language implementation must include:

1. **Source code** implementing the core modules:
   - `CumulativeIndex` - Index arithmetic (S(n) = 8 * (4^(n+1) - 1) / 3)
   - `Vector3D` - 3D unit sphere vector operations
   - `Triangle3D` - Spherical triangle with subdivision and containment
   - `OctahedralProjection` - 8 base faces and face determination
   - `CoordinateConverter` - Lat/lon to index and back
   - `NeighborFinder` - Edge-sharing neighbor finding via probe reflection
   - `TriLocation` - Primary data type wrapping the index

2. **Spec compliance tests** reading `spec/test-vectors.json` and `spec/resolution-table.json`

3. **Package configuration** for the language's package manager

4. **LICENSE** file (Apache 2.0)

### API Naming Conventions

Use idiomatic naming for each language:

| Concept | C# | TypeScript | Python | Java | Rust | Go |
|---------|-----|-----------|--------|------|------|-----|
| Class/Type | `TriLocation` | `TriLocation` | `TriLocation` | `TriLocation` | `TriLocation` | `TriLocation` |
| To index | `ToIndex()` | `toIndex()` | `to_index()` | `toIndex()` | `to_index()` | `ToIndex()` |
| To lat/lon | `ToLatLon()` | `toLatLon()` | `to_lat_lon()` | `toLatLon()` | `to_lat_lon()` | `ToLatLon()` |
| Get parent | `GetParent()` | `getParent()` | `get_parent()` | `getParent()` | `parent()` | `Parent()` |
| Get children | `GetChildren()` | `getChildren()` | `get_children()` | `getChildren()` | `children()` | `Children()` |
| Get neighbors | `GetNeighbors()` | `getNeighbors()` | `get_neighbors()` | `getNeighbors()` | `neighbors()` | `Neighbors()` |
| Contains | `Contains()` | `contains()` | `contains()` | `contains()` | `contains()` | `Contains()` |

### Spec Compliance Tests

Your implementation **must** pass all test vectors from `spec/test-vectors.json`. The test file contains 8 categories:

1. **Resolution Table** (31 levels) - Verify `levelStart`, `levelEnd`, `cumulativeCount`
2. **Level Boundaries** (31 levels) - Same checks from test-vectors.json
3. **Parent-Child** (48+ pairs) - Verify `getResolution`, `getBaseFace`, `getParent`, `getChildren`
4. **Base Faces** (8 faces) - Verify centroid lat/lon (1e-9 tolerance)
5. **Coordinate Conversions** (100+ entries) - Verify `toIndex`, centroid lat/lon
6. **Neighbors** (18+ entries) - Verify `getNeighbors` and `getNeighborsWithin`
7. **Containment** (10+ entries) - Verify `contains` relationship

### Critical Implementation Notes

#### Unsigned 64-bit Overflow (Level 30)

S(30) = 12,297,829,382,473,034,408 exceeds signed 64-bit max (9,223,372,036,854,775,807).

Solutions by language type:
- **Unsigned types** (C, C++, Rust, Go, Swift): Use `uint64_t` / `u64` / `UInt64` directly
- **Signed types** (Java, Kotlin, Dart): Divide by 3 first: `(power - 1) / 3 * 8`, use unsigned comparison functions
- **Arbitrary precision** (Python, Ruby, TypeScript BigInt): No overflow issues

#### Cumulative Count Formula

```
S(n) = 8 * (4^(n+1) - 1) / 3
```

IMPORTANT: In languages with fixed-size signed integers, compute as:
```
power = 1 << (2 * (n + 1))
result = (power - 1) / 3 * 8    // Divide by 3 FIRST to prevent overflow
```

#### Floating-Point Tolerance

Centroid lat/lon comparisons use 1e-9 tolerance across all implementations (IEEE 754 differences).

#### Face Determination Algorithm

Use the division-based algorithm matching the C# reference:
```
lon = atan2(y, x)
if (lon < 0) lon += 2 * pi
quadrant = (int)(lon / (pi / 2))
if (quadrant > 3) quadrant = 3
```

Do NOT use an if/else chain with atan2 ranges, as it produces different results at face boundaries (e.g., lon = pi).

### Directory Structure

```
trilocation-{lang}/
  src/ or lib/           Source code
  test/ or tests/        Spec compliance tests
  README.md              Language-specific documentation
  LICENSE                Apache 2.0
  {package-config}       e.g., package.json, Cargo.toml, setup.py
```

### Existing Implementations

Each language implementation lives in its own repository (`trilocation-{lang}`).

| Language | Repository | Package | Tests |
|----------|-----------|---------|-------|
| C# | This repo | NuGet: Trilocation | 625 GREEN |
| TypeScript | `trilocation-ts` | npm: @trilocation/core | 8/8 GREEN |
| Python | `trilocation-py` | PyPI: trilocation | 8/8 GREEN |
| Java | `trilocation-java` | Maven: io.trilocation | 8/8 GREEN |
| Kotlin | `trilocation-kotlin` | Maven: io.trilocation | 8/8 GREEN |
| Rust | `trilocation-rust` | crates.io: trilocation | 8/8 GREEN |
| Go | `trilocation-go` | Go modules | Written |
| Swift | `trilocation-swift` | SPM: Trilocation | Written |
| Dart | `trilocation-dart` | pub.dev: trilocation | 8/8 GREEN |
| C++ | `trilocation-cpp` | Header-only (CMake) | 8/8 GREEN |
| C | `trilocation-c` | Header-only (C99) | 1303/1303 GREEN |
| Objective-C | `trilocation-objc` | CocoaPods/SPM | Written |
| WASM | `trilocation-wasm` | npm: @trilocation/wasm | Built from Rust |
| Ruby | `trilocation-ruby` | RubyGems: trilocation | Written |
| PHP | `trilocation-php` | Packagist: trilocation | Written |
| R | `trilocation-r` | CRAN: trilocation | Written |
| Scala | `trilocation-scala` | Maven: io.trilocation | Written |
| Elixir | `trilocation-elixir` | Hex: trilocation | Written |
| Lua | `trilocation-lua` | LuaRocks: trilocation | Written |
| Julia | `trilocation-julia` | Pkg: Trilocation.jl | Written |

## Submitting Changes

1. Fork the repository
2. Create a feature branch
3. Ensure all spec compliance tests pass
4. Submit a pull request with:
   - Description of changes
   - Test results
   - Any language-specific notes

## Code Style

- Follow idiomatic conventions for each language
- All code comments in English
- Use string concatenation (+ operator), not template literals
- No `any` or `unknown` types in TypeScript

## Questions?

Open an issue with the label "question" for any implementation questions.
