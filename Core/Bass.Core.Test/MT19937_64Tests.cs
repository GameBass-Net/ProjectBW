using Bass.Core;
using Xunit;

namespace Bass.Core.Test
{
    /// <summary>
    /// <see cref="MT19937_64"/> 결정성·표준 적합성 테스트.
    /// </summary>
    public sealed class MT19937_64Tests
    {
        /// <summary>
        /// C++ 표준이 명시한 적합성 값: 시드 5489로 10000번째 출력은 9981545732273789042.
        /// 이 값이 맞으면 우리 구현이 <c>std::mt19937_64</c>와 비트 단위로 동일함이 보장된다.
        /// </summary>
        [Fact]
        public void NextULong_Seed5489_10000thValueMatchesStdMt19937_64()
        {
            var mt = new MT19937_64(5489UL);
            ulong value = 0UL;
            for (int i = 0; i < 10000; i++)
            {
                value = mt.NextULong();
            }
            Assert.Equal(9981545732273789042UL, value);
        }

        [Fact]
        public void NextULong_SameSeed_ProducesIdenticalSequence()
        {
            var a = new MT19937_64(12345UL);
            var b = new MT19937_64(12345UL);
            for (int i = 0; i < 1000; i++)
            {
                Assert.Equal(a.NextULong(), b.NextULong());
            }
        }

        [Fact]
        public void NextULong_DifferentSeeds_ProduceDifferentFirstValue()
        {
            var a = new MT19937_64(1UL);
            var b = new MT19937_64(2UL);
            Assert.NotEqual(a.NextULong(), b.NextULong());
        }

        [Fact]
        public void NextDouble01_ManyDraws_StaysInUnitRange()
        {
            var mt = new MT19937_64(99UL);
            for (int i = 0; i < 10000; i++)
            {
                double v = mt.NextDouble01();
                Assert.True(v >= 0.0 && v < 1.0);
            }
        }

        [Fact]
        public void NextRange_ManyDraws_AlwaysWithinBounds()
        {
            var mt = new MT19937_64(777UL);
            for (int i = 0; i < 10000; i++)
            {
                int v = mt.NextRange(5, 10);
                Assert.True(v >= 5 && v < 10);
            }
        }

        [Fact]
        public void NextInt_Zero_ReturnsZero()
        {
            var mt = new MT19937_64(3UL);
            Assert.Equal(0, mt.NextInt(0));
        }
    }
}
