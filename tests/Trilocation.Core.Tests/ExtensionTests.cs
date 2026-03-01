using Xunit;
using Trilocation.Core.Extensions;
using Trilocation.Core.Primitives;
using Trilocation.Core.Indexing;

namespace Trilocation.Core.Tests
{
    public class ExtensionTests
    {
        // === IsAtResolution ===

        [Fact]
        public void IsAtResolution_Correct_ReturnsTrue()
        {
            TriLocation loc = new TriLocation(9);
            Assert.True(loc.IsAtResolution(1));
        }

        [Fact]
        public void IsAtResolution_Wrong_ReturnsFalse()
        {
            TriLocation loc = new TriLocation(9);
            Assert.False(loc.IsAtResolution(2));
        }

        // === IsBaseFace ===

        [Fact]
        public void IsBaseFace_Level0_ReturnsTrue()
        {
            TriLocation loc = new TriLocation(1);
            Assert.True(loc.IsBaseFace());
        }

        [Fact]
        public void IsBaseFace_Level1_ReturnsFalse()
        {
            TriLocation loc = new TriLocation(9);
            Assert.False(loc.IsBaseFace());
        }

        // === GetSiblings ===

        [Fact]
        public void GetSiblings_Level1_ReturnsThreeSiblings()
        {
            TriLocation loc = new TriLocation(9); // child of face 0
            TriLocation[] siblings = loc.GetSiblings();

            Assert.Equal(3, siblings.Length);
            Assert.DoesNotContain(loc.Index, siblings.Select(s => s.Index));
            // Siblings should be 10, 11, 12
            var siblingIndices = siblings.Select(s => s.Index).OrderBy(i => i).ToArray();
            Assert.Equal(new ulong[] { 10, 11, 12 }, siblingIndices);
        }

        [Fact]
        public void GetSiblings_Level0_ThrowsInvalidOperation()
        {
            TriLocation loc = new TriLocation(1);
            Assert.Throws<InvalidOperationException>(() => loc.GetSiblings());
        }

        // === ToGeoJson ===

        [Fact]
        public void ToGeoJson_ContainsFeatureType()
        {
            TriLocation loc = new TriLocation(9);
            string json = loc.ToGeoJson();

            Assert.Contains("\"type\"", json);
            Assert.Contains("\"Feature\"", json);
        }

        [Fact]
        public void ToGeoJson_ContainsPolygonGeometry()
        {
            TriLocation loc = new TriLocation(9);
            string json = loc.ToGeoJson();

            Assert.Contains("\"Polygon\"", json);
            Assert.Contains("\"coordinates\"", json);
        }

        [Fact]
        public void ToGeoJson_ContainsProperties()
        {
            TriLocation loc = new TriLocation(9);
            string json = loc.ToGeoJson();

            Assert.Contains("\"index\"", json);
            Assert.Contains("\"resolution\"", json);
        }

        // === GetBounds ===

        [Fact]
        public void GetBounds_MultipleLocations_ReturnsBounds()
        {
            TriLocation[] locs = new TriLocation[]
            {
                new TriLocation(1),
                new TriLocation(2),
                new TriLocation(3)
            };

            GeoBounds bounds = locs.GetBounds();

            Assert.True(bounds.MaxLatitude >= bounds.MinLatitude);
            Assert.True(bounds.MaxLongitude >= bounds.MinLongitude);
        }

        [Fact]
        public void GetBounds_SingleLocation_MatchesCellBounds()
        {
            TriLocation loc = new TriLocation(9);
            GeoBounds bounds = new TriLocation[] { loc }.GetBounds();
            GeoBounds cellBounds = loc.ToCell().GetBounds();

            Assert.Equal(cellBounds.MinLatitude, bounds.MinLatitude, 6);
            Assert.Equal(cellBounds.MaxLatitude, bounds.MaxLatitude, 6);
        }

        // === GroupByParent ===

        [Fact]
        public void GroupByParent_SiblingsGroupTogether()
        {
            TriLocation[] siblings = new TriLocation[]
            {
                new TriLocation(9),
                new TriLocation(10),
                new TriLocation(11),
                new TriLocation(12)
            };

            var groups = siblings.GroupByParent().ToArray();

            Assert.Single(groups);
            Assert.Equal(4, groups[0].Count());
            Assert.Equal(1UL, groups[0].Key.Index); // parent is face 0 (index 1)
        }

        // === Compact ===

        [Fact]
        public void Compact_FourSiblings_ReturnsParent()
        {
            TriLocation[] siblings = new TriLocation[]
            {
                new TriLocation(9),
                new TriLocation(10),
                new TriLocation(11),
                new TriLocation(12)
            };

            TriLocation[] compacted = siblings.Compact().ToArray();

            Assert.Single(compacted);
            Assert.Equal(1UL, compacted[0].Index); // parent is face 0
        }

        [Fact]
        public void Compact_PartialSiblings_NoCompaction()
        {
            TriLocation[] partial = new TriLocation[]
            {
                new TriLocation(9),
                new TriLocation(10),
                new TriLocation(11)
                // Missing 12 — cannot compact
            };

            TriLocation[] compacted = partial.Compact().ToArray();

            Assert.Equal(3, compacted.Length);
        }

        [Fact]
        public void Compact_MixedResolutions_OnlyCompactsSameLevel()
        {
            TriLocation[] locs = new TriLocation[]
            {
                new TriLocation(9),
                new TriLocation(10),
                new TriLocation(11),
                new TriLocation(12),
                new TriLocation(2) // level 0, different resolution
            };

            TriLocation[] compacted = locs.Compact().ToArray();

            // 9-12 compact to 1, plus 2 remains = 2 locations
            Assert.Equal(2, compacted.Length);
        }
    }
}
