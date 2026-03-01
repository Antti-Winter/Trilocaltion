using System.Globalization;
using System.Text.Json;
using Xunit;
using Trilocation.Core;
using Trilocation.Core.Indexing;

namespace Trilocation.Spec.Tests
{
    public class SpecComplianceTests
    {
        private readonly JsonDocument _testVectors;
        private readonly JsonDocument _resolutionTable;

        public SpecComplianceTests()
        {
            string vectorsPath = Path.Combine(AppContext.BaseDirectory, "test-vectors.json");
            string tablePath = Path.Combine(AppContext.BaseDirectory, "resolution-table.json");
            _testVectors = JsonDocument.Parse(File.ReadAllText(vectorsPath));
            _resolutionTable = JsonDocument.Parse(File.ReadAllText(tablePath));
        }

        // ===== Resolution Table Tests =====

        [Fact]
        public void ResolutionTable_Has31Levels()
        {
            var levels = _resolutionTable.RootElement.GetProperty("levels");
            Assert.Equal(31, levels.GetArrayLength());
        }

        [Fact]
        public void ResolutionTable_MaxResolution_Is30()
        {
            int maxRes = _resolutionTable.RootElement.GetProperty("max_resolution").GetInt32();
            Assert.Equal(30, maxRes);
        }

        [Fact]
        public void ResolutionTable_AllLevels_MatchImplementation()
        {
            var levels = _resolutionTable.RootElement.GetProperty("levels");
            foreach (var level in levels.EnumerateArray())
            {
                int levelNum = level.GetProperty("level").GetInt32();
                ulong expectedStart = ulong.Parse(
                    level.GetProperty("level_start").GetString()!, CultureInfo.InvariantCulture);
                ulong expectedEnd = ulong.Parse(
                    level.GetProperty("level_end").GetString()!, CultureInfo.InvariantCulture);
                ulong expectedCount = ulong.Parse(
                    level.GetProperty("triangle_count").GetString()!, CultureInfo.InvariantCulture);
                ulong expectedCumulative = ulong.Parse(
                    level.GetProperty("cumulative_count").GetString()!, CultureInfo.InvariantCulture);

                Assert.Equal(expectedStart, CumulativeIndex.LevelStart(levelNum));
                Assert.Equal(expectedEnd, CumulativeIndex.LevelEnd(levelNum));
                Assert.Equal(expectedCount, CumulativeIndex.LevelEnd(levelNum) - CumulativeIndex.LevelStart(levelNum) + 1);
                Assert.Equal(expectedCumulative, CumulativeIndex.CumulativeCount(levelNum));
            }
        }

        // ===== Level Boundaries Tests =====

        [Fact]
        public void LevelBoundaries_AllMatch()
        {
            var boundaries = _testVectors.RootElement.GetProperty("level_boundaries");
            int count = 0;
            foreach (var boundary in boundaries.EnumerateArray())
            {
                int level = boundary.GetProperty("level").GetInt32();
                ulong expectedStart = ulong.Parse(
                    boundary.GetProperty("start").GetString()!, CultureInfo.InvariantCulture);
                ulong expectedEnd = ulong.Parse(
                    boundary.GetProperty("end").GetString()!, CultureInfo.InvariantCulture);
                ulong expectedCumulative = ulong.Parse(
                    boundary.GetProperty("cumulative").GetString()!, CultureInfo.InvariantCulture);

                Assert.Equal(expectedStart, CumulativeIndex.LevelStart(level));
                Assert.Equal(expectedEnd, CumulativeIndex.LevelEnd(level));
                Assert.Equal(expectedCumulative, CumulativeIndex.CumulativeCount(level));
                count++;
            }
            Assert.Equal(31, count);
        }

        // ===== Parent-Child Tests =====

