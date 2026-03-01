using Trilocation.Core.Primitives;

namespace Trilocation.Core.Conversions
{
    /// <summary>
    /// Web Mercator (EPSG:3857) projection conversions.
    /// Valid latitude range: -85.06 to +85.06 degrees (Mercator limit).
    /// </summary>
    internal static class WebMercatorProjection
    {
        private const double MaxExtent = 20037508.342789244;

        /// <summary>Converts Web Mercator (x, y) meters to WGS84 (lat, lon) degrees.</summary>
        public static (double Latitude, double Longitude) ToWgs84(double x, double y)
        {
            double lon = x / MaxExtent * 180.0;
            double lat = Math.Atan(Math.Sinh(y / MaxExtent * Math.PI))
                * GeoConstants.RadiansToDegrees;
            return (lat, lon);
        }

        /// <summary>Converts WGS84 (lat, lon) degrees to Web Mercator (x, y) meters.</summary>
        public static (double X, double Y) FromWgs84(double latitude, double longitude)
        {
            double x = longitude * MaxExtent / 180.0;
            double latRad = latitude * GeoConstants.DegreesToRadians;
            double y = Math.Log(Math.Tan(Math.PI / 4.0 + latRad / 2.0))
                / Math.PI * MaxExtent;
            return (x, y);
        }
    }
}
