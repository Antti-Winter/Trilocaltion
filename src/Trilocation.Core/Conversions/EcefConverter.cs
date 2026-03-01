using Trilocation.Core.Primitives;

namespace Trilocation.Core.Conversions
{
    /// <summary>
    /// ECEF (Earth-Centered, Earth-Fixed) coordinate conversions using WGS84 ellipsoid.
    /// </summary>
    internal static class EcefConverter
    {
        // WGS84 ellipsoid parameters
        private const double SemiMajorAxis = 6378137.0;
        private const double Flattening = 1.0 / 298.257223563;
        private static readonly double SemiMinorAxis = SemiMajorAxis * (1.0 - Flattening);
        private static readonly double EccentricitySquared = 2.0 * Flattening - Flattening * Flattening;

        /// <summary>Converts ECEF (x, y, z) meters to WGS84 lat/lon degrees (Bowring iterative).</summary>
        public static (double Latitude, double Longitude) ToWgs84(double x, double y, double z)
        {
            double lon = Math.Atan2(y, x) * GeoConstants.RadiansToDegrees;

            double p = Math.Sqrt(x * x + y * y);
            // Initial estimate using parametric latitude
            double theta = Math.Atan2(z * SemiMajorAxis, p * SemiMinorAxis);
            double sinTheta = Math.Sin(theta);
            double cosTheta = Math.Cos(theta);

            double lat = Math.Atan2(
                z + EccentricitySquared / (1.0 - EccentricitySquared)
                    * SemiMinorAxis * sinTheta * sinTheta * sinTheta,
                p - EccentricitySquared * SemiMajorAxis
                    * cosTheta * cosTheta * cosTheta);

            double latDeg = lat * GeoConstants.RadiansToDegrees;
            return (latDeg, lon);
        }

        /// <summary>Converts WGS84 lat/lon degrees to ECEF (x, y, z) meters (height = 0).</summary>
        public static (double X, double Y, double Z) FromWgs84(double latitude, double longitude)
        {
            double latRad = latitude * GeoConstants.DegreesToRadians;
            double lonRad = longitude * GeoConstants.DegreesToRadians;

            double sinLat = Math.Sin(latRad);
            double cosLat = Math.Cos(latRad);
            double sinLon = Math.Sin(lonRad);
            double cosLon = Math.Cos(lonRad);

            // Prime vertical radius of curvature
            double n = SemiMajorAxis / Math.Sqrt(1.0 - EccentricitySquared * sinLat * sinLat);

            double x = n * cosLat * cosLon;
            double y = n * cosLat * sinLon;
            double z = n * (1.0 - EccentricitySquared) * sinLat;

            return (x, y, z);
        }
    }
}
