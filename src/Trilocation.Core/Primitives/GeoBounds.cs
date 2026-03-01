namespace Trilocation.Core.Primitives
{
    /// <summary>
    /// Bounding box for geographic points.
    /// </summary>
    public readonly struct GeoBounds : IEquatable<GeoBounds>
    {
        /// <summary>Minimum latitude.</summary>
        public double MinLatitude { get; }

        /// <summary>Maximum latitude.</summary>
        public double MaxLatitude { get; }

        /// <summary>Minimum longitude.</summary>
        public double MinLongitude { get; }

        /// <summary>Maximum longitude.</summary>
        public double MaxLongitude { get; }

        /// <summary>Creates a new bounding box.</summary>
        public GeoBounds(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude)
        {
            MinLatitude = minLatitude;
            MaxLatitude = maxLatitude;
            MinLongitude = minLongitude;
            MaxLongitude = maxLongitude;
        }

        /// <summary>Checks whether the bounding box contains a point.</summary>
        public bool Contains(GeoPoint point)
        {
            return point.Latitude >= MinLatitude
                && point.Latitude <= MaxLatitude
                && point.Longitude >= MinLongitude
                && point.Longitude <= MaxLongitude;
        }

        /// <summary>Checks whether two bounding boxes intersect.</summary>
        public bool Intersects(GeoBounds other)
        {
            return MinLatitude <= other.MaxLatitude
                && MaxLatitude >= other.MinLatitude
                && MinLongitude <= other.MaxLongitude
                && MaxLongitude >= other.MinLongitude;
        }

        /// <summary>Equality comparison.</summary>
        public bool Equals(GeoBounds other)
        {
            return MinLatitude == other.MinLatitude
                && MaxLatitude == other.MaxLatitude
                && MinLongitude == other.MinLongitude
                && MaxLongitude == other.MaxLongitude;
        }

        /// <summary>Equality comparison.</summary>
        public override bool Equals(object? obj)
        {
            return obj is GeoBounds other && Equals(other);
        }

        /// <summary>Returns the hash code.</summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(MinLatitude, MaxLatitude, MinLongitude, MaxLongitude);
        }

        /// <summary>Equality operator.</summary>
        public static bool operator ==(GeoBounds left, GeoBounds right) => left.Equals(right);

        /// <summary>Inequality operator.</summary>
        public static bool operator !=(GeoBounds left, GeoBounds right) => !left.Equals(right);
    }
}
