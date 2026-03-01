using Trilocation.Core.Indexing;
using Trilocation.Core.Primitives;

namespace Trilocation.Core.Extensions
{
    /// <summary>
    /// Extension methods for IEnumerable of TriLocation.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>Returns the bounding box enclosing all given locations.</summary>
        public static GeoBounds GetBounds(this IEnumerable<TriLocation> locations)
        {
            double minLat = double.MaxValue;
            double maxLat = double.MinValue;
            double minLon = double.MaxValue;
            double maxLon = double.MinValue;

            foreach (var loc in locations)
            {
                TriCell cell = loc.ToCell();
                GeoBounds cellBounds = cell.GetBounds();
                if (cellBounds.MinLatitude < minLat) minLat = cellBounds.MinLatitude;
                if (cellBounds.MaxLatitude > maxLat) maxLat = cellBounds.MaxLatitude;
                if (cellBounds.MinLongitude < minLon) minLon = cellBounds.MinLongitude;
                if (cellBounds.MaxLongitude > maxLon) maxLon = cellBounds.MaxLongitude;
            }

            return new GeoBounds(minLat, maxLat, minLon, maxLon);
        }

        /// <summary>Groups locations by their parent triangle.</summary>
        public static IEnumerable<IGrouping<TriLocation, TriLocation>> GroupByParent(
            this IEnumerable<TriLocation> locations)
        {
            return locations
                .Where(l => l.Resolution > 0)
                .GroupBy(l => l.GetParent());
        }

        /// <summary>
        /// Compacts locations by merging complete sibling sets into their parent.
        /// Repeats until no more compaction is possible.
        /// </summary>
        public static IEnumerable<TriLocation> Compact(this IEnumerable<TriLocation> locations)
        {
            var indices = new HashSet<ulong>(locations.Select(l => l.Index));
            bool changed = true;

            while (changed)
            {
                changed = false;
                var nextIndices = new HashSet<ulong>();
                var processedParents = new HashSet<ulong>();

                // First, keep all resolution 0 entries
                foreach (var index in indices)
                {
                    int resolution = CumulativeIndex.GetResolution(index);
                    if (resolution == 0)
                    {
                        nextIndices.Add(index);
                    }
                }

                // Process non-zero resolution entries
                foreach (var index in indices)
                {
                    int resolution = CumulativeIndex.GetResolution(index);
                    if (resolution == 0) continue;

                    ulong parentIndex = CumulativeIndex.GetParent(index);
                    if (processedParents.Contains(parentIndex)) continue;
                    processedParents.Add(parentIndex);

                    ulong[] siblings = CumulativeIndex.GetChildren(parentIndex);
                    bool allPresent = true;
                    for (int i = 0; i < siblings.Length; i++)
                    {
                        if (!indices.Contains(siblings[i]))
                        {
                            allPresent = false;
                            break;
                        }
                    }

                    if (allPresent)
                    {
                        nextIndices.Add(parentIndex);
                        changed = true;
                    }
                    else
                    {
                        for (int i = 0; i < siblings.Length; i++)
                        {
                            if (indices.Contains(siblings[i]))
                            {
                                nextIndices.Add(siblings[i]);
                            }
                        }
                    }
                }

                indices = nextIndices;
            }

            return indices.Select(i => new TriLocation(i));
        }
    }
}
