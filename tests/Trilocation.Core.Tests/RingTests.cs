using Xunit;
using Trilocation.Core.Algorithms;

namespace Trilocation.Core.Tests
{
    public class RingTests
    {
        // === GetRing ===

        [Fact]
        public void GetRing_Radius1_ReturnsThreeNeighbors()
        {
            TriLocation center = new TriLocation(1);
            TriLocation[] ring = NeighborFinder.GetRing(center, 1);

            Assert.Equal(3, ring.Length);
        }

        [Fact]
        public void GetRing_Radius1_MatchesGetNeighbors()
        {
            TriLocation center = new TriLocation(9);
            TriLocation[] ring = NeighborFinder.GetRing(center, 1);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(center);

            Assert.Equal(
                neighbors.Select(n => n.Index).OrderBy(i => i),
                ring.Select(n => n.Index).OrderBy(i => i));
        }

        [Fact]
        public void GetRing_Radius2_ReturnsMoreThanRing1()
        {
            TriLocation center = new TriLocation(12);
            TriLocation[] ring1 = NeighborFinder.GetRing(center, 1);
            TriLocation[] ring2 = NeighborFinder.GetRing(center, 2);

            Assert.True(ring2.Length > ring1.Length);
        }

        [Fact]
        public void GetRing_Radius2_DoesNotContainCenter()
        {
            TriLocation center = new TriLocation(12);
            TriLocation[] ring2 = NeighborFinder.GetRing(center, 2);

            Assert.DoesNotContain(center.Index, ring2.Select(n => n.Index));
        }

        [Fact]
        public void GetRing_Radius2_DoesNotContainRing1()
        {
            TriLocation center = new TriLocation(12);
            TriLocation[] ring1 = NeighborFinder.GetRing(center, 1);
            TriLocation[] ring2 = NeighborFinder.GetRing(center, 2);

            var ring1Indices = new HashSet<ulong>(ring1.Select(n => n.Index));
            Assert.All(ring2, r => Assert.DoesNotContain(r.Index, ring1Indices));
        }

        [Fact]
        public void GetRing_Radius2_NoDuplicates()
        {
            TriLocation center = new TriLocation(12);
            TriLocation[] ring2 = NeighborFinder.GetRing(center, 2);

            Assert.Equal(ring2.Length, ring2.Select(n => n.Index).Distinct().Count());
        }

        [Fact]
        public void GetRing_Radius2_AllSameResolution()
        {
            TriLocation center = new TriLocation(12);
            TriLocation[] ring2 = NeighborFinder.GetRing(center, 2);

            Assert.All(ring2, r => Assert.Equal(center.Resolution, r.Resolution));
        }

        // === GetNeighborsWithin ===

        [Fact]
        public void GetNeighborsWithin_Distance0_ReturnsOnlyCenter()
        {
            TriLocation center = new TriLocation(9);
            TriLocation[] result = NeighborFinder.GetNeighborsWithin(center, 0);

            Assert.Single(result);
            Assert.Equal(center.Index, result[0].Index);
        }

        [Fact]
        public void GetNeighborsWithin_Distance1_ReturnsFour()
        {
            TriLocation center = new TriLocation(9);
            TriLocation[] result = NeighborFinder.GetNeighborsWithin(center, 1);

            // Center + 3 neighbors = 4
            Assert.Equal(4, result.Length);
            Assert.Contains(center.Index, result.Select(n => n.Index));
        }

        [Fact]
        public void GetNeighborsWithin_Distance1_ContainsCenter()
        {
            TriLocation center = new TriLocation(12);
            TriLocation[] result = NeighborFinder.GetNeighborsWithin(center, 1);

            Assert.Contains(center.Index, result.Select(n => n.Index));
        }

        [Fact]
        public void GetNeighborsWithin_Distance2_ContainsAllPreviousRings()
        {
            TriLocation center = new TriLocation(12);
            TriLocation[] within1 = NeighborFinder.GetNeighborsWithin(center, 1);
            TriLocation[] within2 = NeighborFinder.GetNeighborsWithin(center, 2);

            var within2Indices = new HashSet<ulong>(within2.Select(n => n.Index));
            Assert.All(within1, w => Assert.Contains(w.Index, within2Indices));
        }

        [Fact]
        public void GetNeighborsWithin_NoDuplicates()
        {
            TriLocation center = new TriLocation(12);
            TriLocation[] result = NeighborFinder.GetNeighborsWithin(center, 2);

            Assert.Equal(result.Length, result.Select(n => n.Index).Distinct().Count());
        }

        [Fact]
        public void GetNeighborsWithin_AllSameResolution()
        {
            TriLocation center = new TriLocation(12);
            TriLocation[] result = NeighborFinder.GetNeighborsWithin(center, 2);

            Assert.All(result, r => Assert.Equal(center.Resolution, r.Resolution));
        }
    }
}
