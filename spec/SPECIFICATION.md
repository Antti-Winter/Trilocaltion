# Trilocation Specification

**Version:** 1.0
**Status:** Draft
**License:** Apache 2.0 (this specification only)

## Table of Contents

1. [Overview](#1-overview)
2. [Octahedral Base Mesh](#2-octahedral-base-mesh)
3. [Quaternary Subdivision](#3-quaternary-subdivision)
4. [Cumulative Indexing](#4-cumulative-indexing)
5. [Coordinate Conversion](#5-coordinate-conversion)
6. [Neighbor Finding](#6-neighbor-finding)
7. [Data Types](#7-data-types)
8. [Constants](#8-constants)

---

## 1. Overview

Trilocation is a hierarchical geospatial indexing system that divides the Earth's surface into triangular cells using an octahedral projection. The system supports 31 resolution levels (0-30), from 8 base triangles covering the entire globe down to sub-centimeter precision.

### Key Properties

- **Hierarchical:** Each triangle subdivides into exactly 4 children.
- **Global coverage:** 8 base faces cover the entire sphere without gaps or overlaps.
- **Unique indexing:** Every cell at every resolution has a unique 64-bit unsigned integer index.
- **Deterministic:** The same geographic coordinates always map to the same index at a given resolution.
- **Resolution-independent:** An index encodes both its position and resolution level.

### Index Structure

Indices are 1-based unsigned 64-bit integers. The index encodes:
- The resolution level (0-30)
- The base face (0-7)
- The hierarchical path from base face to the cell

---

## 2. Octahedral Base Mesh

The base mesh is a regular octahedron inscribed in the unit sphere with 6 vertices and 8 triangular faces.

### 2.1 Vertices

| Vertex | Cartesian (x, y, z) | Geographic |
|--------|---------------------|------------|
| North Pole (NP) | (0, 0, 1) | 90N, 0E |
| South Pole (SP) | (0, 0, -1) | 90S, 0E |
| Equator 0 (Eq0) | (1, 0, 0) | 0N, 0E |
| Equator 90 (Eq90) | (0, 1, 0) | 0N, 90E |
| Equator 180 (Eq180) | (-1, 0, 0) | 0N, 180E |
| Equator 270 (Eq270) | (0, -1, 0) | 0N, 90W |

### 2.2 Face Definitions

Faces are numbered 0-7. Vertices are ordered counterclockwise when viewed from outside the sphere.

| Face | Hemisphere | Vertex A | Vertex B | Vertex C | Longitude Range |
|------|-----------|----------|----------|----------|-----------------|
| 0 | North | NP | Eq0 | Eq90 | 0E - 90E |
| 1 | North | NP | Eq90 | Eq180 | 90E - 180E |
| 2 | North | NP | Eq180 | Eq270 | 180E - 270E (90W) |
| 3 | North | NP | Eq270 | Eq0 | 270E (90W) - 360E (0E) |
| 4 | South | SP | Eq90 | Eq0 | 0E - 90E |
| 5 | South | SP | Eq180 | Eq90 | 90E - 180E |
| 6 | South | SP | Eq270 | Eq180 | 180E - 270E (90W) |
| 7 | South | SP | Eq0 | Eq270 | 270E (90W) - 360E (0E) |

**Note:** Southern hemisphere faces have vertices B and C swapped compared to the northern pattern, maintaining counterclockwise winding.

### 2.3 Face Determination Algorithm

Given a 3D unit vector `point`:

1. Compute longitude: `lon = atan2(point.Y, point.X)`
2. Normalize to [0, 2pi): if `lon < 0` then `lon = lon + 2 * pi`
3. Determine quadrant: `q = floor(lon / (pi / 2))`; clamp to [0, 3]
4. If `point.Z >= 0`: face = q (northern hemisphere)
5. Else: face = q + 4 (southern hemisphere)

---

## 3. Quaternary Subdivision

Each triangle is subdivided into 4 child triangles by connecting the midpoints of its edges. All midpoints are normalized to the unit sphere (projected back onto the sphere surface).

### 3.1 Subdivision Process

Given a triangle with vertices A, B, C:

1. Compute spherical midpoints:
   - `midAB = Normalize(A + B)`
   - `midBC = Normalize(B + C)`
   - `midCA = Normalize(C + A)`

2. Create 4 child triangles:

| Child Index | Name | Vertices | Description |
|-------------|------|----------|-------------|
| 0 | Apex | (A, midAB, midCA) | Contains vertex A |
| 1 | Left | (midAB, B, midBC) | Contains vertex B |
| 2 | Right | (midCA, midBC, C) | Contains vertex C |
| 3 | Center | (midBC, midCA, midAB) | Inverted central triangle |

### 3.2 Properties

- Child 3 (center) has opposite winding compared to children 0-2.
- At high resolutions, the center triangle is approximately 40% larger than the corner triangles due to spherical distortion.
- Midpoint normalization ensures all vertices remain on the unit sphere.

---

## 4. Cumulative Indexing

All cells across all resolution levels are assigned unique indices using a cumulative indexing scheme.

### 4.1 Formulas

**Triangles at level n:**

    T(n) = 8 * 4^n

**Cumulative count up to and including level n:**

    S(n) = 8 * (4^(n+1) - 1) / 3

**Level start (first index at level n):**

    LevelStart(n) = S(n-1) + 1    for n >= 1
    LevelStart(0) = 1

**Level end (last index at level n):**

    LevelEnd(n) = S(n)

### 4.2 Index Ranges

| Level | T(n) | S(n) | LevelStart | LevelEnd |
|-------|------|------|------------|----------|
| 0 | 8 | 8 | 1 | 8 |
| 1 | 32 | 40 | 9 | 40 |
| 2 | 128 | 168 | 41 | 168 |
| 3 | 512 | 680 | 169 | 680 |
| 4 | 2,048 | 2,728 | 681 | 2,728 |
| 5 | 8,192 | 10,920 | 2,729 | 10,920 |
| ... | ... | ... | ... | ... |
| 30 | ~9.22e18 | ~1.23e19 | ... | ... |

The complete table for all 31 levels is provided in `resolution-table.json`.

### 4.3 Index Operations

**GetResolution(index):** Find which level an index belongs to by scanning LevelStart/LevelEnd tables.

**GetParent(index):**

    resolution = GetResolution(index)
    levelStart = LevelStart(resolution)
    positionInLevel = index - levelStart
    parentPosition = positionInLevel / 4    (integer division)
    parent = LevelStart(resolution - 1) + parentPosition

**GetChildren(index):**

    resolution = GetResolution(index)
    levelStart = LevelStart(resolution)
    positionInLevel = index - levelStart
    childLevelStart = LevelStart(resolution + 1)
    firstChild = childLevelStart + positionInLevel * 4
    children = [firstChild, firstChild+1, firstChild+2, firstChild+3]

**GetBaseFace(index):**

    Navigate up to resolution 0 using GetParent repeatedly.
    baseFace = index_at_res0 - 1

### 4.4 Index Properties

- Index 0 is reserved/invalid. Valid indices start at 1.
- Maximum valid index: S(30) = 12,297,829,382,473,034,408
- This fits within a 64-bit unsigned integer (max 18,446,744,073,709,551,615).
- Each index uniquely encodes its resolution level, base face, and hierarchical path.

---

## 5. Coordinate Conversion

### 5.1 LatLon to Index (Encoding)

**Input:** latitude (-90 to 90), longitude (-180 to 180), resolution (0 to 30)
**Output:** cumulative index (ulong)

**Algorithm:**

1. **Convert to 3D unit vector:**
   - `latRad = latitude * pi / 180`
   - `lonRad = longitude * pi / 180`
   - `x = cos(latRad) * cos(lonRad)`
   - `y = cos(latRad) * sin(lonRad)`
   - `z = sin(latRad)`

2. **Determine base face** (0-7) using the face determination algorithm (Section 2.3).

3. **Build hierarchical path** by iterative subdivision:
   - Set `currentTriangle` = base face triangle
   - For each level from 0 to resolution-1:
     - Subdivide `currentTriangle` into 4 children
     - Find child containing the point using the containment test (Section 5.3)
     - If no child passes containment test, select child with maximum `dot(point, child.Centroid)` (fallback for edge cases)
     - Store selected child index (0-3) in `path[level]`
     - Set `currentTriangle` = selected child

4. **Convert path to cumulative index:**
   - Start: `index = baseFace + 1`
   - For each level 1 to resolution:
     - `levelStart = LevelStart(level)`
     - `parentLevelStart = LevelStart(level - 1)`
     - `positionInLevel = (index - parentLevelStart) * 4 + path[level - 1]`
     - `index = levelStart + positionInLevel`
   - Return `index`

### 5.2 Index to LatLon (Decoding)

**Input:** cumulative index (ulong)
**Output:** (latitude, longitude) of cell centroid

**Algorithm:**

1. **Extract hierarchical path from index:**
   - `resolution = GetResolution(index)`
   - `current = index`
   - For each level from `resolution` down to 1:
     - `levelStart = LevelStart(level)`
     - `positionInLevel = current - levelStart`
     - `path[level - 1] = positionInLevel % 4`
     - `parentLevelStart = LevelStart(level - 1)`
     - `current = parentLevelStart + positionInLevel / 4`
   - `baseFace = current - 1`

2. **Navigate subdivision hierarchy:**
   - Set `currentTriangle` = base face triangle for `baseFace`
   - For each level 0 to `resolution - 1`:
     - Subdivide `currentTriangle` into 4 children
     - `currentTriangle = children[path[level]]`

3. **Compute centroid:**
   - `centroid = Normalize(vertex_A + vertex_B + vertex_C)`
   - Convert to geographic coordinates:
     - `latitude = asin(centroid.Z) * 180 / pi`
     - `longitude = atan2(centroid.Y, centroid.X) * 180 / pi`

### 5.3 Point-in-Triangle Test (Containment)

Given triangle (A, B, C) and a point P, all on the unit sphere:

1. Compute scalar triple products:
   - `d1 = A . (B x P)` (equivalent to `det(A, B, P)`)
   - `d2 = B . (C x P)` (equivalent to `det(B, C, P)`)
   - `d3 = C . (A x P)` (equivalent to `det(C, A, P)`)

2. Determine triangle orientation:
   - `orient = A . (B x C)` (equivalent to `det(A, B, C)`)

3. Test containment:
   - If `orient >= 0` (counterclockwise): P is inside if `d1 >= -epsilon AND d2 >= -epsilon AND d3 >= -epsilon`
   - If `orient < 0` (clockwise): P is inside if `d1 <= epsilon AND d2 <= epsilon AND d3 <= epsilon`
   - Where `epsilon = 1e-14` (numerical tolerance)

---

## 6. Neighbor Finding

### 6.1 Edge Neighbors (GetNeighbors)

Each triangle has exactly 3 edge-sharing neighbors. The algorithm uses a probe reflection method.

**Input:** TriLocation (index + resolution)
**Output:** Array of 3 TriLocation neighbors

**Algorithm:**

1. Retrieve the Triangle3D for the given index (using Section 5.2 path extraction).
2. Compute the triangle centroid: `centroid = Normalize(A + B + C)`
3. For each of the 3 edges (AB, BC, CA):
   - Compute edge midpoint: `edgeMid = Normalize(edgeVertex1 + edgeVertex2)`
   - Reflect centroid across edge midpoint: `probe = Normalize(edgeMid * 2.0 - centroid)`
   - Convert probe to (lat, lon): `lat = asin(probe.Z) * 180/pi`, `lon = atan2(probe.Y, probe.X) * 180/pi`
   - Encode probe as index at the same resolution: `neighborIndex = ToIndex(lat, lon, resolution)`
   - Create neighbor: `TriLocation(neighborIndex)`
4. Return 3 neighbors in order: [neighbor_AB, neighbor_BC, neighbor_CA]

### 6.2 Ring Enumeration (GetRing)

Returns all cells at exactly distance `radius` from the center cell.

**Algorithm:** Breadth-First Search

1. Initialize: `visited = {center.Index}`, `currentRing = [center]`
2. For each step from 1 to radius:
   - `nextRing = []`
   - For each cell in `currentRing`:
     - Get 3 neighbors
     - For each neighbor not in `visited`:
       - Add to `visited` and `nextRing`
   - `currentRing = nextRing`
3. Return `currentRing`

### 6.3 Disk Enumeration (GetNeighborsWithin)

Returns all cells within distance `distance` from the center cell (inclusive).

**Algorithm:** Breadth-First Search

1. Initialize: `visited = {center.Index}`, `result = [center]`, `currentLevel = [center]`
2. For each step from 0 to distance-1:
   - `nextLevel = []`
   - For each cell in `currentLevel`:
     - Get 3 neighbors
     - For each neighbor not in `visited`:
       - Add to `visited`, `nextLevel`, and `result`
   - `currentLevel = nextLevel`
3. Return `result`

---

## 7. Data Types

### 7.1 TriLocation

The primary data type representing a cell in the Trilocation system.

| Field | Type | Description |
|-------|------|-------------|
| Index | uint64 | Cumulative index (1-based) |
| Resolution | int | Resolution level (0-30), derived from Index |
| BaseFace | int | Base octahedral face (0-7), derived from Index |

**Construction:**
- From index: `TriLocation(index: uint64)`
- From coordinates: `TriLocation(latitude: float64, longitude: float64, resolution: int)`

**Key Methods:**
- `ToLatLon()` -> (latitude, longitude): Returns centroid coordinates
- `GetParent()` -> TriLocation: Returns parent cell (one level coarser)
- `GetChildren()` -> TriLocation[4]: Returns 4 child cells
- `GetNeighbors()` -> TriLocation[3]: Returns 3 edge-sharing neighbors
- `GetNeighborsWithin(distance: int)` -> TriLocation[]: Returns all cells within distance
- `Contains(other: TriLocation)` -> bool: Checks if other is a descendant

### 7.2 TriCell

Extended cell information including geometry.

| Field | Type | Description |
|-------|------|-------------|
| Location | TriLocation | The cell identity |
| VertexA | GeoPoint | First vertex in geographic coordinates |
| VertexB | GeoPoint | Second vertex |
| VertexC | GeoPoint | Third vertex |
| Centroid | GeoPoint | Centroid in geographic coordinates |
| AreaKm2 | float64 | Area in square kilometers |

### 7.3 GeoPoint

A geographic coordinate.

| Field | Type | Range | Description |
|-------|------|-------|-------------|
| Latitude | float64 | [-90, 90] | Degrees north |
| Longitude | float64 | [-180, 180] | Degrees east |

### 7.4 GeoBounds

An axis-aligned geographic bounding box.

| Field | Type | Description |
|-------|------|-------------|
| MinLatitude | float64 | Southern bound |
| MaxLatitude | float64 | Northern bound |
| MinLongitude | float64 | Western bound |
| MaxLongitude | float64 | Eastern bound |

---

## 8. Constants

### 8.1 Earth Parameters

| Constant | Value | Unit |
|----------|-------|------|
| Earth Radius | 6,371.0 | km |
| Earth Radius | 6,371,000.0 | m |
| Earth Surface Area | 510,065,623.0 | km2 |

### 8.2 System Limits

| Constant | Value |
|----------|-------|
| Maximum Resolution | 30 |
| Base Face Count | 8 |
| Children Per Triangle | 4 |
| Minimum Valid Index | 1 |
| Maximum Valid Index | 12,297,829,382,473,034,408 |

### 8.3 Conversion Factors

| Constant | Value |
|----------|-------|
| Degrees to Radians | pi / 180 |
| Radians to Degrees | 180 / pi |

### 8.4 Base Face Vertices (3D Unit Sphere)

| Vertex | X | Y | Z |
|--------|---|---|---|
| North Pole | 0.0 | 0.0 | 1.0 |
| South Pole | 0.0 | 0.0 | -1.0 |
| Equator 0 (0E) | 1.0 | 0.0 | 0.0 |
| Equator 90 (90E) | 0.0 | 1.0 | 0.0 |
| Equator 180 (180E) | -1.0 | 0.0 | 0.0 |
| Equator 270 (90W) | 0.0 | -1.0 | 0.0 |

### 8.5 Spherical Area Calculation

The area of a spherical triangle on the unit sphere is computed using the Van Oosterom-Strackee formula:

    T = A . (B x C)
    denominator = 1 + A.B + A.C + B.C
    halfExcess = atan2(|T|, denominator)
    sphericalExcess = 2 * |halfExcess|
    areaKm2 = sphericalExcess * EarthRadiusKm^2

### 8.6 Resolution Table Summary

| Level | Cell Count | Approx. Cell Area |
|-------|-----------|-------------------|
| 0 | 8 | ~63,758,000 km2 |
| 5 | 8,192 | ~62,000 km2 |
| 10 | 8,388,608 | ~60 km2 |
| 15 | 8,589,934,592 | ~0.06 km2 |
| 20 | ~8.8e12 | ~58 m2 |
| 24 | ~1.4e14 | ~3.6 m2 |
| 30 | ~9.2e18 | ~0.00006 m2 |

The complete resolution table with exact values is provided in `resolution-table.json`.

---

## Appendix A: Test Vectors

The file `test-vectors.json` contains reference test vectors generated from the C# reference implementation. Any conforming implementation must produce identical results for all test vectors.

### Categories

| Category | Count | Description |
|----------|-------|-------------|
| Level Boundaries | 31 | LevelStart/LevelEnd for each resolution |
| Parent-Child | 48+ | Hierarchical relationships |
| Base Faces | 8 | All base face geometries |
| Coordinate Conversions | 100+ | LatLon to index mappings |
| Neighbors | 18+ | Edge neighbor relationships |
| Containment | 10+ | Hierarchical containment |

### Important Notes

- All indices are represented as strings in JSON to preserve uint64 precision.
- Geographic coordinates use WGS84 datum.
- Latitude range: [-90, 90] degrees.
- Longitude range: [-180, 180] degrees.
- The coordinate conversion algorithm is deterministic: the same (lat, lon, resolution) input always produces the same index.

---

## Appendix B: Compliance Requirements

A conforming implementation MUST:

1. Produce identical indices for all coordinate conversion test vectors.
2. Produce identical parent-child relationships for all hierarchy test vectors.
3. Produce identical neighbor indices for all neighbor test vectors.
4. Support resolutions 0 through 30 inclusive.
5. Use 1-based cumulative indexing as defined in Section 4.
6. Use the same base face numbering and vertex ordering as defined in Section 2.

A conforming implementation SHOULD:

1. Use uint64 (or equivalent) for index storage.
2. Provide both encoding (LatLon to Index) and decoding (Index to LatLon) operations.
3. Support hierarchical navigation (parent, children, ancestors).
4. Support neighbor finding.
