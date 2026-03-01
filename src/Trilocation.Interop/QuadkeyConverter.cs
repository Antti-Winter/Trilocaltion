using Trilocation.Core;

namespace Trilocation.Interop
{
    /// <summary>
    /// Converts between Bing Maps Quadkeys and TriLocation.
    /// Uses Web Mercator (EPSG:3857) tile projection.
    /// </summary>
    public static class QuadkeyConverter
    {
        /// <summary>Converts a Quadkey string to TriLocation via WGS84 center point.</summary>
        public static TriLocation FromQuadkey(string quadkey, int resolution)
        {
            if (string.IsNullOrEmpty(quadkey))
            {
                throw new ArgumentException("Quadkey must not be empty");
            }

            int level = quadkey.Length;
            int tileX = 0;
            int tileY = 0;

            for (int i = 0; i < level; i++)
            {
                int mask = 1 << (level - 1 - i);
                char digit = quadkey[i];
                if (digit == '1' || digit == '3')
                {
                    tileX = tileX | mask;
                }
                if (digit == '2' || digit == '3')
                {
                    tileY = tileY | mask;
                }
            }

            // Convert tile center to lat/lon
            int mapSize = 1 << level;
            double lon = (tileX + 0.5) / mapSize * 360.0 - 180.0;
            double latRad = Math.Atan(Math.Sinh(Math.PI * (1.0 - 2.0 * (tileY + 0.5) / mapSize)));
            double lat = latRad * 180.0 / Math.PI;

            return new TriLocation(lat, lon, resolution);
        }

        /// <summary>Converts a TriLocation to a Quadkey string at the given level (1-23).</summary>
        public static string ToQuadkey(TriLocation location, int level)
        {
            if (level < 1 || level > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(level), "Level must be 1-23");
            }

            var (lat, lon) = location.ToLatLon();

            // Clamp latitude to Web Mercator range
            double clampedLat = Math.Max(-85.05112878, Math.Min(85.05112878, lat));

            // Convert lat/lon to tile coordinates
            int mapSize = 1 << level;
            double x = (lon + 180.0) / 360.0 * mapSize;
            double sinLat = Math.Sin(clampedLat * Math.PI / 180.0);
            double y = (0.5 - Math.Log((1.0 + sinLat) / (1.0 - sinLat)) / (4.0 * Math.PI)) * mapSize;

            int tileX = Math.Max(0, Math.Min(mapSize - 1, (int)x));
            int tileY = Math.Max(0, Math.Min(mapSize - 1, (int)y));

            // Convert tile XY to quadkey
            char[] result = new char[level];
            for (int i = 0; i < level; i++)
            {
                int mask = 1 << (level - 1 - i);
                int digit = 0;
                if ((tileX & mask) != 0) digit = digit + 1;
                if ((tileY & mask) != 0) digit = digit + 2;
                result[i] = (char)('0' + digit);
            }

            return new string(result);
        }
    }
}
