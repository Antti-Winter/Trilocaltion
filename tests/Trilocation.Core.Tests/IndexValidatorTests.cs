using Trilocation.Core.Indexing;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class IndexValidatorTests
    {
        [Theory]
        [InlineData(1UL)]
        [InlineData(8UL)]
        [InlineData(74UL)]
        [InlineData(10_920UL)]
        public void IsValid_ValidIndex_ReturnsTrue(ulong index)
        {
            Assert.True(IndexValidator.IsValid(index));
        }

        [Fact]
        public void IsValid_Zero_ReturnsFalse()
        {
            Assert.False(IndexValidator.IsValid(0));
        }

        [Fact]
        public void IsValid_AboveMaxIndex_ReturnsFalse()
        {
            ulong maxIndex = CumulativeIndex.CumulativeCount(30);
            Assert.False(IndexValidator.IsValid(maxIndex + 1));
        }

        [Fact]
        public void IsValid_MaxIndex_ReturnsTrue()
        {
            ulong maxIndex = CumulativeIndex.CumulativeCount(30);
            Assert.True(IndexValidator.IsValid(maxIndex));
        }

        [Fact]
        public void ValidateIndex_Zero_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => IndexValidator.ValidateIndex(0));
        }

        [Fact]
        public void ValidateIndex_AboveMax_ThrowsArgumentOutOfRangeException()
        {
            ulong maxIndex = CumulativeIndex.CumulativeCount(30);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => IndexValidator.ValidateIndex(maxIndex + 1));
        }

        [Fact]
        public void ValidateIndex_ValidIndex_DoesNotThrow()
        {
            IndexValidator.ValidateIndex(1);
            IndexValidator.ValidateIndex(74);
            IndexValidator.ValidateIndex(CumulativeIndex.CumulativeCount(30));
        }
    }
}
