using Trilocation.Core;

namespace Trilocation.Interop
{
    /// <summary>
    /// Unified facade for all geospatial system interop conversions.
    /// All conversions go through WGS84 center-point mapping.
    /// </summary>
    public static class TriInterop
    {
        // === H3 (Uber) ===

        /// <summary>Converts an H3 index to TriLocation.</summary>
        public static TriLocation FromH3(ulong h3Index, int resolution)
            => H3Converter.FromH3(h3Index, resolution);

        /// <summary>Converts a TriLocation to an H3 index.</summary>
        public static ulong ToH3(TriLocation location, int h3Resolution)
            => H3Converter.ToH3(location, h3Resolution);

        // === S2 (Google) ===

        /// <summary>Converts an S2 Cell ID to TriLocation.</summary>
        public static TriLocation FromS2CellId(ulong s2CellId, int resolution)
            => S2Converter.FromS2CellId(s2CellId, resolution);

        /// <summary>Converts a TriLocation to an S2 Cell ID.</summary>
        public static ulong ToS2CellId(TriLocation location, int s2Level)
            => S2Converter.ToS2CellId(location, s2Level);

        // === Geohash ===

        /// <summary>Converts a Geohash string to TriLocation.</summary>
        public static TriLocation FromGeohash(string geohash, int resolution)
            => GeohashConverter.FromGeohash(geohash, resolution);

        /// <summary>Converts a TriLocation to a Geohash string.</summary>
        public static string ToGeohash(TriLocation location, int precision)
            => GeohashConverter.ToGeohash(location, precision);

        // === Plus Codes ===

        /// <summary>Converts a Plus Code to TriLocation.</summary>
        public static TriLocation FromPlusCode(string plusCode, int resolution)
            => PlusCodeConverter.FromPlusCode(plusCode, resolution);

        /// <summary>Converts a TriLocation to a Plus Code.</summary>
        public static string ToPlusCode(TriLocation location, int length)
            => PlusCodeConverter.ToPlusCode(location, length);

        // === Quadkey (Bing Maps) ===

        /// <summary>Converts a Quadkey to TriLocation.</summary>
        public static TriLocation FromQuadkey(string quadkey, int resolution)
            => QuadkeyConverter.FromQuadkey(quadkey, resolution);

        /// <summary>Converts a TriLocation to a Quadkey.</summary>
        public static string ToQuadkey(TriLocation location, int level)
            => QuadkeyConverter.ToQuadkey(location, level);

        // === Maidenhead Grid ===

        /// <summary>Converts a Maidenhead Grid Locator to TriLocation.</summary>
        public static TriLocation FromMaidenhead(string grid, int resolution)
            => MaidenheadConverter.FromMaidenhead(grid, resolution);

        /// <summary>Converts a TriLocation to a Maidenhead Grid Locator.</summary>
        public static string ToMaidenhead(TriLocation location, int precision)
            => MaidenheadConverter.ToMaidenhead(location, precision);
    }
}
