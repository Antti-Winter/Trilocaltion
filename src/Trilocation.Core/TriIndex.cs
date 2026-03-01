using Trilocation.Core.Algorithms;
using Trilocation.Core.Geometry;
using Trilocation.Core.Indexing;
using Trilocation.Core.Primitives;

namespace Trilocation.Core
{
    /// <summary>
    /// Static facade for all Trilocation operations.
    /// Provides a single entry point for creating, querying, and manipulating triangle locations.
    /// </summary>
    public static class TriIndex
    {
        /// <summary>Creates a TriLocation from lat/lon coordinates at the given resolution.</summary>
        public static TriLocation FromLatLon(double latitude, double longitude, int resolution)
        {
            return new TriLocation(latitude, longitude, resolution);
        }

        /// <summary>Creates a TriLocation from a cumulative index.</summary>
        public static TriLocation FromIndex(ulong index)
        {
            return new TriLocation(index);
        }

        /// <summary>Returns all TriLocations that overlap with the given bounds at the given resolution.</summary>
        public static TriLocation[] FromBounds(GeoBounds bounds, int resolution)
        {
            if (resolution < 0 || resolution > IndexConstants.MaxResolution)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resolution),
                    "Resolution must be between 0 and " + IndexConstants.MaxResolution);
            }

            var result = new List<TriLocation>();
            Triangle3D[] baseFaces = OctahedralProjection.GetBaseFaces();

            for (int face = 0; face < 8; face++)
            {
                if (TriangleBoundsOverlap(baseFaces[face], bounds))
                {
                    SearchWithinBounds(
                        (ulong)(face + 1), baseFaces[face], 0, resolution, bounds, result);
                }
            }

            return result.ToArray();
        }

        /// <summary>Returns the cumulative count S(n) = 8 * (4^(n+1) - 1) / 3.</summary>
        public static ulong CumulativeCount(int resolution)
        {
            return CumulativeIndex.CumulativeCount(resolution);
        }

        /// <summary>Returns the first index of the given resolution level.</summary>
        public static ulong LevelStart(int resolution)
        {
            return CumulativeIndex.LevelStart(resolution);
        }

        /// <summary>Returns the last index of the given resolution level.</summary>
        public static ulong LevelEnd(int resolution)
        {
            return CumulativeIndex.LevelEnd(resolution);
        }

        /// <summary>Determines the resolution level from the index magnitude.</summary>
        public static int GetResolution(ulong index)
        {
            return CumulativeIndex.GetResolution(index);
        }

        /// <summary>Returns the parent TriLocation (one level up).</summary>
        public static TriLocation GetParent(TriLocation location)
        {
            return HierarchyNavigator.GetParent(location);
        }

        /// <summary>Returns the 4 child TriLocations (one level down).</summary>
        public static TriLocation[] GetChildren(TriLocation location)
        {
            return HierarchyNavigator.GetChildren(location);
        }

        /// <summary>Returns all descendants down to the specified depth.</summary>
        public static TriLocation[] GetDescendants(TriLocation location, int depth)
        {
            return HierarchyNavigator.GetDescendants(location, depth);
        }

        /// <summary>Returns the 3 edge-neighbors of the given triangle.</summary>
        public static TriLocation[] GetNeighbors(TriLocation location)
        {
            return NeighborFinder.GetNeighbors(location);
        }

        /// <summary>Returns all triangles at exactly the given grid-distance from the center.</summary>
        public static TriLocation[] GetRing(TriLocation center, int radius)
        {
            return NeighborFinder.GetRing(center, radius);
        }

        /// <summary>Returns the geographic distance between two TriLocations in meters.</summary>
        public static double GetDistance(TriLocation a, TriLocation b)
        {
            return DistanceCalculator.GetDistance(a, b);
        }

        /// <summary>Returns the grid distance (number of edge-crossings) between two TriLocations.</summary>
        public static int GetGridDistance(TriLocation a, TriLocation b)
        {
            return DistanceCalculator.GetGridDistance(a, b);
        }

        /// <summary>Checks whether a cumulative index is valid.</summary>
        public static bool IsValid(ulong index)
        {
            return IndexValidator.IsValid(index);
        }

        private static void SearchWithinBounds(
            ulong index, Triangle3D triangle, int currentRes, int targetRes,
            GeoBounds bounds, List<TriLocation> result)
        {
            if (currentRes == targetRes)
            {
                result.Add(new TriLocation(index));
                return;
            }

            Triangle3D[] children = triangle.Subdivide();
            ulong[] childIndices = CumulativeIndex.GetChildren(index);

            for (int i = 0; i < 4; i++)
            {
                if (TriangleBoundsOverlap(children[i], bounds))
                {
                    SearchWithinBounds(
                        childIndices[i], children[i], currentRes + 1, targetRes, bounds, result);
                }
            }
        }

        private static bool TriangleBoundsOverlap(Triangle3D triangle, GeoBounds bounds)
        {
            var (latA, lonA) = triangle.A.ToLatLon();
            var (latB, lonB) = triangle.B.ToLatLon();
            var (latC, lonC) = triangle.C.ToLatLon();

            double minLat = Math.Min(latA, Math.Min(latB, latC));
            double maxLat = Math.Max(latA, Math.Max(latB, latC));
            double minLon = Math.Min(lonA, Math.Min(lonB, lonC));
            double maxLon = Math.Max(lonA, Math.Max(lonB, lonC));

            GeoBounds triangleBounds = new GeoBounds(minLat, maxLat, minLon, maxLon);
            return bounds.Intersects(triangleBounds);
        }
    }
}
