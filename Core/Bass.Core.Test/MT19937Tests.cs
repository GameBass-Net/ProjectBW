using Bass.Core;
using Xunit;

namespace Bass.Core.Test
{
    /// <summary>
    /// <see cref="MT19937"/> 결정성·표준 적합성 테스트.
    /// </summary>
    public sealed class MT19937Tests
    {
        /// <summary>
        /// C++ 표준이 명시한 적합성 값: 시드 5489로 10000번째 출력은 4123659995.
        /// 이 값이 맞으면 우리 구현이 <c>std::mt19937</c>와 비트 단위로 동일함이 보장된다.
        /// </summary>
        [Fact]
        public void NextUInt_Seed5489_10000thValueMatchesStdMt19937()
        {
            var mt = new MT19937(5489u);
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
            var a = new MT19937(12345u);
            var b = new MT19937(12345u);
            for (int i = 0; i < 1000; i++)
            {
                Assert.Equal(a.NextUInt(), b.NextUInt());
            }
        }

        [Fact]
        public void NextUInt_DifferentSeeds_ProduceDifferentFirstValue()
        {
            var a = new MT19937(1u);
            var b = new MT19937(2u);
            Assert.NotEqual(a.NextUInt(), b.NextUInt());
        }

        [Fact]
        public void NextRange_ManyDraws_AlwaysWithinBounds()
        {
            var mt = new MT19937(777u);
            for (int i = 0; i < 10000; i++)
            {
                int v = mt.NextRange(5, 10);
                Assert.True(v >= 5 && v < 10);
            }
        }

        [Fact]
        public void NextRange_MinEqualsMax_ReturnsThatValue()
        {
            var mt = new MT19937(1u);
            Assert.Equal(7, mt.NextRange(7, 7));
        }

        [Fact]
        public void NextRange_ReversedBounds_StaysWithinSwappedRange()
        {
            var mt = new MT19937(2u);
            for (int i = 0; i < 10000; i++)
            {
                int v = mt.NextRange(10, 5); // 스왑되어 [5, 10)
                Assert.True(v >= 5 && v < 10);
            }
        }

        [Fact]
        public void NextInt_Zero_ReturnsZero()
        {
            var mt = new MT19937(3u);
            Assert.Equal(0, mt.NextInt(0));
        }

        [Fact]
        public void NextInt_Positive_StaysWithinBounds()
        {
            var mt = new MT19937(4u);
            for (int i = 0; i < 10000; i++)
            {
                int v = mt.NextInt(8);
                Assert.True(v >= 0 && v < 8);
            }
        }
    }
}
