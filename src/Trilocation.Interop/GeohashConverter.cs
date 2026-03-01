using Trilocation.Core;

namespace Trilocation.Interop
{
    /// <summary>
    /// Converts between Geohash strings and TriLocation.
    /// Self-contained implementation with no external dependencies.
    /// Uses base32 encoding with alternating longitude/latitude bit interleaving.
    /// </summary>
    public static class GeohashConverter
    {
        private const string Base32Alphabet = "0123456789bcdefghjkmnpqrstuvwxyz";

        /// <summary>Converts a Geohash string to TriLocation via WGS84 center point.</summary>
        public static TriLocation FromGeohash(string geohash, int resolution)
        {
            double latMin = -90.0;
            double latMax = 90.0;
            double lonMin = -180.0;
            double lonMax = 180.0;
            bool isLon = true;

            for (int i = 0; i < geohash.Length; i++)
            {
                int charIndex = Base32Alphabet.IndexOf(char.ToLower(geohash[i]));
                if (charIndex < 0)
                {
                    throw new ArgumentException("Invalid geohash character: " + geohash[i]);
                }

                for (int bit = 4; bit >= 0; bit--)
                {
                    int bitValue = (charIndex >> bit) & 1;
                    if (isLon)
                    {
                        double mid = (lonMin + lonMax) / 2.0;
                        if (bitValue == 1)
                        {
                            lonMin = mid;
                        }
                        else
                        {
                            lonMax = mid;
                        }
                    }
                    else
                    {
                        double mid = (latMin + latMax) / 2.0;
                        if (bitValue == 1)
                        {
                            latMin = mid;
                        }
                        else
                        {
                            latMax = mid;
                        }
                    }
                    isLon = !isLon;
                }
            }

            double lat = (latMin + latMax) / 2.0;
            double lon = (lonMin + lonMax) / 2.0;
            return new TriLocation(lat, lon, resolution);
        }

        /// <summary>Converts a TriLocation to a Geohash string at the given precision (1-12).</summary>
        public static string ToGeohash(TriLocation location, int precision)
        {
            if (precision < 1 || precision > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(precision), "Precision must be 1-12");
            }

            var (lat, lon) = location.ToLatLon();

            double latMin = -90.0;
            double latMax = 90.0;
            double lonMin = -180.0;
            double lonMax = 180.0;
            bool isLon = true;

            char[] result = new char[precision];
            int charIndex = 0;
            int bit = 4;

            for (int i = 0; i < precision * 5; i++)
            {
                if (isLon)
                {
                    double mid = (lonMin + lonMax) / 2.0;
                    if (lon >= mid)
                    {
                        charIndex = charIndex | (1 << bit);
                        lonMin = mid;
                    }
                    else
                    {
                        lonMax = mid;
                    }
                }
                else
                {
                    double mid = (latMin + latMax) / 2.0;
                    if (lat >= mid)
                    {
                        charIndex = charIndex | (1 << bit);
                        latMin = mid;
                    }
                    else
                    {
                        latMax = mid;
                    }
                }

                isLon = !isLon;
                bit--;

                if (bit < 0)
                {
                    result[i / 5] = Base32Alphabet[charIndex];
                    charIndex = 0;
                    bit = 4;
                }
            }

            return new string(result);
        }
    }
}
