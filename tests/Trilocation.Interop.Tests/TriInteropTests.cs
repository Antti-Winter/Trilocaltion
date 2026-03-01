using Xunit;
using Trilocation.Core;
using Trilocation.Interop;

namespace Trilocation.Interop.Tests
{
    public class TriInteropTests
    {
        // === Facade delegation tests ===

        [Fact]
        public void Facade_ToGeohash_MatchesConverter()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string fromFacade = TriInterop.ToGeohash(location, 6);
            string fromConverter = GeohashConverter.ToGeohash(location, 6);
            Assert.Equal(fromConverter, fromFacade);
        }

        [Fact]
        public void Facade_FromGeohash_MatchesConverter()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string geohash = GeohashConverter.ToGeohash(location, 6);
            var fromFacade = TriInterop.FromGeohash(geohash, 10);
            var fromConverter = GeohashConverter.FromGeohash(geohash, 10);
            Assert.Equal(fromConverter.Index, fromFacade.Index);
        }

        [Fact]
        public void Facade_ToPlusCode_MatchesConverter()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string fromFacade = TriInterop.ToPlusCode(location, 10);
            string fromConverter = PlusCodeConverter.ToPlusCode(location, 10);
            Assert.Equal(fromConverter, fromFacade);
        }

        [Fact]
        public void Facade_FromPlusCode_MatchesConverter()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string code = PlusCodeConverter.ToPlusCode(location, 10);
            var fromFacade = TriInterop.FromPlusCode(code, 10);
            var fromConverter = PlusCodeConverter.FromPlusCode(code, 10);
            Assert.Equal(fromConverter.Index, fromFacade.Index);
        }

        [Fact]
        public void Facade_ToQuadkey_MatchesConverter()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string fromFacade = TriInterop.ToQuadkey(location, 12);
            string fromConverter = QuadkeyConverter.ToQuadkey(location, 12);
            Assert.Equal(fromConverter, fromFacade);
        }

        [Fact]
        public void Facade_ToMaidenhead_MatchesConverter()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string fromFacade = TriInterop.ToMaidenhead(location, 3);
            string fromConverter = MaidenheadConverter.ToMaidenhead(location, 3);
            Assert.Equal(fromConverter, fromFacade);
        }

        [Fact]
        public void Facade_ToH3_MatchesConverter()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            ulong fromFacade = TriInterop.ToH3(location, 9);
            ulong fromConverter = H3Converter.ToH3(location, 9);
            Assert.Equal(fromConverter, fromFacade);
        }

        [Fact]
        public void Facade_ToS2CellId_MatchesConverter()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            ulong fromFacade = TriInterop.ToS2CellId(location, 15);
            ulong fromConverter = S2Converter.ToS2CellId(location, 15);
            Assert.Equal(fromConverter, fromFacade);
        }
    }
}
