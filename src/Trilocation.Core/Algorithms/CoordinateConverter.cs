using Trilocation.Core.Geometry;
using Trilocation.Core.Indexing;
using Trilocation.Core.Primitives;

namespace Trilocation.Core.Algorithms
{
    /// <summary>
    /// Converts between WGS84 coordinates (lat/lon) and cumulative triangle indices.
    /// </summary>
    internal static class CoordinateConverter
    {
        /// <summary>
        /// Encodes a lat/lon point to a cumulative triangle index at the given resolution.
        /// Algorithm: projectplan.md section 4.1.
        /// </summary>
        public static ulong ToIndex(double latitude, double longitude, int resolution)
        {
            if (resolution < 0 || resolution > IndexConstants.MaxResolution)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resolution),
                    "Resolution must be between 0 and " + IndexConstants.MaxResolution);
            }

            // Step 1: Convert to 3D unit vector
            Vector3D point = Vector3D.FromLatLon(latitude, longitude);

            // Step 2: Determine base face
            int baseFace = OctahedralProjection.PointToFace(point);

            // Step 3: Build path through subdivision hierarchy
            Triangle3D[] baseFaces = OctahedralProjection.GetBaseFaces();
            Triangle3D currentTriangle = baseFaces[baseFace];
            int[] path = new int[resolution];

            for (int level = 0; level < resolution; level++)
            {
                Triangle3D[] children = currentTriangle.Subdivide();
                bool found = false;

                for (int j = 0; j < 4; j++)
                {
                    if (children[j].Contains(point))
                    {
                        path[level] = j;
                        currentTriangle = children[j];
                        found = true;
                        break;
                    }
                }

                // Fallback: pick closest child by dot product with centroid
                if (!found)
                {
                    double bestDot = double.MinValue;
                    int bestChild = 0;
                    for (int j = 0; j < 4; j++)
                    {
                        double dot = point.Dot(children[j].Centroid);
                        if (dot > bestDot)
                        {
                            bestDot = dot;
                            bestChild = j;
                        }
                    }
                    path[level] = bestChild;
                    currentTriangle = children[bestChild];
                }
            }

            // Step 4: Convert path to cumulative index
            ulong index = (ulong)(baseFace + 1); // Level 0: 1-based

            for (int level = 1; level <= resolution; level++)
            {
                int step = path[level - 1];
                ulong levelStart = IndexConstants.LevelStartTable[level];
                ulong parentLevelStart = IndexConstants.LevelStartTable[level - 1];
                ulong positionInLevel = (index - parentLevelStart) * 4 + (ulong)step;
                index = levelStart + positionInLevel;
            }

            return index;
        }

        /// <summary>
        /// Decodes a cumulative triangle index to lat/lon coordinates (centroid of the triangle).
        /// Algorithm: projectplan.md section 4.2.
        /// </summary>
        public static (double Latitude, double Longitude) ToLatLon(ulong index)
        {
            IndexValidator.ValidateIndex(index);

            Triangle3D triangle = GetTriangle(index);
            return triangle.Centroid.ToLatLon();
        }

        /// <summary>
        /// Returns the Triangle3D geometry for the given cumulative index.
        /// </summary>
        public static Triangle3D GetTriangle(ulong index)
        {
            IndexValidator.ValidateIndex(index);

            int resolution = CumulativeIndex.GetResolution(index);

            // Step 1: Extract hierarchical path from cumulative index
            // Optimized: inline parent calculation to avoid GetResolution() call in GetParent()
            int[] path = new int[resolution];
            ulong current = index;

            for (int level = resolution; level >= 1; level--)
            {
                ulong levelStart = IndexConstants.LevelStartTable[level];
                ulong positionInLevel = current - levelStart;
                path[level - 1] = (int)(positionInLevel % 4);
                ulong parentLevelStart = IndexConstants.LevelStartTable[level - 1];
                current = parentLevelStart + positionInLevel / 4;
            }

            int baseFace = (int)(current - 1);

            // Step 2: Navigate subdivision hierarchy following the path
            Triangle3D[] baseFaces = OctahedralProjection.GetBaseFaces();
            Triangle3D currentTriangle = baseFaces[baseFace];

            for (int i = 0; i < resolution; i++)
            {
                Triangle3D[] children = currentTriangle.Subdivide();
                currentTriangle = children[path[i]];
            }

            return currentTriangle;
        }
    }
}
