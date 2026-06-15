namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// 한 지점이 각 바이옴에 속하는 비율(연속 바이옴 블렌딩의 핵심 자료구조).<br/>
    /// 가중치 합은 1에 수렴하며, 텍스처/높이/동굴이 모두 이 가중치로 섞인다.<br/>
    /// 런타임에 위치별로 계산되는 값이므로 <c>const</c>가 아닌 <c>readonly</c> 값 타입이다.
    /// 고정 필드 + 인덱서 구조라 힙 할당이 없고 원소까지 불변이다.
    /// </summary>
    public readonly struct BiomeWeights
    {
        /// <summary>바이옴 개수(= <see cref="EBiomeId"/> 멤버 수).</summary>
        public const int Count = 5;

        /// <summary>초원 가중치.</summary>
        public readonly float Grassland;

        /// <summary>설산 가중치.</summary>
        public readonly float SnowMountain;

        /// <summary>사막 가중치.</summary>
        public readonly float Desert;

        /// <summary>암석지대 가중치.</summary>
        public readonly float Rocky;

        /// <summary>대양 가중치.</summary>
        public readonly float Ocean;

        /// <summary>각 바이옴 가중치로 생성한다.</summary>
        public BiomeWeights(float grassland, float snowMountain, float desert, float rocky, float ocean)
        {
            Grassland = grassland;
            SnowMountain = snowMountain;
            Desert = desert;
            Rocky = rocky;
            Ocean = ocean;
        }

        /// <summary>바이옴 식별자로 해당 가중치를 읽는다(배열식 접근).</summary>
        public float this[EBiomeId id] => id switch
        {
            EBiomeId.Grassland => Grassland,
            EBiomeId.SnowMountain => SnowMountain,
            EBiomeId.Desert => Desert,
            EBiomeId.Rocky => Rocky,
            EBiomeId.Ocean => Ocean,
            _ => 0f,
        };

        /// <summary>가중치 총합. 정규화 검증/디버깅용(정상 시 1에 수렴).</summary>
        public float Sum => Grassland + SnowMountain + Desert + Rocky + Ocean;

        /// <summary>가중치가 가장 큰 지배 바이옴(이산 결정용, 예: 동굴 타입).</summary>
        public EBiomeId Dominant
        {
            get
            {
                EBiomeId best = EBiomeId.Grassland;
                float bestWeight = Grassland;

                if (SnowMountain > bestWeight)
                {
                    best = EBiomeId.SnowMountain;
                    bestWeight = SnowMountain;
                }

                if (Desert > bestWeight)
                {
                    best = EBiomeId.Desert;
                    bestWeight = Desert;
                }

                if (Rocky > bestWeight)
                {
                    best = EBiomeId.Rocky;
                    bestWeight = Rocky;
                }

                if (Ocean > bestWeight)
                {
                    best = EBiomeId.Ocean;
                }

                return best;
            }
        }
    }
}
