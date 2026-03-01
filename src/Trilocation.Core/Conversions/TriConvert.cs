namespace Trilocation.Core.Conversions
{
    /// <summary>
    /// Coordinate conversion API. All conversions go through WGS84 internally.
    /// </summary>
    public static class TriConvert
    {
        /// <summary>Converts WGS84 lat/lon to TriLocation.</summary>
        public static TriLocation FromWgs84(double latitude, double longitude, int resolution)
        {
            return new TriLocation(latitude, longitude, resolution);
        }

        /// <summary>Converts TriLocation to WGS84 lat/lon (centroid).</summary>
        public static (double Latitude, double Longitude) ToWgs84(TriLocation location)
        {
            return location.ToLatLon();
        }

        /// <summary>Converts Web Mercator (EPSG:3857) coordinates to TriLocation.</summary>
        public static TriLocation FromWebMercator(double x, double y, int resolution)
        {
            var (lat, lon) = WebMercatorProjection.ToWgs84(x, y);
            return new TriLocation(lat, lon, resolution);
        }

        /// <summary>Converts TriLocation to Web Mercator (EPSG:3857) coordinates.</summary>
        public static (double X, double Y) ToWebMercator(TriLocation location)
        {
            var (lat, lon) = location.ToLatLon();
            return WebMercatorProjection.FromWgs84(lat, lon);
        }

        /// <summary>Converts UTM coordinates to TriLocation.</summary>
        public static TriLocation FromUtm(int zone, char band, double easting, double northing, int resolution)
        {
            var (lat, lon) = UtmConverter.ToWgs84(zone, band, easting, northing);
            return new TriLocation(lat, lon, resolution);
        }

        /// <summary>Converts TriLocation to UTM coordinates.</summary>
        public static (int Zone, char Band, double Easting, double Northing) ToUtm(TriLocation location)
        {
            var (lat, lon) = location.ToLatLon();
            return UtmConverter.FromWgs84(lat, lon);
        }

        /// <summary>Converts MGRS string to TriLocation.</summary>
        public static TriLocation FromMgrs(string mgrs, int resolution)
        {
            var (lat, lon) = MgrsConverter.ToWgs84(mgrs);
            return new TriLocation(lat, lon, resolution);
        }

        /// <summary>Converts TriLocation to MGRS string at the given precision (1-5).</summary>
        public static string ToMgrs(TriLocation location, int precision)
        {
            var (lat, lon) = location.ToLatLon();
            return MgrsConverter.FromWgs84(lat, lon, precision);
        }

        /// <summary>Converts ECEF (Earth-Centered, Earth-Fixed) coordinates to TriLocation.</summary>
        public static TriLocation FromEcef(double x, double y, double z, int resolution)
        {
            var (lat, lon) = EcefConverter.ToWgs84(x, y, z);
            return new TriLocation(lat, lon, resolution);
        }

        /// <summary>Converts TriLocation to ECEF coordinates.</summary>
        public static (double X, double Y, double Z) ToEcef(TriLocation location)
        {
            var (lat, lon) = location.ToLatLon();
            return EcefConverter.FromWgs84(lat, lon);
        }
    }
}
