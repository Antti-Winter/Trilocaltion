using Trilocation.Core;

namespace Trilocation.Interop
{
    /// <summary>
    /// Converts between Maidenhead Grid Locator strings and TriLocation.
    /// Self-contained implementation of the Maidenhead Locator System.
    /// </summary>
    public static class MaidenheadConverter
    {
        /// <summary>Converts a Maidenhead Grid Locator to TriLocation via WGS84 center point.</summary>
        public static TriLocation FromMaidenhead(string grid, int resolution)
        {
            if (string.IsNullOrEmpty(grid) || grid.Length < 2)
            {
                throw new ArgumentException("Grid locator must be at least 2 characters");
            }

            double lon = 0.0;
            double lat = 0.0;
            double lonStep = 0.0;
            double latStep = 0.0;

            // Field (2 uppercase letters A-R): 20° lon, 10° lat
            lon = (char.ToUpper(grid[0]) - 'A') * 20.0;
            lat = (char.ToUpper(grid[1]) - 'A') * 10.0;
            lonStep = 20.0;
            latStep = 10.0;

            // Square (2 digits 0-9): 2° lon, 1° lat
            if (grid.Length >= 4)
            {
                lon = lon + (grid[2] - '0') * 2.0;
                lat = lat + (grid[3] - '0') * 1.0;
                lonStep = 2.0;
                latStep = 1.0;
            }

            // Subsquare (2 lowercase letters a-x): 5' lon, 2.5' lat
            if (grid.Length >= 6)
            {
                lon = lon + (char.ToLower(grid[4]) - 'a') * (2.0 / 24.0);
                lat = lat + (char.ToLower(grid[5]) - 'a') * (1.0 / 24.0);
                lonStep = 2.0 / 24.0;
                latStep = 1.0 / 24.0;
            }

            // Extended square (2 digits 0-9): 30" lon, 15" lat
            if (grid.Length >= 8)
            {
                lon = lon + (grid[6] - '0') * (2.0 / 240.0);
                lat = lat + (grid[7] - '0') * (1.0 / 240.0);
                lonStep = 2.0 / 240.0;
                latStep = 1.0 / 240.0;
            }

            // Center of cell, shift from [0,360) × [0,180) to [-180,180) × [-90,90)
            lon = lon + lonStep / 2.0 - 180.0;
            lat = lat + latStep / 2.0 - 90.0;

            return new TriLocation(lat, lon, resolution);
        }

        /// <summary>Converts a TriLocation to a Maidenhead Grid Locator at the given precision (1-4).</summary>
        public static string ToMaidenhead(TriLocation location, int precision)
        {
            if (precision < 1 || precision > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(precision), "Precision must be 1-4");
            }

            var (lat, lon) = location.ToLatLon();

            // Normalize to [0, 180) lat, [0, 360) lon
            double adjustedLon = lon + 180.0;
            double adjustedLat = lat + 90.0;

            // Clamp
            if (adjustedLon < 0.0) adjustedLon = 0.0;
            if (adjustedLon >= 360.0) adjustedLon = 359.999999;
            if (adjustedLat < 0.0) adjustedLat = 0.0;
            if (adjustedLat >= 180.0) adjustedLat = 179.999999;

            char[] result = new char[precision * 2];

            // Field (A-R): 20° lon, 10° lat
            int lonField = (int)(adjustedLon / 20.0);
            int latField = (int)(adjustedLat / 10.0);
            if (lonField > 17) lonField = 17;
            if (latField > 17) latField = 17;
            result[0] = (char)('A' + lonField);
            result[1] = (char)('A' + latField);

            if (precision >= 2)
            {
                // Square (0-9): 2° lon, 1° lat
                double lonRemainder = adjustedLon - lonField * 20.0;
                double latRemainder = adjustedLat - latField * 10.0;
                int lonSquare = (int)(lonRemainder / 2.0);
                int latSquare = (int)(latRemainder / 1.0);
                if (lonSquare > 9) lonSquare = 9;
                if (latSquare > 9) latSquare = 9;
                result[2] = (char)('0' + lonSquare);
                result[3] = (char)('0' + latSquare);

                if (precision >= 3)
                {
                    // Subsquare (a-x): 5' lon, 2.5' lat (= 2/24° lon, 1/24° lat)
                    double lonRem2 = lonRemainder - lonSquare * 2.0;
                    double latRem2 = latRemainder - latSquare * 1.0;
                    int lonSub = (int)(lonRem2 / (2.0 / 24.0));
                    int latSub = (int)(latRem2 / (1.0 / 24.0));
                    if (lonSub > 23) lonSub = 23;
                    if (latSub > 23) latSub = 23;
                    result[4] = (char)('a' + lonSub);
                    result[5] = (char)('a' + latSub);

                    if (precision >= 4)
                    {
                        // Extended square (0-9): 30" lon, 15" lat (= 2/240° lon, 1/240° lat)
                        double lonRem3 = lonRem2 - lonSub * (2.0 / 24.0);
                        double latRem3 = latRem2 - latSub * (1.0 / 24.0);
                        int lonExt = (int)(lonRem3 / (2.0 / 240.0));
                        int latExt = (int)(latRem3 / (1.0 / 240.0));
                        if (lonExt > 9) lonExt = 9;
                        if (latExt > 9) latExt = 9;
                        result[6] = (char)('0' + lonExt);
                        result[7] = (char)('0' + latExt);
                    }
                }
            }

            return new string(result);
        }
    }
}
