using Trilocation.Core.Algorithms;
using Trilocation.Core.Indexing;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class HierarchyNavigatorTests
    {
        // === GetParent ===

        [Fact]
        public void GetParent_Index74_Returns17()
        {
            // From projectplan: 74 -> 17
            TriLocation loc = new TriLocation(74);
            TriLocation parent = HierarchyNavigator.GetParent(loc);
            Assert.Equal(17UL, parent.Index);
        }

        [Fact]
        public void GetParent_Index17_Returns3()
        {
            // From projectplan: 17 -> 3
            TriLocation loc = new TriLocation(17);
            TriLocation parent = HierarchyNavigator.GetParent(loc);
            Assert.Equal(3UL, parent.Index);
        }

        [Fact]
        public void GetParent_Chain_74_17_3()
        {
            TriLocation loc74 = new TriLocation(74);
            TriLocation loc17 = HierarchyNavigator.GetParent(loc74);
            TriLocation loc3 = HierarchyNavigator.GetParent(loc17);

            Assert.Equal(17UL, loc17.Index);
            Assert.Equal(3UL, loc3.Index);
        }

        [Fact]
        public void GetParent_Level0_ThrowsInvalidOperation()
        {
            TriLocation loc = new TriLocation(3);
            Assert.Throws<InvalidOperationException>(() =>
                HierarchyNavigator.GetParent(loc));
        }

        [Fact]
        public void GetParent_ResolutionDecreases()
        {
            TriLocation loc = new TriLocation(74);
            TriLocation parent = HierarchyNavigator.GetParent(loc);
            Assert.Equal(loc.Resolution - 1, parent.Resolution);
        }

        // === GetChildren ===

        [Fact]
        public void GetChildren_Index3_Returns17_18_19_20()
        {
            // From projectplan: 3 -> [17,18,19,20]
            TriLocation loc = new TriLocation(3);
            TriLocation[] children = HierarchyNavigator.GetChildren(loc);

            Assert.Equal(4, children.Length);
            Assert.Equal(17UL, children[0].Index);
            Assert.Equal(18UL, children[1].Index);
            Assert.Equal(19UL, children[2].Index);
            Assert.Equal(20UL, children[3].Index);
        }

        [Fact]
        public void GetChildren_Index17_Returns73_74_75_76()
        {
            // From projectplan: 17 -> [73,74,75,76]
            TriLocation loc = new TriLocation(17);
            TriLocation[] children = HierarchyNavigator.GetChildren(loc);

            Assert.Equal(4, children.Length);
            Assert.Equal(73UL, children[0].Index);
            Assert.Equal(74UL, children[1].Index);
            Assert.Equal(75UL, children[2].Index);
            Assert.Equal(76UL, children[3].Index);
        }

        [Fact]
        public void GetChildren_ReturnsFourChildren()
        {
            TriLocation loc = new TriLocation(1);
            TriLocation[] children = HierarchyNavigator.GetChildren(loc);
            Assert.Equal(4, children.Length);
        }

        [Fact]
        public void GetChildren_ResolutionIncreases()
        {
            TriLocation loc = new TriLocation(3);
            TriLocation[] children = HierarchyNavigator.GetChildren(loc);
            for (int i = 0; i < children.Length; i++)
            {
                Assert.Equal(loc.Resolution + 1, children[i].Resolution);
            }
        }

        [Fact]
        public void GetChildren_MaxResolution_ThrowsInvalidOperation()
        {
            // Create a location at max resolution using known index
            ulong maxLevelStart = IndexConstants.LevelStartTable[IndexConstants.MaxResolution];
            TriLocation loc = new TriLocation(maxLevelStart);
            Assert.Throws<InvalidOperationException>(() =>
                HierarchyNavigator.GetChildren(loc));
        }

        // === GetParent/GetChildren round-trip ===

        [Theory]
        [InlineData(1UL)]
        [InlineData(3UL)]
        [InlineData(8UL)]
        [InlineData(17UL)]
        [InlineData(74UL)]
        [InlineData(169UL)]
        public void RoundTrip_GetParent_GetChildren(ulong index)
        {
            TriLocation loc = new TriLocation(index);
            if (loc.Resolution >= IndexConstants.MaxResolution)
            {
                return;
            }

            TriLocation[] children = HierarchyNavigator.GetChildren(loc);
            for (int i = 0; i < 4; i++)
            {
                TriLocation parent = HierarchyNavigator.GetParent(children[i]);
                Assert.Equal(loc.Index, parent.Index);
            }
        }

        // === GetAncestor ===

        [Fact]
        public void GetAncestor_74_Resolution0_Returns3()
        {
            TriLocation loc = new TriLocation(74);
            TriLocation ancestor = HierarchyNavigator.GetAncestor(loc, 0);
            Assert.Equal(3UL, ancestor.Index);
        }

        [Fact]
        public void GetAncestor_74_Resolution1_Returns17()
        {
            TriLocation loc = new TriLocation(74);
            TriLocation ancestor = HierarchyNavigator.GetAncestor(loc, 1);
            Assert.Equal(17UL, ancestor.Index);
        }

        [Fact]
        public void GetAncestor_SameResolution_ReturnsSelf()
        {
            TriLocation loc = new TriLocation(74);
            TriLocation ancestor = HierarchyNavigator.GetAncestor(loc, loc.Resolution);
            Assert.Equal(loc.Index, ancestor.Index);
        }

        [Fact]
        public void GetAncestor_InvalidResolution_Negative_Throws()
        {
            TriLocation loc = new TriLocation(74);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                HierarchyNavigator.GetAncestor(loc, -1));
        }

        [Fact]
        public void GetAncestor_InvalidResolution_TooHigh_Throws()
        {
            TriLocation loc = new TriLocation(74);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                HierarchyNavigator.GetAncestor(loc, loc.Resolution + 1));
        }

        // === GetDescendants ===

        [Fact]
        public void GetDescendants_Depth1_ReturnsFour()
        {
            TriLocation loc = new TriLocation(3);
            TriLocation[] descendants = HierarchyNavigator.GetDescendants(loc, 1);
            Assert.Equal(4, descendants.Length);
        }

        [Fact]
        public void GetDescendants_Depth2_ReturnsTwenty()
        {
            // depth 2 = 4 + 16 = 20
            TriLocation loc = new TriLocation(3);
            TriLocation[] descendants = HierarchyNavigator.GetDescendants(loc, 2);
            Assert.Equal(20, descendants.Length);
        }

        [Fact]
        public void GetDescendants_Depth3_ReturnsEightyFour()
        {
            // depth 3 = 4 + 16 + 64 = 84
            TriLocation loc = new TriLocation(3);
            TriLocation[] descendants = HierarchyNavigator.GetDescendants(loc, 3);
            Assert.Equal(84, descendants.Length);
        }

        [Fact]
        public void GetDescendants_Depth0_ReturnsEmpty()
        {
            TriLocation loc = new TriLocation(3);
            TriLocation[] descendants = HierarchyNavigator.GetDescendants(loc, 0);
            Assert.Empty(descendants);
        }

        [Fact]
        public void GetDescendants_AllAreValidIndices()
        {
            TriLocation loc = new TriLocation(3);
            TriLocation[] descendants = HierarchyNavigator.GetDescendants(loc, 2);
            for (int i = 0; i < descendants.Length; i++)
            {
                Assert.True(IndexValidator.IsValid(descendants[i].Index));
            }
        }

        [Fact]
        public void GetDescendants_Depth1_MatchesGetChildren()
        {
            TriLocation loc = new TriLocation(3);
            TriLocation[] descendants = HierarchyNavigator.GetDescendants(loc, 1);
            TriLocation[] children = HierarchyNavigator.GetChildren(loc);

            Assert.Equal(children.Length, descendants.Length);
            for (int i = 0; i < children.Length; i++)
            {
                Assert.Equal(children[i].Index, descendants[i].Index);
            }
        }
    }
}
