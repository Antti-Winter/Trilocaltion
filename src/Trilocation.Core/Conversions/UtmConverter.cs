using Trilocation.Core.Primitives;

namespace Trilocation.Core.Conversions
{
    /// <summary>
    /// UTM (Universal Transverse Mercator) coordinate conversions.
    /// Uses series expansion formulas with WGS84 ellipsoid.
    /// </summary>
    internal static class UtmConverter
    {
        // WGS84 ellipsoid
        private const double A = 6378137.0;
        private const double F = 1.0 / 298.257223563;
        private static readonly double E2 = 2.0 * F - F * F;
        private static readonly double EPrime2 = E2 / (1.0 - E2);
        private const double K0 = 0.9996;
        private const double FalseEasting = 500000.0;
        private const double FalseNorthingSouth = 10000000.0;

        // UTM band letters C-X (omitting I and O)
        private static readonly string BandLetters = "CDEFGHJKLMNPQRSTUVWX";

        /// <summary>Converts UTM coordinates to WGS84 lat/lon.</summary>
        public static (double Latitude, double Longitude) ToWgs84(
            int zone, char band, double easting, double northing)
        {
            bool isSouth = char.ToUpper(band) < 'N';
            if (isSouth)
            {
                northing = northing - FalseNorthingSouth;
            }

            double x = easting - FalseEasting;
            double y = northing;

            double m = y / K0;
            double mu = m / (A * (1.0 - E2 / 4.0 - 3.0 * E2 * E2 / 64.0
                - 5.0 * E2 * E2 * E2 / 256.0));

            double e1 = (1.0 - Math.Sqrt(1.0 - E2)) / (1.0 + Math.Sqrt(1.0 - E2));
            double phi1 = mu
                + (3.0 * e1 / 2.0 - 27.0 * e1 * e1 * e1 / 32.0) * Math.Sin(2.0 * mu)
                + (21.0 * e1 * e1 / 16.0 - 55.0 * e1 * e1 * e1 * e1 / 32.0) * Math.Sin(4.0 * mu)
                + (151.0 * e1 * e1 * e1 / 96.0) * Math.Sin(6.0 * mu);

            double sinPhi1 = Math.Sin(phi1);
            double cosPhi1 = Math.Cos(phi1);
            double tanPhi1 = Math.Tan(phi1);

            double n1 = A / Math.Sqrt(1.0 - E2 * sinPhi1 * sinPhi1);
            double t1 = tanPhi1 * tanPhi1;
            double c1 = EPrime2 * cosPhi1 * cosPhi1;
            double r1 = A * (1.0 - E2) / Math.Pow(1.0 - E2 * sinPhi1 * sinPhi1, 1.5);
            double d = x / (n1 * K0);

            double lat = phi1
                - (n1 * tanPhi1 / r1) * (
                    d * d / 2.0
                    - (5.0 + 3.0 * t1 + 10.0 * c1 - 4.0 * c1 * c1 - 9.0 * EPrime2)
                        * d * d * d * d / 24.0
                    + (61.0 + 90.0 * t1 + 298.0 * c1 + 45.0 * t1 * t1
                        - 252.0 * EPrime2 - 3.0 * c1 * c1)
                        * d * d * d * d * d * d / 720.0);

            double lonOrigin = ((zone - 1) * 6 - 180 + 3) * GeoConstants.DegreesToRadians;
            double lon = lonOrigin + (
                d
                - (1.0 + 2.0 * t1 + c1) * d * d * d / 6.0
                + (5.0 - 2.0 * c1 + 28.0 * t1 - 3.0 * c1 * c1
                    + 8.0 * EPrime2 + 24.0 * t1 * t1)
                    * d * d * d * d * d / 120.0)
                / cosPhi1;

            return (lat * GeoConstants.RadiansToDegrees, lon * GeoConstants.RadiansToDegrees);
        }

        /// <summary>Converts WGS84 lat/lon to UTM coordinates.</summary>
        public static (int Zone, char Band, double Easting, double Northing) FromWgs84(
            double latitude, double longitude)
        {
            int zone = (int)Math.Floor((longitude + 180.0) / 6.0) + 1;
            char band = GetBand(latitude);

            double latRad = latitude * GeoConstants.DegreesToRadians;
            double lonRad = longitude * GeoConstants.DegreesToRadians;

            double lonOrigin = ((zone - 1) * 6 - 180 + 3) * GeoConstants.DegreesToRadians;
            double dLon = lonRad - lonOrigin;

            double sinLat = Math.Sin(latRad);
            double cosLat = Math.Cos(latRad);
            double tanLat = Math.Tan(latRad);

            double n = A / Math.Sqrt(1.0 - E2 * sinLat * sinLat);
            double t = tanLat * tanLat;
            double c = EPrime2 * cosLat * cosLat;

            double mCalc = A * (
                (1.0 - E2 / 4.0 - 3.0 * E2 * E2 / 64.0 - 5.0 * E2 * E2 * E2 / 256.0) * latRad
                - (3.0 * E2 / 8.0 + 3.0 * E2 * E2 / 32.0 + 45.0 * E2 * E2 * E2 / 1024.0)
                    * Math.Sin(2.0 * latRad)
                + (15.0 * E2 * E2 / 256.0 + 45.0 * E2 * E2 * E2 / 1024.0)
                    * Math.Sin(4.0 * latRad)
                - (35.0 * E2 * E2 * E2 / 3072.0) * Math.Sin(6.0 * latRad));

            double easting = FalseEasting + K0 * n * (
                dLon * cosLat
                + (1.0 - t + c) * dLon * dLon * dLon * cosLat * cosLat * cosLat / 6.0
                + (5.0 - 18.0 * t + t * t + 72.0 * c - 58.0 * EPrime2)
                    * dLon * dLon * dLon * dLon * dLon
                    * cosLat * cosLat * cosLat * cosLat * cosLat / 120.0);

            double northing = K0 * (mCalc + n * tanLat * (
                dLon * dLon * cosLat * cosLat / 2.0
                + (5.0 - t + 9.0 * c + 4.0 * c * c)
                    * dLon * dLon * dLon * dLon
                    * cosLat * cosLat * cosLat * cosLat / 24.0
                + (61.0 - 58.0 * t + t * t + 600.0 * c - 330.0 * EPrime2)
                    * dLon * dLon * dLon * dLon * dLon * dLon
                    * cosLat * cosLat * cosLat * cosLat * cosLat * cosLat / 720.0));

            if (latitude < 0.0)
            {
                northing = northing + FalseNorthingSouth;
            }

            return (zone, band, easting, northing);
        }

        private static char GetBand(double latitude)
        {
            if (latitude < -80.0) return 'C';
            if (latitude >= 84.0) return 'X';

            // Bands C-W cover 8-degree intervals from -80 to 72
            // Band X covers 72 to 84 (12 degrees)
            int index = (int)Math.Floor((latitude + 80.0) / 8.0);
            if (index < 0) index = 0;
            if (index >= BandLetters.Length) index = BandLetters.Length - 1;
            return BandLetters[index];
        }
    }
}
