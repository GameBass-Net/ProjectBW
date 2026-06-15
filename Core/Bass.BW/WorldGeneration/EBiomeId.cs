namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// 바이옴 식별자. 육지 4종 + 대양 1종.<br/>
    /// 정수값은 <see cref="BiomeWeights"/> 인덱싱·마스터데이터 테이블 인덱스로 사용되므로 0부터 연속이어야 한다. <br/>
    /// TODO : 아래 enum 은 프로토타입 v1 용. 이후 타입 변경이나 추가 예정. <br/>
    /// </summary>
    public enum EBiomeId
    {
        /// <summary>초원.</summary>
        Grassland = 0,

        /// <summary>설산.</summary>
        SnowMountain = 1,

        /// <summary>사막.</summary>
        Desert = 2,

        /// <summary>암석지대.</summary>
        Rocky = 3,

        /// <summary>대양(순수 바다).</summary>
        Ocean = 4,
    }
}
