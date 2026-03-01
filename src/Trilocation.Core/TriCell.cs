using Trilocation.Core.Algorithms;
using Trilocation.Core.Geometry;
using Trilocation.Core.Primitives;

namespace Trilocation.Core
{
    /// <summary>
    /// Geometric triangle cell on the map with vertices, centroid, and area.
    /// </summary>
    public readonly struct TriCell
    {
        private readonly Triangle3D _triangle;

        /// <summary>The TriLocation this cell represents.</summary>
        public TriLocation Location { get; }

        /// <summary>First vertex in lat/lon.</summary>
        public GeoPoint VertexA { get; }

        /// <summary>Second vertex in lat/lon.</summary>
        public GeoPoint VertexB { get; }

        /// <summary>Third vertex in lat/lon.</summary>
        public GeoPoint VertexC { get; }

        /// <summary>Centroid of the triangle in lat/lon.</summary>
        public GeoPoint Centroid { get; }

        /// <summary>Area of the triangle in square kilometers.</summary>
        public double AreaKm2 { get; }

        /// <summary>Creates a TriCell from a TriLocation and its Triangle3D geometry.</summary>
        internal TriCell(TriLocation location, Triangle3D triangle)
        {
            _triangle = triangle;
            Location = location;

            var (latA, lonA) = triangle.A.ToLatLon();
            var (latB, lonB) = triangle.B.ToLatLon();
            var (latC, lonC) = triangle.C.ToLatLon();
            var (latCen, lonCen) = triangle.Centroid.ToLatLon();

            VertexA = new GeoPoint(latA, lonA);
            VertexB = new GeoPoint(latB, lonB);
            VertexC = new GeoPoint(latC, lonC);
            Centroid = new GeoPoint(latCen, lonCen);

            // Area on unit sphere (steradians) -> km2
            double areaSterad = triangle.Area();
            AreaKm2 = areaSterad * GeoConstants.EarthRadiusKm * GeoConstants.EarthRadiusKm;
        }

        /// <summary>Checks whether a lat/lon point is inside this cell.</summary>
        public bool Contains(double latitude, double longitude)
        {
            Vector3D point = Vector3D.FromLatLon(latitude, longitude);
            return _triangle.Contains(point);
        }

        /// <summary>Returns the bounding box of this cell.</summary>
        public GeoBounds GetBounds()
        {
            double minLat = Math.Min(VertexA.Latitude, Math.Min(VertexB.Latitude, VertexC.Latitude));
            double maxLat = Math.Max(VertexA.Latitude, Math.Max(VertexB.Latitude, VertexC.Latitude));
            double minLon = Math.Min(VertexA.Longitude, Math.Min(VertexB.Longitude, VertexC.Longitude));
            double maxLon = Math.Max(VertexA.Longitude, Math.Max(VertexB.Longitude, VertexC.Longitude));

            return new GeoBounds(minLat, maxLat, minLon, maxLon);
        }
    }
}
