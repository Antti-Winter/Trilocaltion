using System.Globalization;

namespace Trilocation.Core.Conversions
{
    /// <summary>
    /// MGRS (Military Grid Reference System) coordinate conversions.
    /// Based on UTM with 100km grid square identifiers.
    /// </summary>
    internal static class MgrsConverter
    {
        // Column letters repeat every 3 zones: set 1 (zones 1,4,7,...), set 2 (zones 2,5,8,...), set 3 (zones 3,6,9,...)
        private static readonly string ColumnLettersSet1 = "ABCDEFGH";
        private static readonly string ColumnLettersSet2 = "JKLMNPQR";
        private static readonly string ColumnLettersSet3 = "STUVWXYZ";

        // Row letters repeat every 2M northing
        private static readonly string RowLettersOdd = "ABCDEFGHJKLMNPQRSTUV";
        private static readonly string RowLettersEven = "FGHJKLMNPQRSTUVABCDE";

        /// <summary>Parses MGRS string to WGS84 lat/lon (center of grid square at given precision).</summary>
        public static (double Latitude, double Longitude) ToWgs84(string mgrs)
        {
            mgrs = mgrs.Replace(" ", "");

            // Parse zone number (1-2 digits)
            int zoneEnd = 0;
            while (zoneEnd < mgrs.Length && char.IsDigit(mgrs[zoneEnd]))
            {
                zoneEnd++;
            }
            int zone = int.Parse(mgrs.Substring(0, zoneEnd), CultureInfo.InvariantCulture);

            // Parse band letter
            char band = mgrs[zoneEnd];
            int pos = zoneEnd + 1;

            // Parse grid square (2 letters)
            char colLetter = mgrs[pos];
            char rowLetter = mgrs[pos + 1];
            pos += 2;

            // Parse easting/northing digits (remaining characters, split evenly)
            string digits = mgrs.Substring(pos);
            int precision = digits.Length / 2;
            double easting;
            double northing;

            if (precision == 0)
            {
                easting = 0.0;
                northing = 0.0;
            }
            else
            {
                string eastStr = digits.Substring(0, precision);
                string northStr = digits.Substring(precision);
                easting = double.Parse(eastStr, CultureInfo.InvariantCulture);
                northing = double.Parse(northStr, CultureInfo.InvariantCulture);

                // Scale to meters based on precision
                double scale = Math.Pow(10, 5 - precision);
                easting = easting * scale;
                northing = northing * scale;
            }

            // Convert grid square to easting/northing offset
            double colOffset = GetColumnOffset(zone, colLetter);
            double rowOffset = GetRowOffset(zone, rowLetter, band);

            double utmEasting = colOffset + easting;
            double utmNorthing = rowOffset + northing;

            return UtmConverter.ToWgs84(zone, band, utmEasting, utmNorthing);
        }

        /// <summary>Converts WGS84 lat/lon to MGRS string at the given precision (1-5).</summary>
        public static string FromWgs84(double latitude, double longitude, int precision)
        {
            if (precision < 1 || precision > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(precision), "Precision must be 1-5");
            }

            var (zone, band, easting, northing) = UtmConverter.FromWgs84(latitude, longitude);

            // Get column and row letters
            char colLetter = GetColumnLetter(zone, easting);
            char rowLetter = GetRowLetter(zone, northing);

            // Get easting/northing within the 100km square
            double e100k = easting % 100000.0;
            double n100k = northing % 100000.0;

            // Format digits at given precision
            double scale = Math.Pow(10, 5 - precision);
            int eDigits = (int)Math.Floor(e100k / scale);
            int nDigits = (int)Math.Floor(n100k / scale);

            string format = "D" + precision;
            return zone.ToString(CultureInfo.InvariantCulture)
                + band
                + colLetter
                + rowLetter
                + eDigits.ToString(format, CultureInfo.InvariantCulture)
                + nDigits.ToString(format, CultureInfo.InvariantCulture);
        }

        private static char GetColumnLetter(int zone, double easting)
        {
            int setIndex = ((zone - 1) % 3);
            string letters;
            if (setIndex == 0) letters = ColumnLettersSet1;
            else if (setIndex == 1) letters = ColumnLettersSet2;
            else letters = ColumnLettersSet3;

            int col = (int)Math.Floor(easting / 100000.0) - 1;
            if (col < 0) col = 0;
            if (col >= letters.Length) col = letters.Length - 1;
            return letters[col];
        }

        private static char GetRowLetter(int zone, double northing)
        {
            string letters = (zone % 2 != 0) ? RowLettersOdd : RowLettersEven;
            int row = (int)Math.Floor(northing / 100000.0) % 20;
            if (row < 0) row = row + 20;
            return letters[row];
        }

        private static double GetColumnOffset(int zone, char colLetter)
        {
            int setIndex = ((zone - 1) % 3);
            string letters;
            if (setIndex == 0) letters = ColumnLettersSet1;
            else if (setIndex == 1) letters = ColumnLettersSet2;
            else letters = ColumnLettersSet3;

            int index = letters.IndexOf(char.ToUpper(colLetter));
            if (index < 0)
            {
                throw new ArgumentException("Invalid MGRS column letter: " + colLetter);
            }
            return (index + 1) * 100000.0;
        }

        private static double GetRowOffset(int zone, char rowLetter, char band)
        {
            string letters = (zone % 2 != 0) ? RowLettersOdd : RowLettersEven;
            int index = letters.IndexOf(char.ToUpper(rowLetter));
            if (index < 0)
            {
                throw new ArgumentException("Invalid MGRS row letter: " + rowLetter);
            }

            double baseNorthing = index * 100000.0;

            // Determine the northing origin based on band
            // Bands N and above are northern hemisphere (northing from equator)
            // Bands below N are southern hemisphere (northing from 10,000,000)
            bool isNorth = char.ToUpper(band) >= 'N';
            double bandMinNorthing = GetBandMinNorthing(band);

            // Find the correct 2,000,000m cycle that matches the band
            double northing = baseNorthing;
            while (northing < bandMinNorthing - 100000.0)
            {
                northing = northing + 2000000.0;
            }

            return northing;
        }

        private static double GetBandMinNorthing(char band)
        {
            char upper = char.ToUpper(band);
            // Approximate northing at band bottom using UTM formula
            // Bands: C=-80, D=-72, E=-64, ..., N=0, P=8, ..., X=72
            string bandLetters = "CDEFGHJKLMNPQRSTUVWX";
            int index = bandLetters.IndexOf(upper);
            if (index < 0) return 0.0;

            double latitude = -80.0 + index * 8.0;
            if (latitude >= 0.0)
            {
                // Northern hemisphere: directly compute meridional arc
                var (_, _, _, northing) = UtmConverter.FromWgs84(latitude, 3.0);
                return northing;
            }
            else
            {
                // Southern hemisphere: use false northing
                var (_, _, _, northing) = UtmConverter.FromWgs84(latitude, 3.0);
                return northing;
            }
        }
    }
}
