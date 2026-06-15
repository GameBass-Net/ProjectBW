namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// <see cref="BiomeClassifier"/> 튜닝 파라미터. 시작값은 임시이며 "만들어보고 조정"(D5 보류)한다.
    /// </summary>
    public sealed class BiomeClassifierSettings
    {
        /// <summary>해수면 아래로 이 폭(정규화 높이)만큼이 해안 전이대. 0이면 칼같은 해안선.</summary>
        public float OceanCoastBand { get; set; } = 0.05f;

        /// <summary>기후 거리 → 가중치 변환의 날카로움. 클수록 바이옴 경계가 또렷해진다.</summary>
        public float BlendSharpness { get; set; } = 25.0f;

        /// <summary>초원 기후 이상점.</summary>
        public BiomeClimate Grassland { get; set; } = new BiomeClimate(0.40f, 0.55f, 0.60f);

        /// <summary>설산 기후 이상점(고지·한랭).</summary>
        public BiomeClimate SnowMountain { get; set; } = new BiomeClimate(0.85f, 0.10f, 0.50f);

        /// <summary>사막 기후 이상점(저지·고온·건조).</summary>
        public BiomeClimate Desert { get; set; } = new BiomeClimate(0.35f, 0.90f, 0.12f);

        /// <summary>암석지대 기후 이상점(고지·건조~온대).</summary>
        public BiomeClimate Rocky { get; set; } = new BiomeClimate(0.80f, 0.50f, 0.25f);
    }
}
