namespace Trilocation.Core.Primitives
{
    /// <summary>
    /// Geospatial constants.
    /// </summary>
    public static class GeoConstants
    {
        /// <summary>Mean radius of the Earth in kilometers.</summary>
        public const double EarthRadiusKm = 6371.0;

        /// <summary>Mean radius of the Earth in meters.</summary>
        public const double EarthRadiusM = 6_371_000.0;

        /// <summary>Surface area of the Earth in square kilometers.</summary>
        public const double EarthSurfaceAreaKm2 = 510_065_623.0;

        /// <summary>Conversion factor from degrees to radians.</summary>
        public const double DegreesToRadians = Math.PI / 180.0;

        /// <summary>Conversion factor from radians to degrees.</summary>
        public const double RadiansToDegrees = 180.0 / Math.PI;

        /// <summary>Minimum latitude value.</summary>
        public const double MinLatitude = -90.0;

        /// <summary>Maximum latitude value.</summary>
        public const double MaxLatitude = 90.0;

        /// <summary>Minimum longitude value.</summary>
        public const double MinLongitude = -180.0;

        /// <summary>Maximum longitude value.</summary>
        public const double MaxLongitude = 180.0;
    }
}
