using Trilocation.Core.Indexing;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class CumulativeIndexTests
    {
        // === CumulativeCount ===

        [Theory]
        [InlineData(0, 8UL)]
        [InlineData(1, 40UL)]
        [InlineData(2, 168UL)]
        [InlineData(3, 680UL)]
        [InlineData(4, 2_728UL)]
        [InlineData(5, 10_920UL)]
        public void CumulativeCount_KnownLevels_ReturnsCorrectValue(int resolution, ulong expected)
        {
            ulong result = CumulativeIndex.CumulativeCount(resolution);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CumulativeCount_Level30_NoOverflow()
        {
            // S(30) = 12,297,829,382,473,034,408
            ulong result = CumulativeIndex.CumulativeCount(30);
            Assert.Equal(12_297_829_382_473_034_408UL, result);
        }

        [Fact]
        public void CumulativeCount_Level30_FitsInUlong()
        {
            ulong result = CumulativeIndex.CumulativeCount(30);
            Assert.True(result < ulong.MaxValue);
        }

        // === LevelStart / LevelEnd ===

        [Theory]
        [InlineData(0, 1UL)]
        [InlineData(1, 9UL)]
        [InlineData(2, 41UL)]
        [InlineData(3, 169UL)]
        [InlineData(4, 681UL)]
        [InlineData(5, 2_729UL)]
        public void LevelStart_KnownLevels_ReturnsCorrectValue(int resolution, ulong expected)
        {
            Assert.Equal(expected, CumulativeIndex.LevelStart(resolution));
        }

        [Theory]
        [InlineData(0, 8UL)]
        [InlineData(1, 40UL)]
        [InlineData(2, 168UL)]
        [InlineData(3, 680UL)]
        [InlineData(4, 2_728UL)]
        [InlineData(5, 10_920UL)]
        public void LevelEnd_KnownLevels_ReturnsCorrectValue(int resolution, ulong expected)
        {
            Assert.Equal(expected, CumulativeIndex.LevelEnd(resolution));
        }

        [Fact]
        public void LevelStart_ConsistentWithLevelEnd()
        {
            for (int i = 1; i <= IndexConstants.MaxResolution; i++)
            {
                Assert.Equal(
                    CumulativeIndex.LevelEnd(i - 1) + 1,
                    CumulativeIndex.LevelStart(i));
            }
        }

        // === GetResolution ===

        [Theory]
        [InlineData(1UL, 0)]
        [InlineData(8UL, 0)]
        [InlineData(9UL, 1)]
        [InlineData(40UL, 1)]
        [InlineData(41UL, 2)]
        [InlineData(168UL, 2)]
        [InlineData(169UL, 3)]
        [InlineData(680UL, 3)]
        public void GetResolution_BoundaryValues_ReturnsCorrectLevel(ulong index, int expectedResolution)
        {
            Assert.Equal(expectedResolution, CumulativeIndex.GetResolution(index));
        }

        [Fact]
        public void GetResolution_IndexZero_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => CumulativeIndex.GetResolution(0));
        }

        [Fact]
        public void GetResolution_AboveMaxIndex_ThrowsArgumentOutOfRangeException()
        {
            ulong tooLarge = CumulativeIndex.CumulativeCount(30) + 1;
            Assert.Throws<ArgumentOutOfRangeException>(() => CumulativeIndex.GetResolution(tooLarge));
        }

        [Fact]
        public void GetResolution_Level30LastIndex_Returns30()
        {
            ulong lastIndex = CumulativeIndex.CumulativeCount(30);
            Assert.Equal(30, CumulativeIndex.GetResolution(lastIndex));
        }

        // === GetParent ===

        [Theory]
        [InlineData(74UL, 17UL)]
        [InlineData(17UL, 3UL)]
        [InlineData(73UL, 17UL)]
        [InlineData(75UL, 17UL)]
        [InlineData(76UL, 17UL)]
        public void GetParent_KnownHierarchy_ReturnsCorrectParent(ulong index, ulong expectedParent)
        {
            Assert.Equal(expectedParent, CumulativeIndex.GetParent(index));
        }

        [Fact]
        public void GetParent_Level0_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => CumulativeIndex.GetParent(1));
            Assert.Throws<InvalidOperationException>(() => CumulativeIndex.GetParent(8));
        }

        // === GetChildren ===

        [Fact]
        public void GetChildren_BaseFace3_Returns_17_18_19_20()
        {
            ulong[] children = CumulativeIndex.GetChildren(3);
            Assert.Equal(new ulong[] { 17, 18, 19, 20 }, children);
        }

        [Fact]
        public void GetChildren_Index17_Returns_73_74_75_76()
        {
            ulong[] children = CumulativeIndex.GetChildren(17);
            Assert.Equal(new ulong[] { 73, 74, 75, 76 }, children);
        }

        [Fact]
        public void GetChildren_AlwaysReturns4()
        {
            for (ulong i = 1; i <= 8; i++)
            {
                ulong[] children = CumulativeIndex.GetChildren(i);
                Assert.Equal(4, children.Length);
            }
        }

        [Fact]
        public void GetChildren_Level30_ThrowsInvalidOperationException()
        {
            ulong lastLevel30 = CumulativeIndex.CumulativeCount(30);
            Assert.Throws<InvalidOperationException>(() => CumulativeIndex.GetChildren(lastLevel30));
        }

        // === GetParent/GetChildren round-trip ===

        [Fact]
        public void GetParent_GetChildren_RoundTrip_IsConsistent()
        {
            // Testaa useilla tasoilla
            ulong[] testIndices = { 1, 3, 5, 8, 17, 74, 169, 681 };
            foreach (ulong index in testIndices)
            {
                int resolution = CumulativeIndex.GetResolution(index);
                if (resolution >= IndexConstants.MaxResolution) continue;

                ulong[] children = CumulativeIndex.GetChildren(index);
                for (int i = 0; i < 4; i++)
                {
                    ulong parent = CumulativeIndex.GetParent(children[i]);
                    Assert.Equal(index, parent);
                }
            }
        }

        // === GetBaseFace ===

        [Theory]
        [InlineData(1UL, 0)]
        [InlineData(2UL, 1)]
        [InlineData(3UL, 2)]
        [InlineData(8UL, 7)]
        public void GetBaseFace_Level0_ReturnsCorrectFace(ulong index, int expectedFace)
        {
            Assert.Equal(expectedFace, CumulativeIndex.GetBaseFace(index));
        }

        [Fact]
        public void GetBaseFace_Index74_Returns2()
        {
            // Hierarkia: 3 -> 17 -> 74, base face = 2
            Assert.Equal(2, CumulativeIndex.GetBaseFace(74));
        }

        [Fact]
        public void GetBaseFace_Index17_Returns2()
        {
            Assert.Equal(2, CumulativeIndex.GetBaseFace(17));
        }

        [Fact]
        public void GetBaseFace_AllChildrenSameBaseFace()
        {
            for (ulong baseFaceIndex = 1; baseFaceIndex <= 8; baseFaceIndex++)
            {
                int expectedFace = (int)(baseFaceIndex - 1);
                ulong[] children = CumulativeIndex.GetChildren(baseFaceIndex);
                foreach (ulong child in children)
                {
                    Assert.Equal(expectedFace, CumulativeIndex.GetBaseFace(child));
                }
            }
        }
    }
}
