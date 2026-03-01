using Google.Common.Geometry;
using Trilocation.Core;

namespace Trilocation.Interop
{
    /// <summary>
    /// Converts between S2 (Google) Cell IDs and TriLocation.
    /// Uses S2Geometry NuGet package for S2 operations.
    /// All conversions go through WGS84 center-point mapping.
    /// </summary>
    public static class S2Converter
    {
        /// <summary>Converts an S2 Cell ID to TriLocation via WGS84 center point.</summary>
        public static TriLocation FromS2CellId(ulong s2CellId, int resolution)
        {
            var cellId = new S2CellId(s2CellId);
            S2LatLng center = cellId.ToLatLng();
            double lat = center.LatDegrees;
            double lon = center.LngDegrees;
            return new TriLocation(lat, lon, resolution);
        }

        /// <summary>Converts a TriLocation to an S2 Cell ID at the given S2 level (0-30).</summary>
        public static ulong ToS2CellId(TriLocation location, int s2Level)
        {
            if (s2Level < 0 || s2Level > 30)
            {
                throw new ArgumentOutOfRangeException(nameof(s2Level), "S2 level must be 0-30");
            }

            var (lat, lon) = location.ToLatLon();
            var latLng = S2LatLng.FromDegrees(lat, lon);
            var cellId = S2CellId.FromLatLng(latLng);
            var parentCell = cellId.ParentForLevel(s2Level);
            return parentCell.Id;
        }
    }
}
