---
name: "New Language Implementation"
about: "Propose or track a new language implementation"
title: "New language implementation: [LANGUAGE]"
labels: "new-language"
---

## Language

**Language:** [e.g., Haskell, F#, Clojure]
**Package manager:** [e.g., Hackage, NuGet, Clojars]

## Implementation Plan

- [ ] Core modules (CumulativeIndex, Vector3D, Triangle3D, OctahedralProjection, CoordinateConverter, NeighborFinder, TriLocation)
- [ ] Spec compliance tests (reading test-vectors.json)
- [ ] Package configuration
- [ ] README with usage examples

## 64-bit Integer Strategy

How does this language handle 64-bit unsigned integers?

- [ ] Native unsigned 64-bit type (e.g., uint64, u64)
- [ ] Signed 64-bit with unsigned comparison functions
- [ ] Arbitrary precision integers
- [ ] Other: [describe]

## Additional Notes

[Any language-specific considerations, ecosystem requirements, etc.]
