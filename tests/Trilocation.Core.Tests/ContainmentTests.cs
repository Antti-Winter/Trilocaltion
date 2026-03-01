using Trilocation.Core.Algorithms;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class ContainmentTests
    {
        // === Contains ===

        [Fact]
        public void Contains_ParentChild_ReturnsTrue()
        {
            // 3 contains 17 (direct parent-child)
            TriLocation parent = new TriLocation(3);
            TriLocation child = new TriLocation(17);
            Assert.True(HierarchyNavigator.Contains(parent, child));
        }

        [Fact]
        public void Contains_Grandparent_ReturnsTrue()
        {
            // 3 contains 74 (grandparent: 3 -> 17 -> 74)
            TriLocation grandparent = new TriLocation(3);
            TriLocation grandchild = new TriLocation(74);
            Assert.True(HierarchyNavigator.Contains(grandparent, grandchild));
        }

        [Fact]
        public void Contains_DifferentBranch_ReturnsFalse()
        {
            // 3 does NOT contain 21 (21 is child of 4, not 3)
            TriLocation parent = new TriLocation(3);
            TriLocation other = new TriLocation(21);
            Assert.False(HierarchyNavigator.Contains(parent, other));
        }

        [Fact]
        public void Contains_SameIndex_ReturnsTrue()
        {
            TriLocation loc = new TriLocation(17);
            Assert.True(HierarchyNavigator.Contains(loc, loc));
        }

        [Fact]
        public void Contains_ChildContainsParent_ReturnsFalse()
        {
            TriLocation parent = new TriLocation(3);
            TriLocation child = new TriLocation(17);
            Assert.False(HierarchyNavigator.Contains(child, parent));
        }

        [Fact]
        public void Contains_DifferentBaseFaces_ReturnsFalse()
        {
            TriLocation face1 = new TriLocation(1);
            TriLocation face2child = new TriLocation(13);
            Assert.False(HierarchyNavigator.Contains(face1, face2child));
        }

        [Fact]
        public void Contains_Transitivity()
        {
            // If a contains b and b contains c, then a contains c
            TriLocation a = new TriLocation(3);
            TriLocation b = new TriLocation(17);
            TriLocation c = new TriLocation(74);

            Assert.True(HierarchyNavigator.Contains(a, b));
            Assert.True(HierarchyNavigator.Contains(b, c));
            Assert.True(HierarchyNavigator.Contains(a, c));
        }

        // === GetCommonAncestorLevel ===

        [Fact]
        public void GetCommonAncestorLevel_Siblings_ReturnsParentLevel()
        {
            // 17 and 18 are siblings (children of 3, which is level 0)
            TriLocation a = new TriLocation(17);
            TriLocation b = new TriLocation(18);
            int level = HierarchyNavigator.GetCommonAncestorLevel(a, b);
            Assert.Equal(0, level);
        }

        [Fact]
        public void GetCommonAncestorLevel_ParentChild_ReturnsParentLevel()
        {
            TriLocation parent = new TriLocation(17);
            TriLocation child = new TriLocation(74);
            int level = HierarchyNavigator.GetCommonAncestorLevel(parent, child);
            Assert.Equal(parent.Resolution, level);
        }

        [Fact]
        public void GetCommonAncestorLevel_SameIndex_ReturnsSameLevel()
        {
            TriLocation a = new TriLocation(74);
            int level = HierarchyNavigator.GetCommonAncestorLevel(a, a);
            Assert.Equal(a.Resolution, level);
        }

        [Fact]
        public void GetCommonAncestorLevel_DifferentBaseFaces_ReturnsNegativeOne()
        {
            // Different base faces have no common ancestor
            TriLocation a = new TriLocation(1);
            TriLocation b = new TriLocation(2);
            int level = HierarchyNavigator.GetCommonAncestorLevel(a, b);
            Assert.Equal(-1, level);
        }

        [Fact]
        public void GetCommonAncestorLevel_Cousins()
        {
            // 73 (child of 17, child of 3) and 77 (child of 18, child of 3)
            // Common ancestor is at level 0 (index 3)
            TriLocation a = new TriLocation(73);
            TriLocation b = new TriLocation(77);
            int level = HierarchyNavigator.GetCommonAncestorLevel(a, b);
            Assert.Equal(0, level);
        }

        // === TriLocation instance methods ===

        [Fact]
        public void TriLocation_GetParent_Works()
        {
            TriLocation loc = new TriLocation(74);
            TriLocation parent = loc.GetParent();
            Assert.Equal(17UL, parent.Index);
        }

        [Fact]
        public void TriLocation_GetChildren_Works()
        {
            TriLocation loc = new TriLocation(3);
            TriLocation[] children = loc.GetChildren();
            Assert.Equal(4, children.Length);
            Assert.Equal(17UL, children[0].Index);
        }

        [Fact]
        public void TriLocation_GetAncestor_Works()
        {
            TriLocation loc = new TriLocation(74);
            TriLocation ancestor = loc.GetAncestor(0);
            Assert.Equal(3UL, ancestor.Index);
        }

        [Fact]
        public void TriLocation_Contains_Works()
        {
            TriLocation parent = new TriLocation(3);
            TriLocation child = new TriLocation(17);
            Assert.True(parent.Contains(child));
        }

        [Fact]
        public void TriLocation_GetCommonAncestorLevel_Works()
        {
            TriLocation a = new TriLocation(17);
            TriLocation b = new TriLocation(18);
            Assert.Equal(0, a.GetCommonAncestorLevel(b));
        }
    }
}
