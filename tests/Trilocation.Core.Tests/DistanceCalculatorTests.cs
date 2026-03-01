using Xunit;
using Trilocation.Core.Algorithms;

namespace Trilocation.Core.Tests
{
    public class DistanceCalculatorTests
    {
        // === GetDistance ===

        [Fact]
        public void GetDistance_SameTriangle_ReturnsZero()
        {
            TriLocation loc = new TriLocation(9);
            double distance = DistanceCalculator.GetDistance(loc, loc);

            Assert.Equal(0.0, distance);
        }

        [Fact]
        public void GetDistance_AdjacentTriangles_IsPositive()
        {
            TriLocation loc = new TriLocation(9);
            TriLocation neighbor = loc.GetNeighbors()[0];
            double distance = DistanceCalculator.GetDistance(loc, neighbor);

            Assert.True(distance > 0);
        }

        [Fact]
        public void GetDistance_Symmetric()
        {
            TriLocation a = new TriLocation(9);
            TriLocation b = new TriLocation(10);
            double distAB = DistanceCalculator.GetDistance(a, b);
            double distBA = DistanceCalculator.GetDistance(b, a);

            Assert.Equal(distAB, distBA, 1);
        }

        [Fact]
        public void GetDistance_DifferentBaseFaces_LargeDistance()
        {
            // Face 0 (northern) vs Face 4 (southern, same quadrant) - should be a large distance
            TriLocation north = new TriLocation(1);
            TriLocation south = new TriLocation(5);
            double distance = DistanceCalculator.GetDistance(north, south);

            // Two opposite-hemisphere face centroids should be thousands of km apart
            Assert.True(distance > 5000000); // > 5000 km
        }

        // === GetGridDistance ===

        [Fact]
        public void GetGridDistance_SameTriangle_ReturnsZero()
        {
            TriLocation loc = new TriLocation(9);
            int distance = DistanceCalculator.GetGridDistance(loc, loc);

            Assert.Equal(0, distance);
        }

        [Fact]
        public void GetGridDistance_AdjacentTriangles_ReturnsOne()
        {
            TriLocation loc = new TriLocation(12); // center child of face 0
            TriLocation neighbor = NeighborFinder.GetNeighbors(loc)[0];
            int distance = DistanceCalculator.GetGridDistance(loc, neighbor);

            Assert.Equal(1, distance);
        }

        [Fact]
        public void GetGridDistance_TwoStepsAway_ReturnsTwo()
        {
            TriLocation center = new TriLocation(12);
            TriLocation[] ring1 = NeighborFinder.GetRing(center, 1);
            TriLocation[] ring2 = NeighborFinder.GetRing(center, 2);

            // Pick a ring2 triangle
            if (ring2.Length > 0)
            {
                int distance = DistanceCalculator.GetGridDistance(center, ring2[0]);
                Assert.Equal(2, distance);
            }
        }

        [Fact]
        public void GetGridDistance_DifferentResolutions_Throws()
        {
            TriLocation a = new TriLocation(9);  // resolution 1
            TriLocation b = new TriLocation(41); // resolution 2

            Assert.Throws<ArgumentException>(() => DistanceCalculator.GetGridDistance(a, b));
        }

        [Fact]
        public void GetGridDistance_Symmetric()
        {
            TriLocation a = new TriLocation(9);
            TriLocation b = new TriLocation(10);
            int distAB = DistanceCalculator.GetGridDistance(a, b);
            int distBA = DistanceCalculator.GetGridDistance(b, a);

            Assert.Equal(distAB, distBA);
        }
    }
}
