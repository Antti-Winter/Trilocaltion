using Trilocation.Core.Indexing;

namespace Trilocation.Data.Tests
{
    public class TriIndexValueConverterTests
    {
        private readonly TriIndexValueConverter _converter = new();

        private long ConvertToLong(ulong value)
        {
            var convertToProvider = _converter.ConvertToProviderExpression.Compile();
            return convertToProvider(value);
        }

        private ulong ConvertToUlong(long value)
        {
            var convertFromProvider = _converter.ConvertFromProviderExpression.Compile();
            return convertFromProvider(value);
        }

        // === Round-trip tests ===

        [Fact]
        public void RoundTrip_Zero()
        {
            ulong original = 0;
            long asLong = ConvertToLong(original);
            ulong back = ConvertToUlong(asLong);
            Assert.Equal(original, back);
        }

        [Fact]
        public void RoundTrip_One()
        {
            ulong original = 1;
            long asLong = ConvertToLong(original);
            ulong back = ConvertToUlong(asLong);
            Assert.Equal(original, back);
        }

        [Fact]
        public void RoundTrip_LongMaxValue()
        {
            ulong original = (ulong)long.MaxValue;
            long asLong = ConvertToLong(original);
            ulong back = ConvertToUlong(asLong);
            Assert.Equal(original, back);
        }

        [Fact]
        public void RoundTrip_AboveLongMaxValue()
        {
            ulong original = (ulong)long.MaxValue + 1;
            long asLong = ConvertToLong(original);
            ulong back = ConvertToUlong(asLong);
            Assert.Equal(original, back);
        }

        [Fact]
        public void RoundTrip_UlongMaxValue()
        {
            ulong original = ulong.MaxValue;
            long asLong = ConvertToLong(original);
            ulong back = ConvertToUlong(asLong);
            Assert.Equal(original, back);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        [InlineData(20)]
        [InlineData(25)]
        [InlineData(29)]
        public void RoundTrip_CumulativeCountAtResolution(int resolution)
        {
            ulong original = CumulativeIndex.CumulativeCount(resolution);
            long asLong = ConvertToLong(original);
            ulong back = ConvertToUlong(asLong);
            Assert.Equal(original, back);
        }

        [Fact]
        public void RoundTrip_Resolution30_CumulativeCount()
        {
            ulong original = CumulativeIndex.CumulativeCount(30);
            long asLong = ConvertToLong(original);
            ulong back = ConvertToUlong(asLong);
            Assert.Equal(original, back);
        }

        // === Bit pattern tests ===

        [Fact]
        public void ConvertToLong_ZeroMapsToZero()
        {
            long result = ConvertToLong(0);
            Assert.Equal(0L, result);
        }

        [Fact]
        public void ConvertToLong_LongMaxValuePreserved()
        {
            long result = ConvertToLong((ulong)long.MaxValue);
            Assert.Equal(long.MaxValue, result);
        }

        [Fact]
        public void ConvertToLong_AboveLongMaxValueBecomesNegative()
        {
            long result = ConvertToLong((ulong)long.MaxValue + 1);
            Assert.Equal(long.MinValue, result);
        }

        [Fact]
        public void ConvertToLong_UlongMaxValueBecomesMinusOne()
        {
            long result = ConvertToLong(ulong.MaxValue);
            Assert.Equal(-1L, result);
        }

        [Fact]
        public void ConvertFromLong_NegativeOneBecomesUlongMaxValue()
        {
            ulong result = ConvertToUlong(-1L);
            Assert.Equal(ulong.MaxValue, result);
        }

        [Fact]
        public void ConvertFromLong_LongMinValueBecomesLongMaxValuePlusOne()
        {
            ulong result = ConvertToUlong(long.MinValue);
            Assert.Equal((ulong)long.MaxValue + 1, result);
        }
    }
}
