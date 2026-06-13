using Xunit;

namespace Bass.Core.Test
{
    /// <summary>
    /// 벤더링한 전역 <c>FastNoiseLite</c>가 코어에서 사용 가능하고 결정적인지 확인.
    /// </summary>
    public sealed class FastNoiseLiteTests
    {
        [Fact]
        public void GetNoise_SameSeedSameCoord_IsDeterministic()
        {
            var a = new FastNoiseLite(1337);
            var b = new FastNoiseLite(1337);
            Assert.Equal(a.GetNoise(12.5f, -7.25f), b.GetNoise(12.5f, -7.25f));
        }

        [Fact]
        public void GetNoise_DifferentSeed_DiffersSomewhere()
        {
            var a = new FastNoiseLite(1);
            var b = new FastNoiseLite(2);
            bool anyDifferent = false;
            for (int i = 0; i < 8 && !anyDifferent; i++)
            {
                if (a.GetNoise(i * 3.1f, i * 1.7f) != b.GetNoise(i * 3.1f, i * 1.7f))
                {
                    anyDifferent = true;
                }
            }
            Assert.True(anyDifferent);
        }

        [Fact]
        public void GetNoise_2D_StaysWithinUnitRange()
        {
            var n = new FastNoiseLite(42);
            for (int i = 0; i < 1000; i++)
            {
                float v = n.GetNoise(i * 0.13f, i * 0.37f);
                Assert.True(v >= -1.0001f && v <= 1.0001f);
            }
        }
    }
}
