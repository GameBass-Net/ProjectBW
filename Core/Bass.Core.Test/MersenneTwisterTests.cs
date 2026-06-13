using Bass.Core;
using Xunit;

namespace Bass.Core.Test
{
    /// <summary>
    /// <see cref="MersenneTwister"/> 결정성·표준 적합성 테스트.
    /// </summary>
    public sealed class MersenneTwisterTests
    {
        /// <summary>
        /// C++ 표준이 명시한 적합성 값: 시드 5489로 10000번째 출력은 4123659995.
        /// 이 값이 맞으면 우리 구현이 <c>std::mt19937</c>와 비트 단위로 동일함이 보장된다.
        /// </summary>
        [Fact]
        public void NextUInt_Seed5489_10000thValueMatchesStdMt19937()
        {
            var mt = new MersenneTwister(5489u);
            uint value = 0u;
            for (int i = 0; i < 10000; i++)
            {
                value = mt.NextUInt();
            }
            Assert.Equal(4123659995u, value);
        }

        [Fact]
        public void NextUInt_SameSeed_ProducesIdenticalSequence()
        {
            var a = new MersenneTwister(12345u);
            var b = new MersenneTwister(12345u);
            for (int i = 0; i < 1000; i++)
            {
                Assert.Equal(a.NextUInt(), b.NextUInt());
            }
        }

        [Fact]
        public void NextUInt_DifferentSeeds_ProduceDifferentFirstValue()
        {
            var a = new MersenneTwister(1u);
            var b = new MersenneTwister(2u);
            Assert.NotEqual(a.NextUInt(), b.NextUInt());
        }

        [Fact]
        public void NextRange_ManyDraws_AlwaysWithinBounds()
        {
            var mt = new MersenneTwister(777u);
            for (int i = 0; i < 10000; i++)
            {
                int v = mt.NextRange(5, 10);
                Assert.True(v >= 5 && v < 10);
            }
        }
    }
}
