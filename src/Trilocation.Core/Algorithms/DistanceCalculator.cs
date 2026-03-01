using Trilocation.Core.Primitives;

namespace Trilocation.Core.Algorithms
{
    /// <summary>
    /// Calculates geographic and grid distances between triangle locations.
    /// </summary>
    internal static class DistanceCalculator
    {
        private const int MaxGridSearchDistance = 100;

        /// <summary>Returns the geographic distance between two TriLocations in meters (Haversine).</summary>
        public static double GetDistance(TriLocation a, TriLocation b)
        {
            if (a.Index == b.Index)
            {
                return 0.0;
            }

            var (latA, lonA) = a.ToLatLon();
            var (latB, lonB) = b.ToLatLon();
            GeoPoint pointA = new GeoPoint(latA, lonA);
            GeoPoint pointB = new GeoPoint(latB, lonB);

            return pointA.DistanceTo(pointB);
        }

        /// <summary>
        /// Returns the grid distance (minimum edge-crossings) between two TriLocations.
        /// Both must be at the same resolution. Uses bidirectional BFS.
        /// Returns -1 if the distance exceeds the search limit.
        /// </summary>
        public static int GetGridDistance(TriLocation a, TriLocation b)
        {
            if (a.Resolution != b.Resolution)
            {
                throw new ArgumentException(
                    "Both locations must be at the same resolution. "
                    + "A: " + a.Resolution + ", B: " + b.Resolution);
            }

            if (a.Index == b.Index)
            {
                return 0;
            }

            // Bidirectional BFS
            var visitedA = new Dictionary<ulong, int> { { a.Index, 0 } };
            var visitedB = new Dictionary<ulong, int> { { b.Index, 0 } };
            var frontA = new List<TriLocation> { a };
            var frontB = new List<TriLocation> { b };
            int distA = 0;
            int distB = 0;

            while (frontA.Count > 0 && frontB.Count > 0 && distA + distB < MaxGridSearchDistance)
            {
                // Expand smaller frontier
                if (frontA.Count <= frontB.Count)
                {
                    distA++;
                    var nextFront = new List<TriLocation>();
                    foreach (var loc in frontA)
                    {
                        TriLocation[] neighbors = NeighborFinder.GetNeighbors(loc);
                        foreach (var n in neighbors)
                        {
                            if (visitedB.TryGetValue(n.Index, out int db))
                            {
                                return distA + db;
                            }
                            if (!visitedA.ContainsKey(n.Index))
                            {
                                visitedA[n.Index] = distA;
                                nextFront.Add(n);
                            }
                        }
                    }
                    frontA = nextFront;
                }
                else
                {
                    distB++;
                    var nextFront = new List<TriLocation>();
                    foreach (var loc in frontB)
                    {
                        TriLocation[] neighbors = NeighborFinder.GetNeighbors(loc);
                        foreach (var n in neighbors)
                        {
                            if (visitedA.TryGetValue(n.Index, out int da))
                            {
                                return da + distB;
                            }
                            if (!visitedB.ContainsKey(n.Index))
                            {
                                visitedB[n.Index] = distB;
                                nextFront.Add(n);
                            }
                        }
                    }
                    frontB = nextFront;
                }
            }

            return -1;
        }
    }
}
