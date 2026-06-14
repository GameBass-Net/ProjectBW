namespace Bass.Core
{
    /// <summary>
    /// 32비트 메르센 트위스터(MT19937) 난수 생성기.<br/>
    /// C++ 표준 <c>std::mt19937</c>와 동일한 파라미터·시딩·템퍼링을 사용하므로,
    /// 같은 32비트 시드로 초기화하면 <c>std::mt19937</c>와 비트 단위로 동일한 시퀀스를 생성한다.<br/>
    /// 월드 생성의 결정성(같은 시드 → 같은 월드) 보장을 위해, 생성 코어의 모든 난수는 이 클래스로만 뽑는다.<br/>
    /// 상태를 가지므로 클래스(참조 타입)이며, 레이어별 독립 스트림이 필요하면 인스턴스를 분리해 사용한다.
    /// </summary>
    public sealed class MT19937
    {
        // --- MT19937 표준 상수 ---
        private const int N = 624;
        private const int M = 397;
        private const uint MatrixA = 0x9908b0dfu;   // 상수 벡터 a
        private const uint UpperMask = 0x80000000u; // 상위 1비트
        private const uint LowerMask = 0x7fffffffu; // 하위 31비트

        private readonly uint[] _mt = new uint[N]; // 상태 벡터
        private int _mti = N + 1;                  // _mti == N+1 이면 미시딩 상태

        /// <summary>기본 시드(5489)로 초기화한다. <c>std::mt19937</c>의 기본 생성자와 동일.</summary>
        public MT19937()
        {
            Seed(5489u);
        }

        /// <summary>지정한 32비트 시드로 초기화한다.</summary>
        /// <param name="seed">초기 시드. <c>std::mt19937</c>의 단일 값 시딩과 동일하게 처리된다.</param>
        public MT19937(uint seed)
        {
            Seed(seed);
        }

        /// <summary>
        /// 32비트 시드로 상태 벡터를 재초기화한다(<c>init_genrand</c>와 동일).<br/>
        /// 점화식: <c>mt[i] = 1812433253 * (mt[i-1] ^ (mt[i-1] >> 30)) + i</c> (uint 랩어라운드).
        /// </summary>
        /// <param name="seed">초기 시드.</param>
        public void Seed(uint seed)
        {
            _mt[0] = seed;
            for (int i = 1; i < N; i++)
            {
                // uint 곱셈/덧셈은 C#에서 기본 unchecked로 32비트 랩어라운드 → C 구현과 동일.
                _mt[i] = (uint)(1812433253u * (_mt[i - 1] ^ (_mt[i - 1] >> 30)) + (uint)i);
            }
            _mti = N;
        }

        /// <summary>다음 32비트 난수를 반환한다(<c>genrand_int32</c>, 템퍼링 적용).</summary>
        /// <returns>[0, 2^32) 범위의 균등 난수.</returns>
        public uint NextUInt()
        {
            uint y;

            if (_mti >= N)
            {
                // _mti == N+1(미시딩)인 경우에도 안전하게 기본 시드로 초기화한다.
                if (_mti == N + 1)
                {
                    Seed(5489u);
                }

                int kk;
                for (kk = 0; kk < N - M; kk++)
                {
                    y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                    _mt[kk] = _mt[kk + M] ^ (y >> 1) ^ Mag01(y);
                }
                for (; kk < N - 1; kk++)
                {
                    y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                    _mt[kk] = _mt[kk + (M - N)] ^ (y >> 1) ^ Mag01(y);
                }
                y = (_mt[N - 1] & UpperMask) | (_mt[0] & LowerMask);
                _mt[N - 1] = _mt[M - 1] ^ (y >> 1) ^ Mag01(y);

                _mti = 0;
            }

            y = _mt[_mti++];

            // 템퍼링
            y ^= y >> 11;
            y ^= (y << 7) & 0x9d2c5680u;
            y ^= (y << 15) & 0xefc60000u;
            y ^= y >> 18;

            return y;
        }

        /// <summary><c>y &amp; 1</c>이 0이면 0, 1이면 <see cref="MatrixA"/>를 반환한다(<c>mag01</c> 테이블 대체).</summary>
        private static uint Mag01(uint y)
        {
            // (y & 1) == 0 → 0, else MatrixA
            return (y & 1u) == 0u ? 0u : MatrixA;
        }

        /// <summary>[0.0, 1.0) 범위의 float 난수를 반환한다. (32비트 난수를 2^32로 나눔)</summary>
        public float NextFloat01()
        {
            return (float)(NextUInt() * (1.0 / 4294967296.0));
        }

        /// <summary>[0.0, 1.0) 범위의 double 난수를 반환한다. (32비트 난수를 2^32로 나눔)</summary>
        public double NextDouble01()
        {
            return NextUInt() * (1.0 / 4294967296.0);
        }

        /// <summary>
        /// [0, maxExclusive) 범위의 정수 난수를 반환한다. <see cref="NextRange(int, int)"/>의 0-기반 편의 함수.<br/>
        /// maxExclusive가 0이면 0을 반환하고, 음수면 [maxExclusive, 0) 구간으로 처리된다(NextRange의 관대한 동작을 그대로 따름).
        /// </summary>
        /// <param name="maxExclusive">상한(미포함).</param>
        public int NextInt(int maxExclusive)
        {
            return NextRange(0, maxExclusive);
        }

        /// <summary>
        /// [minInclusive, maxExclusive) 범위의 정수 난수를 반환한다(모듈로 편향 없는 리젝션 샘플링).<br/>
        /// 두 값이 같으면 그 값을 반환하고, 역전(min &gt; max)되면 두 값을 스왑해 처리한다(예외 없음).
        /// </summary>
        /// <param name="minInclusive">하한(포함).</param>
        /// <param name="maxExclusive">상한(미포함).</param>
        /// <remarks>
        /// 범위 폭(max-min)이 int 한계를 넘는 극단 입력(예: int 전체 구간)은 내부 뺄셈 오버플로로 결과가 정의되지 않는다.
        /// 정상적 사용 범위에서는 무관하다.
        /// </remarks>
        public int NextRange(int minInclusive, int maxExclusive)
        {
            if (minInclusive == maxExclusive)
                return minInclusive;

            if (minInclusive > maxExclusive)
                (minInclusive, maxExclusive) = (maxExclusive, minInclusive);

            uint range = (uint)(maxExclusive - minInclusive);
            // threshold == 2^32 % range. r < threshold인 구간을 버려 균등 분포를 유지한다.
            uint threshold = (0u - range) % range;

            uint r;
            do
            {
                r = NextUInt();
            }
            while (r < threshold);

            return minInclusive + (int)(r % range);
        }
    }
}
