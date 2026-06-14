namespace Bass.Core
{
    /// <summary>
    /// 64비트 메르센 트위스터(MT19937-64) 난수 생성기.<br/>
    /// C++ 표준 <c>std::mt19937_64</c>와 동일한 파라미터·시딩·템퍼링을 사용하므로,
    /// 같은 64비트 시드로 초기화하면 <c>std::mt19937_64</c>와 비트 단위로 동일한 시퀀스를 생성한다.<br/>
    /// 32비트가 필요하면 <see cref="MT19937"/>를 사용한다. 상태를 가지므로 클래스(참조 타입)이며,
    /// 레이어별 독립 스트림이 필요하면 인스턴스를 분리해 사용한다.
    /// </summary>
    public sealed class MT19937_64
    {
        // --- MT19937-64 표준 상수 ---
        private const int N = 312;
        private const int M = 156;
        private const ulong MatrixA = 0xB5026F5AA96619E9UL; // 상수 벡터 a
        private const ulong UpperMask = 0xFFFFFFFF80000000UL; // 상위 33비트
        private const ulong LowerMask = 0x7FFFFFFFUL;         // 하위 31비트

        private readonly ulong[] _mt = new ulong[N]; // 상태 벡터
        private int _mti = N + 1;                     // _mti == N+1 이면 미시딩 상태

        /// <summary>기본 시드(5489)로 초기화한다. <c>std::mt19937_64</c>의 기본 생성자와 동일.</summary>
        public MT19937_64()
        {
            Seed(5489UL);
        }

        /// <summary>지정한 64비트 시드로 초기화한다.</summary>
        /// <param name="seed">초기 시드. <c>std::mt19937_64</c>의 단일 값 시딩과 동일하게 처리된다.</param>
        public MT19937_64(ulong seed)
        {
            Seed(seed);
        }

        /// <summary>
        /// 64비트 시드로 상태 벡터를 재초기화한다(<c>init_genrand64</c>와 동일).<br/>
        /// 점화식: <c>mt[i] = 6364136223846793005 * (mt[i-1] ^ (mt[i-1] >> 62)) + i</c> (ulong 랩어라운드).
        /// </summary>
        /// <param name="seed">초기 시드.</param>
        public void Seed(ulong seed)
        {
            _mt[0] = seed;
            for (int i = 1; i < N; i++)
            {
                // ulong 곱셈/덧셈은 C#에서 기본 unchecked로 64비트 랩어라운드 → C 구현과 동일.
                _mt[i] = (6364136223846793005UL * (_mt[i - 1] ^ (_mt[i - 1] >> 62))) + (ulong)i;
            }
            _mti = N;
        }

        /// <summary>다음 64비트 난수를 반환한다(<c>genrand64_int64</c>, 템퍼링 적용).</summary>
        /// <returns>[0, 2^64) 범위의 균등 난수.</returns>
        public ulong NextULong()
        {
            ulong x;

            if (_mti >= N)
            {
                // _mti == N+1(미시딩)인 경우에도 안전하게 기본 시드로 초기화한다.
                if (_mti == N + 1)
                {
                    Seed(5489UL);
                }

                int i;
                for (i = 0; i < N - M; i++)
                {
                    x = (_mt[i] & UpperMask) | (_mt[i + 1] & LowerMask);
                    _mt[i] = _mt[i + M] ^ (x >> 1) ^ Mag01(x);
                }
                for (; i < N - 1; i++)
                {
                    x = (_mt[i] & UpperMask) | (_mt[i + 1] & LowerMask);
                    _mt[i] = _mt[i + (M - N)] ^ (x >> 1) ^ Mag01(x);
                }
                x = (_mt[N - 1] & UpperMask) | (_mt[0] & LowerMask);
                _mt[N - 1] = _mt[M - 1] ^ (x >> 1) ^ Mag01(x);

                _mti = 0;
            }

            x = _mt[_mti++];

            // 템퍼링
            x ^= (x >> 29) & 0x5555555555555555UL;
            x ^= (x << 17) & 0x71D67FFFEDA60000UL;
            x ^= (x << 37) & 0xFFF7EEE000000000UL;
            x ^= x >> 43;

            return x;
        }

        /// <summary><c>x &amp; 1</c>이 0이면 0, 1이면 <see cref="MatrixA"/>를 반환한다(<c>mag01</c> 테이블 대체).</summary>
        private static ulong Mag01(ulong x)
        {
            return (x & 1UL) == 0UL ? 0UL : MatrixA;
        }

        /// <summary>[0.0, 1.0) 범위의 double 난수를 반환한다. (상위 53비트 사용, <c>genrand64_real2</c>와 동일)</summary>
        public double NextDouble01()
        {
            return (NextULong() >> 11) * (1.0 / 9007199254740992.0); // 2^53
        }

        /// <summary>[0.0, 1.0) 범위의 float 난수를 반환한다.</summary>
        public float NextFloat01()
        {
            return (float)NextDouble01();
        }

        /// <summary>[0, maxExclusive) 범위의 정수 난수를 반환한다. <see cref="NextRange(int, int)"/>의 0-기반 편의 함수.</summary>
        /// <param name="maxExclusive">상한(미포함). 0이면 0 반환, 음수면 [maxExclusive, 0) 처리.</param>
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
        public int NextRange(int minInclusive, int maxExclusive)
        {
            if (minInclusive == maxExclusive)
                return minInclusive;

            if (minInclusive > maxExclusive)
                (minInclusive, maxExclusive) = (maxExclusive, minInclusive);

            ulong range = (ulong)((long)maxExclusive - minInclusive);
            // threshold == 2^64 % range. r < threshold인 구간을 버려 균등 분포를 유지한다.
            ulong threshold = (0UL - range) % range;

            ulong r;
            do
            {
                r = NextULong();
            }
            while (r < threshold);

            return minInclusive + (int)(r % range);
        }
    }
}
