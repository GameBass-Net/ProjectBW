using System;
using Bass.BW.WorldGeneration;
using Xunit;

namespace Bass.BW.Test.WorldGeneration
{
    /// <summary>
    /// <see cref="HeightField"/> 인덱싱·초기화·유효성 검증.
    /// </summary>
    public sealed class HeightFieldTests
    {
        [Fact]
        public void Indexer_SetThenGet_RoundTrips()
        {
            var field = new HeightField(4, 3);
            field[2, 1] = 0.75f;
            Assert.Equal(0.75f, field[2, 1]);
        }

        [Fact]
        public void NewField_IsZeroInitialized()
        {
            var field = new HeightField(2, 2);
            Assert.Equal(0f, field[0, 0]);
            Assert.Equal(0f, field[1, 1]);
        }

        [Fact]
        public void Indexer_IsRowMajor()
        {
            var field = new HeightField(3, 2);
            field[1, 1] = 0.5f; // 인덱스 = 1*3 + 1 = 4
            Assert.Equal(0.5f, field.Values[4]);
        }

        [Fact]
        public void Constructor_NonPositiveSize_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HeightField(0, 5));
        }
    }
}
