namespace Trilocation.Core.Primitives
{
    /// <summary>
    /// Geographic point (WGS84).
    /// </summary>
    public readonly struct GeoPoint : IEquatable<GeoPoint>
    {
        /// <summary>Latitude in degrees (-90..90).</summary>
        public double Latitude { get; }

        /// <summary>Longitude in degrees (-180..180).</summary>
        public double Longitude { get; }

        /// <summary>Creates a new GeoPoint.</summary>
        public GeoPoint(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>Calculates distance to another point in meters (Haversine formula).</summary>
        public double DistanceTo(GeoPoint other)
        {
            double lat1 = Latitude * GeoConstants.DegreesToRadians;
            double lat2 = other.Latitude * GeoConstants.DegreesToRadians;
            double dLat = (other.Latitude - Latitude) * GeoConstants.DegreesToRadians;
            double dLon = (other.Longitude - Longitude) * GeoConstants.DegreesToRadians;

            double a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0)
                + Math.Cos(lat1) * Math.Cos(lat2)
                * Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);
            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            return GeoConstants.EarthRadiusM * c;
        }

        /// <summary>Calculates the midpoint between two points on a great circle.</summary>
        public GeoPoint MidpointTo(GeoPoint other)
        {
            double lat1 = Latitude * GeoConstants.DegreesToRadians;
            double lon1 = Longitude * GeoConstants.DegreesToRadians;
            double lat2 = other.Latitude * GeoConstants.DegreesToRadians;
            double lon2 = other.Longitude * GeoConstants.DegreesToRadians;

            double dLon = lon2 - lon1;
            double bx = Math.Cos(lat2) * Math.Cos(dLon);
            double by = Math.Cos(lat2) * Math.Sin(dLon);

            double midLat = Math.Atan2(
                Math.Sin(lat1) + Math.Sin(lat2),
                Math.Sqrt((Math.Cos(lat1) + bx) * (Math.Cos(lat1) + bx) + by * by));
            double midLon = lon1 + Math.Atan2(by, Math.Cos(lat1) + bx);

            return new GeoPoint(
                midLat * GeoConstants.RadiansToDegrees,
                midLon * GeoConstants.RadiansToDegrees);
        }

        /// <summary>Equality comparison.</summary>
        public bool Equals(GeoPoint other)
        {
            return Latitude == other.Latitude && Longitude == other.Longitude;
        }

        /// <summary>Equality comparison.</summary>
        public override bool Equals(object? obj)
        {
            return obj is GeoPoint other && Equals(other);
        }

        /// <summary>Returns the hash code.</summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude);
        }

        /// <summary>Returns the string representation.</summary>
        public override string ToString()
        {
            return "(" + Latitude + ", " + Longitude + ")";
        }

        /// <summary>Equality operator.</summary>
        public static bool operator ==(GeoPoint left, GeoPoint right) => left.Equals(right);

        /// <summary>Inequality operator.</summary>
        public static bool operator !=(GeoPoint left, GeoPoint right) => !left.Equals(right);
    }
}
