using Trilocation.Core.Geometry;

namespace Trilocation.Core.Algorithms
{
    /// <summary>
    /// Finds edge-neighbors for triangles at any resolution level.
    /// Each triangle has exactly 3 edge-neighbors (one per edge).
    /// Uses a probe-based approach: reflects the centroid across each edge
    /// midpoint and finds which triangle contains the probe point.
    /// </summary>
    internal static class NeighborFinder
    {
        /// <summary>
        /// Returns the 3 edge-neighbors of the given triangle.
        /// Neighbor 0: across edge A-B, Neighbor 1: across edge B-C, Neighbor 2: across edge C-A.
        /// </summary>
        public static TriLocation[] GetNeighbors(TriLocation location)
        {
            Triangle3D triangle = CoordinateConverter.GetTriangle(location.Index);
            int resolution = location.Resolution;

            // Edge midpoints on the unit sphere
            Vector3D midAB = SphericalMath.MidpointOnSphere(triangle.A, triangle.B);
            Vector3D midBC = SphericalMath.MidpointOnSphere(triangle.B, triangle.C);
            Vector3D midCA = SphericalMath.MidpointOnSphere(triangle.C, triangle.A);

            Vector3D[] edgeMidpoints = new Vector3D[] { midAB, midBC, midCA };
            TriLocation[] neighbors = new TriLocation[3];

            for (int i = 0; i < 3; i++)
            {
                // Probe: reflect centroid across edge midpoint, then normalize to sphere
                Vector3D probe = (edgeMidpoints[i] * 2.0 - triangle.Centroid).Normalize();
                var (lat, lon) = probe.ToLatLon();
                ulong neighborIndex = CoordinateConverter.ToIndex(lat, lon, resolution);
                neighbors[i] = new TriLocation(neighborIndex);
            }

            return neighbors;
        }

        /// <summary>
        /// Returns all triangles at exactly the given grid-distance from the center.
        /// Distance 1 = the 3 direct neighbors (same as GetNeighbors).
        /// </summary>
        public static TriLocation[] GetRing(TriLocation center, int radius)
        {
            if (radius <= 0)
            {
                return new TriLocation[] { center };
            }

            var visited = new HashSet<ulong> { center.Index };
            var currentRing = new List<TriLocation> { center };

            for (int r = 1; r <= radius; r++)
            {
                var nextRing = new List<TriLocation>();
                foreach (var loc in currentRing)
                {
                    TriLocation[] neighbors = GetNeighbors(loc);
                    foreach (var neighbor in neighbors)
                    {
                        if (visited.Add(neighbor.Index))
                        {
                            nextRing.Add(neighbor);
                        }
                    }
                }
                currentRing = nextRing;
            }

            return currentRing.ToArray();
        }

        /// <summary>
        /// Returns all triangles within the given grid-distance from the center.
        /// Includes the center triangle itself. Distance 0 = only center.
        /// </summary>
        public static TriLocation[] GetNeighborsWithin(TriLocation center, int distance)
        {
            var visited = new HashSet<ulong> { center.Index };
            var result = new List<TriLocation> { center };
            var currentLevel = new List<TriLocation> { center };

            for (int d = 0; d < distance; d++)
            {
                var nextLevel = new List<TriLocation>();
                foreach (var loc in currentLevel)
                {
                    TriLocation[] neighbors = GetNeighbors(loc);
                    foreach (var neighbor in neighbors)
                    {
                        if (visited.Add(neighbor.Index))
                        {
                            nextLevel.Add(neighbor);
                            result.Add(neighbor);
                        }
                    }
                }
                currentLevel = nextLevel;
            }

            return result.ToArray();
        }
    }
}
