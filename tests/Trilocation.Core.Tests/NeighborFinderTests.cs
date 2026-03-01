using Xunit;
using Trilocation.Core.Algorithms;

namespace Trilocation.Core.Tests
{
    public class NeighborFinderTests
    {
        // === Level 0: base face neighbors ===

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void GetNeighbors_Level0_ReturnsThreeNeighbors(ulong index)
        {
            TriLocation loc = new TriLocation(index);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(loc);

            Assert.Equal(3, neighbors.Length);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void GetNeighbors_Level0_NeighborsAreLevel0(ulong index)
        {
            TriLocation loc = new TriLocation(index);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(loc);

            Assert.All(neighbors, n => Assert.Equal(0, n.Resolution));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void GetNeighbors_Level0_NoneIsOriginal(ulong index)
        {
            TriLocation loc = new TriLocation(index);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(loc);

            Assert.All(neighbors, n => Assert.NotEqual(loc.Index, n.Index));
        }

        [Fact]
        public void GetNeighbors_Level0_Face0_NeighborsMatchFaceAdjacency()
        {
            // Face 0 (index 1): neighbors are Face 3 (4), Face 4 (5), Face 1 (2)
            TriLocation loc = new TriLocation(1);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(loc);

            var neighborIndices = neighbors.Select(n => n.Index).OrderBy(i => i).ToArray();
            Assert.Equal(new ulong[] { 2, 4, 5 }, neighborIndices);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void GetNeighbors_Level0_Symmetry(ulong index)
        {
            TriLocation loc = new TriLocation(index);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(loc);

            foreach (var neighbor in neighbors)
            {
                TriLocation[] reverseNeighbors = NeighborFinder.GetNeighbors(neighbor);
                Assert.Contains(loc.Index, reverseNeighbors.Select(n => n.Index));
            }
        }

        // === Level 1: subdivision neighbors ===

        [Theory]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(17)]
        [InlineData(18)]
        [InlineData(19)]
        [InlineData(20)]
        public void GetNeighbors_Level1_ReturnsThreeNeighbors(ulong index)
        {
            TriLocation loc = new TriLocation(index);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(loc);

            Assert.Equal(3, neighbors.Length);
        }

        [Theory]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        public void GetNeighbors_Level1_SameResolution(ulong index)
        {
            TriLocation loc = new TriLocation(index);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(loc);

            Assert.All(neighbors, n => Assert.Equal(1, n.Resolution));
        }

        [Fact]
        public void GetNeighbors_Level1_CenterChild_NeighborsAreSiblings()
        {
            // Child 3 of face 0 (index 12) has neighbors: child 0 (9), child 1 (10), child 2 (11)
            TriLocation center = new TriLocation(12);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(center);

            var neighborIndices = neighbors.Select(n => n.Index).OrderBy(i => i).ToArray();
            Assert.Equal(new ulong[] { 9, 10, 11 }, neighborIndices);
        }

        [Fact]
        public void GetNeighbors_Level1_CornerChild_HasCrossFaceNeighbor()
        {
            // Child 0 of face 0 (index 9): apex triangle
            // One neighbor is the center child (12), and two cross face boundaries
            TriLocation apex = new TriLocation(9);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(apex);

            Assert.Equal(3, neighbors.Length);
            // One of the neighbors should be the center sibling (12)
            Assert.Contains(12UL, neighbors.Select(n => n.Index));
            // At least one neighbor should be from a different base face
            Assert.True(
                neighbors.Any(n => n.BaseFace != apex.BaseFace),
                "Apex child should have at least one cross-face neighbor");
        }

        [Theory]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        [InlineData(14)]
        [InlineData(15)]
        [InlineData(16)]
        public void GetNeighbors_Level1_Symmetry(ulong index)
        {
            TriLocation loc = new TriLocation(index);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(loc);

            foreach (var neighbor in neighbors)
            {
                TriLocation[] reverseNeighbors = NeighborFinder.GetNeighbors(neighbor);
                Assert.Contains(loc.Index, reverseNeighbors.Select(n => n.Index));
            }
        }

        // === Level 2: deeper hierarchy ===

        [Theory]
        [InlineData(41)]
        [InlineData(42)]
        [InlineData(43)]
        [InlineData(44)]
        [InlineData(73)]
        [InlineData(74)]
        [InlineData(75)]
        [InlineData(76)]
        public void GetNeighbors_Level2_ReturnsThreeNeighbors(ulong index)
        {
            TriLocation loc = new TriLocation(index);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(loc);

            Assert.Equal(3, neighbors.Length);
            Assert.All(neighbors, n => Assert.Equal(2, n.Resolution));
        }

        [Theory]
        [InlineData(41)]
        [InlineData(42)]
        [InlineData(43)]
        [InlineData(44)]
        [InlineData(73)]
        [InlineData(74)]
        public void GetNeighbors_Level2_Symmetry(ulong index)
        {
            TriLocation loc = new TriLocation(index);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(loc);

            foreach (var neighbor in neighbors)
            {
                TriLocation[] reverseNeighbors = NeighborFinder.GetNeighbors(neighbor);
                Assert.Contains(loc.Index, reverseNeighbors.Select(n => n.Index));
            }
        }

        // === All neighbors distinct ===

        [Theory]
        [InlineData(1)]
        [InlineData(9)]
        [InlineData(12)]
        [InlineData(41)]
        [InlineData(73)]
        public void GetNeighbors_AllDistinct(ulong index)
        {
            TriLocation loc = new TriLocation(index);
            TriLocation[] neighbors = NeighborFinder.GetNeighbors(loc);

            Assert.Equal(neighbors.Length, neighbors.Select(n => n.Index).Distinct().Count());
        }

        // === TriLocation instance methods ===

        [Fact]
        public void TriLocation_GetNeighbors_DelegatesToNeighborFinder()
        {
            TriLocation loc = new TriLocation(9);
            TriLocation[] instanceNeighbors = loc.GetNeighbors();
            TriLocation[] staticNeighbors = NeighborFinder.GetNeighbors(loc);

            Assert.Equal(
                staticNeighbors.Select(n => n.Index).OrderBy(i => i),
                instanceNeighbors.Select(n => n.Index).OrderBy(i => i));
        }

        [Fact]
        public void TriLocation_GetNeighborsWithin_DelegatesToNeighborFinder()
        {
            TriLocation loc = new TriLocation(9);
            TriLocation[] instanceResult = loc.GetNeighborsWithin(1);
            TriLocation[] staticResult = NeighborFinder.GetNeighborsWithin(loc, 1);

            Assert.Equal(
                staticResult.Select(n => n.Index).OrderBy(i => i),
                instanceResult.Select(n => n.Index).OrderBy(i => i));
        }
    }
}
