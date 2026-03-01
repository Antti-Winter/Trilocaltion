using H3Lib;
using H3Lib.Extensions;
using Trilocation.Core;

namespace Trilocation.Interop
{
    /// <summary>
    /// Converts between H3 (Uber) hexagonal indices and TriLocation.
    /// Uses H3Lib NuGet package for H3 operations.
    /// All conversions go through WGS84 center-point mapping.
    /// </summary>
    public static class H3Converter
    {
        /// <summary>Converts an H3 index to TriLocation via WGS84 center point.</summary>
        public static TriLocation FromH3(ulong h3Index, int resolution)
        {
            H3Index h3 = h3Index;
            GeoCoord center = h3.ToGeoCoord();
            double lat = (double)(center.Latitude * (180.0m / (decimal)Math.PI));
            double lon = (double)(center.Longitude * (180.0m / (decimal)Math.PI));
            return new TriLocation(lat, lon, resolution);
        }

        /// <summary>Converts a TriLocation to an H3 index at the given H3 resolution (0-15).</summary>
        public static ulong ToH3(TriLocation location, int h3Resolution)
        {
            if (h3Resolution < 0 || h3Resolution > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(h3Resolution), "H3 resolution must be 0-15");
            }

            var (lat, lon) = location.ToLatLon();
            decimal latRad = (decimal)lat * ((decimal)Math.PI / 180.0m);
            decimal lonRad = (decimal)lon * ((decimal)Math.PI / 180.0m);
            var geoCoord = new GeoCoord(latRad, lonRad);
            H3Index h3 = Api.GeoToH3(geoCoord, h3Resolution);
            return h3;
        }
    }
}
