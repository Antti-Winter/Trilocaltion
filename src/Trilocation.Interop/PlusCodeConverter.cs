using Trilocation.Core;

namespace Trilocation.Interop
{
    /// <summary>
    /// Converts between Plus Codes (Open Location Codes) and TriLocation.
    /// Self-contained implementation of Google's Open Location Code algorithm.
    /// </summary>
    public static class PlusCodeConverter
    {
        private const string Alphabet = "23456789CFGHJMPQRVWX";
        private const int SeparatorPosition = 8;
        private const char Separator = '+';
        private const double LatitudeMax = 90.0;
        private const double LongitudeMax = 180.0;

        // Pair resolutions: each pair divides the cell into 20x20
        // First pair: 20° lat, 20° lon
        // Second pair: 1° lat, 1° lon
        // Third pair: 0.05° lat, 0.05° lon
        // Fourth pair: 0.0025° lat, 0.0025° lon
        // Fifth pair: 0.000125° lat, 0.000125° lon
        private static readonly double[] PairLatSteps = { 20.0, 1.0, 0.05, 0.0025, 0.000125 };
        private static readonly double[] PairLonSteps = { 20.0, 1.0, 0.05, 0.0025, 0.000125 };

        /// <summary>Converts a Plus Code string to TriLocation via WGS84 center point.</summary>
        public static TriLocation FromPlusCode(string plusCode, int resolution)
        {
            string code = plusCode.Replace(Separator.ToString(), "");

            double lat = 0.0;
            double lon = 0.0;
            double latStep = 0.0;
            double lonStep = 0.0;

            int pairCount = Math.Min(code.Length / 2, 5);
            for (int i = 0; i < pairCount; i++)
            {
                latStep = PairLatSteps[i];
                lonStep = PairLonSteps[i];

                int latIndex = Alphabet.IndexOf(code[i * 2]);
                int lonIndex = Alphabet.IndexOf(code[i * 2 + 1]);

                if (latIndex < 0 || lonIndex < 0)
                {
                    throw new ArgumentException("Invalid Plus Code character in: " + plusCode);
                }

                lat = lat + latIndex * latStep;
                lon = lon + lonIndex * lonStep;
            }

            // Center of the cell
            lat = lat + latStep / 2.0 - LatitudeMax;
            lon = lon + lonStep / 2.0 - LongitudeMax;

            return new TriLocation(lat, lon, resolution);
        }

        /// <summary>Converts a TriLocation to a Plus Code string at the given length (2-15, even).</summary>
        public static string ToPlusCode(TriLocation location, int length)
        {
            if (length < 2 || length > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be 2-15");
            }

            var (lat, lon) = location.ToLatLon();

            // Normalize to positive ranges
            double adjustedLat = lat + LatitudeMax;
            double adjustedLon = lon + LongitudeMax;

            // Clamp latitude
            if (adjustedLat < 0.0) adjustedLat = 0.0;
            if (adjustedLat >= LatitudeMax * 2.0) adjustedLat = LatitudeMax * 2.0 - 0.0000000001;

            // Normalize longitude
            while (adjustedLon < 0.0) adjustedLon = adjustedLon + 360.0;
            while (adjustedLon >= 360.0) adjustedLon = adjustedLon - 360.0;

            char[] result = new char[length + 1]; // +1 for separator
            int pos = 0;

            int pairCount = Math.Min(length / 2, 5);
            for (int i = 0; i < pairCount; i++)
            {
                double latStep = PairLatSteps[i];
                double lonStep = PairLonSteps[i];

                int latIndex = Math.Min((int)(adjustedLat / latStep), 19);
                int lonIndex = Math.Min((int)(adjustedLon / lonStep), 19);

                result[pos] = Alphabet[latIndex];
                pos++;
                result[pos] = Alphabet[lonIndex];
                pos++;

                adjustedLat = adjustedLat - latIndex * latStep;
                adjustedLon = adjustedLon - lonIndex * lonStep;

                if (pos == SeparatorPosition)
                {
                    result[pos] = Separator;
                    pos++;
                }
            }

            // If we haven't placed the separator yet (short codes)
            if (pos <= SeparatorPosition)
            {
                // Pad with zeros if needed
                while (pos < SeparatorPosition)
                {
                    result[pos] = '0';
                    pos++;
                }
                result[pos] = Separator;
                pos++;
            }

            return new string(result, 0, pos);
        }
    }
}