        [Fact]
        public void ParentChild_AllPairsMatch()
        {
            var pairs = _testVectors.RootElement.GetProperty("parent_child");
            int count = 0;
            foreach (var pair in pairs.EnumerateArray())
            {
                ulong index = ulong.Parse(
                    pair.GetProperty("index").GetString()!, CultureInfo.InvariantCulture);
                int expectedRes = pair.GetProperty("resolution").GetInt32();
                int expectedBaseFace = pair.GetProperty("base_face").GetInt32();

                Assert.Equal(expectedRes, CumulativeIndex.GetResolution(index));
                Assert.Equal(expectedBaseFace, CumulativeIndex.GetBaseFace(index));

                // Check parent if present
                if (pair.TryGetProperty("parent", out var parentProp))
                {
                    ulong expectedParent = ulong.Parse(
                        parentProp.GetString()!, CultureInfo.InvariantCulture);
                    Assert.Equal(expectedParent, CumulativeIndex.GetParent(index));
                }

                // Check children if present
                if (pair.TryGetProperty("children", out var childrenProp))
                {
                    ulong[] expectedChildren = childrenProp.EnumerateArray()
                        .Select(c => ulong.Parse(c.GetString()!, CultureInfo.InvariantCulture))
                        .ToArray();
                    ulong[] actualChildren = CumulativeIndex.GetChildren(index);
                    Assert.Equal(expectedChildren.Length, actualChildren.Length);
                    for (int i = 0; i < expectedChildren.Length; i++)
                    {
                        Assert.Equal(expectedChildren[i], actualChildren[i]);
                    }
                }

                count++;
            }
            Assert.True(count >= 48, "Expected at least 48 parent-child pairs, got " + count);
        }

        // ===== Base Faces Tests =====

        [Fact]
        public void BaseFaces_All8Match()
        {
            var faces = _testVectors.RootElement.GetProperty("base_faces");
            int count = 0;
            foreach (var face in faces.EnumerateArray())
            {
                int faceNum = face.GetProperty("face").GetInt32();
                ulong expectedIndex = ulong.Parse(
                    face.GetProperty("index").GetString()!, CultureInfo.InvariantCulture);
                double expectedCentroidLat = double.Parse(
                    face.GetProperty("centroid_lat").GetString()!, CultureInfo.InvariantCulture);
                double expectedCentroidLon = double.Parse(
                    face.GetProperty("centroid_lon").GetString()!, CultureInfo.InvariantCulture);

                var location = new TriLocation(expectedIndex);
                Assert.Equal(0, location.Resolution);
                Assert.Equal(faceNum, location.BaseFace);

                var (lat, lon) = location.ToLatLon();
                Assert.Equal(expectedCentroidLat, Math.Round(lat, 10));
                Assert.Equal(expectedCentroidLon, Math.Round(lon, 10));

                // Verify vertex coordinates
                var cell = location.ToCell();
                double expectedVALat = double.Parse(
                    face.GetProperty("vertex_a_lat").GetString()!, CultureInfo.InvariantCulture);
                double expectedVALon = double.Parse(
                    face.GetProperty("vertex_a_lon").GetString()!, CultureInfo.InvariantCulture);
                Assert.Equal(expectedVALat, Math.Round(cell.VertexA.Latitude, 10));
                Assert.Equal(expectedVALon, Math.Round(cell.VertexA.Longitude, 10));

                count++;
            }
            Assert.Equal(8, count);
        }

        // ===== Coordinate Conversion Tests =====

        [Fact]
        public void CoordinateConversions_AllMatch()
        {
            var conversions = _testVectors.RootElement.GetProperty("coordinate_conversions");
            int count = 0;
            foreach (var conv in conversions.EnumerateArray())
            {
                double inputLat = double.Parse(
                    conv.GetProperty("input_lat").GetString()!, CultureInfo.InvariantCulture);
                double inputLon = double.Parse(
                    conv.GetProperty("input_lon").GetString()!, CultureInfo.InvariantCulture);
                int resolution = conv.GetProperty("resolution").GetInt32();
                ulong expectedIndex = ulong.Parse(
                    conv.GetProperty("expected_index").GetString()!, CultureInfo.InvariantCulture);
                int expectedBaseFace = conv.GetProperty("base_face").GetInt32();

                var location = new TriLocation(inputLat, inputLon, resolution);
                Assert.Equal(expectedIndex, location.Index);
                Assert.Equal(expectedBaseFace, location.BaseFace);
                Assert.Equal(resolution, location.Resolution);

                // Verify centroid round-trip
                double expectedCentroidLat = double.Parse(
                    conv.GetProperty("centroid_lat").GetString()!, CultureInfo.InvariantCulture);
                double expectedCentroidLon = double.Parse(
                    conv.GetProperty("centroid_lon").GetString()!, CultureInfo.InvariantCulture);
                var (backLat, backLon) = location.ToLatLon();
                Assert.Equal(expectedCentroidLat, Math.Round(backLat, 10));
                Assert.Equal(expectedCentroidLon, Math.Round(backLon, 10));

                count++;
            }
            Assert.True(count >= 100, "Expected at least 100 coordinate conversions, got " + count);
        }

