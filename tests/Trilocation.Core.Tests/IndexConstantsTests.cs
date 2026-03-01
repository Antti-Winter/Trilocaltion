using Trilocation.Core.Indexing;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class IndexConstantsTests
    {
        [Fact]
        public void MaxResolution_Is30()
        {
            Assert.Equal(30, IndexConstants.MaxResolution);
        }

        [Fact]
        public void BaseFaceCount_Is8()
        {
            Assert.Equal(8, IndexConstants.BaseFaceCount);
        }

        [Fact]
        public void ChildrenPerTriangle_Is4()
        {
            Assert.Equal(4, IndexConstants.ChildrenPerTriangle);
        }

        [Fact]
        public void LevelStartTable_HasCorrectLength()
        {
            Assert.Equal(31, IndexConstants.LevelStartTable.Length);
        }

        [Fact]
        public void LevelEndTable_HasCorrectLength()
        {
            Assert.Equal(31, IndexConstants.LevelEndTable.Length);
        }

        [Fact]
        public void LevelStartTable_Level0_Is1()
        {
            Assert.Equal(1UL, IndexConstants.LevelStartTable[0]);
        }

        [Fact]
        public void LevelEndTable_Level0_Is8()
        {
            Assert.Equal(8UL, IndexConstants.LevelEndTable[0]);
        }

        [Fact]
        public void LevelStartTable_MatchesLevelEndPlusOne()
        {
            for (int i = 1; i <= IndexConstants.MaxResolution; i++)
            {
                Assert.Equal(
                    IndexConstants.LevelEndTable[i - 1] + 1,
                    IndexConstants.LevelStartTable[i]);
            }
        }

        [Fact]
        public void LevelEndTable_Level30_MatchesCumulativeCount()
        {
            Assert.Equal(12_297_829_382_473_034_408UL, IndexConstants.LevelEndTable[30]);
        }

        [Fact]
        public void TriangleCount_PerLevel_Is8Times4ToTheN()
        {
            for (int i = 0; i <= 15; i++)
            {
                ulong count = IndexConstants.LevelEndTable[i] - IndexConstants.LevelStartTable[i] + 1;
                ulong expected = 8UL * (1UL << (2 * i)); // 8 * 4^i
                Assert.Equal(expected, count);
            }
        }
    }
}
