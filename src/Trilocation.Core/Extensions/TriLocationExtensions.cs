using System.Globalization;

namespace Trilocation.Core.Extensions
{
    /// <summary>
    /// Extension methods for TriLocation.
    /// </summary>
    public static class TriLocationExtensions
    {
        /// <summary>Checks whether the location is at the specified resolution.</summary>
        public static bool IsAtResolution(this TriLocation location, int resolution)
        {
            return location.Resolution == resolution;
        }

        /// <summary>Checks whether the location is a base face (resolution 0).</summary>
        public static bool IsBaseFace(this TriLocation location)
        {
            return location.Resolution == 0;
        }

        /// <summary>Returns the 3 siblings (other children of the same parent).</summary>
        public static TriLocation[] GetSiblings(this TriLocation location)
        {
            if (location.Resolution == 0)
            {
                throw new InvalidOperationException(
                    "Base faces (resolution 0) have no siblings");
            }

            TriLocation parent = location.GetParent();
            TriLocation[] children = parent.GetChildren();

            return children.Where(c => c.Index != location.Index).ToArray();
        }

        /// <summary>Returns the GeoJSON Feature representation of this triangle.</summary>
        public static string ToGeoJson(this TriLocation location)
        {
            TriCell cell = location.ToCell();
            var (latA, lonA) = (cell.VertexA.Latitude, cell.VertexA.Longitude);
            var (latB, lonB) = (cell.VertexB.Latitude, cell.VertexB.Longitude);
            var (latC, lonC) = (cell.VertexC.Latitude, cell.VertexC.Longitude);

            // GeoJSON uses [longitude, latitude] order
            string coordA = "[" + FormatDouble(lonA) + "," + FormatDouble(latA) + "]";
            string coordB = "[" + FormatDouble(lonB) + "," + FormatDouble(latB) + "]";
            string coordC = "[" + FormatDouble(lonC) + "," + FormatDouble(latC) + "]";

            return "{"
                + "\"type\":\"Feature\","
                + "\"properties\":{"
                + "\"index\":" + location.Index + ","
                + "\"resolution\":" + location.Resolution + ","
                + "\"baseFace\":" + location.BaseFace
                + "},"
                + "\"geometry\":{"
                + "\"type\":\"Polygon\","
                + "\"coordinates\":[[" + coordA + "," + coordB + "," + coordC + "," + coordA + "]]"
                + "}"
                + "}";
        }

        private static string FormatDouble(double value)
        {
            return value.ToString("G", CultureInfo.InvariantCulture);
        }
    }
}