        // ===== Neighbor Tests =====

        [Fact]
        public void Neighbors_AllMatch()
        {
            var neighborTests = _testVectors.RootElement.GetProperty("neighbors");
            int count = 0;
            foreach (var test in neighborTests.EnumerateArray())
            {
                ulong centerIndex = ulong.Parse(
                    test.GetProperty("center_index").GetString()!, CultureInfo.InvariantCulture);
                var center = new TriLocation(centerIndex);

                if (test.TryGetProperty("distance", out var distProp))
                {
                    // GetNeighborsWithin test
                    int distance = distProp.GetInt32();
                    int expectedCount = test.GetProperty("result_count").GetInt32();
                    var expectedIndices = test.GetProperty("result_indices").EnumerateArray()
                        .Select(i => ulong.Parse(i.GetString()!, CultureInfo.InvariantCulture))
                        .OrderBy(i => i)
                        .ToArray();

                    var actual = center.GetNeighborsWithin(distance);
                    Assert.Equal(expectedCount, actual.Length);

                    var actualSorted = actual.Select(n => n.Index).OrderBy(i => i).ToArray();
                    Assert.Equal(expectedIndices.Length, actualSorted.Length);
                    for (int i = 0; i < expectedIndices.Length; i++)
                    {
                        Assert.Equal(expectedIndices[i], actualSorted[i]);
                    }
                }
                else
                {
                    // GetNeighbors test (3 edge neighbors)
                    int expectedCount = test.GetProperty("neighbor_count").GetInt32();
                    var expectedIndices = test.GetProperty("neighbor_indices").EnumerateArray()
                        .Select(i => ulong.Parse(i.GetString()!, CultureInfo.InvariantCulture))
                        .OrderBy(i => i)
                        .ToArray();

                    var actual = center.GetNeighbors();
                    Assert.Equal(expectedCount, actual.Length);

                    var actualSorted = actual.Select(n => n.Index).OrderBy(i => i).ToArray();
                    Assert.Equal(expectedIndices.Length, actualSorted.Length);
                    for (int i = 0; i < expectedIndices.Length; i++)
                    {
                        Assert.Equal(expectedIndices[i], actualSorted[i]);
                    }
                }

                count++;
            }
            Assert.True(count >= 18, "Expected at least 18 neighbor tests, got " + count);
        }

        // ===== Containment Tests =====

        [Fact]
        public void Containment_AllMatch()
        {
            var containTests = _testVectors.RootElement.GetProperty("containment");
            int count = 0;
            foreach (var test in containTests.EnumerateArray())
            {
                ulong parentIndex = ulong.Parse(
                    test.GetProperty("parent_index").GetString()!, CultureInfo.InvariantCulture);
                ulong childIndex = ulong.Parse(
                    test.GetProperty("child_index").GetString()!, CultureInfo.InvariantCulture);
                bool expectedContains = test.GetProperty("expected_contains").GetBoolean();

                var parent = new TriLocation(parentIndex);
                var child = new TriLocation(childIndex);
                bool actualContains = parent.Contains(child);

                Assert.Equal(expectedContains, actualContains);
                count++;
            }
            Assert.True(count >= 10, "Expected at least 10 containment tests, got " + count);
        }

        // ===== Statistics Tests =====

        [Fact]
        public void TestVectors_HaveExpectedCounts()
        {
            var stats = _testVectors.RootElement.GetProperty("statistics");
            Assert.Equal(31, stats.GetProperty("level_boundaries").GetInt32());
            Assert.Equal(48, stats.GetProperty("parent_child").GetInt32());
            Assert.Equal(8, stats.GetProperty("base_faces").GetInt32());
            Assert.True(stats.GetProperty("coordinate_conversions").GetInt32() >= 100);
            Assert.True(stats.GetProperty("neighbors").GetInt32() >= 18);
            Assert.True(stats.GetProperty("containment").GetInt32() >= 10);
            Assert.True(stats.GetProperty("total").GetInt32() >= 100);
        }

        [Fact]
        public void TestVectors_Version_Is1()
        {
            string version = _testVectors.RootElement.GetProperty("version").GetString()!;
            Assert.Equal("1.0", version);
        }
    }
}
