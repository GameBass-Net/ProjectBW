namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// <see cref="BiomeClassifier"/> 튜닝 파라미터. 시작값은 임시이며 "만들어보고 조정"(D5 보류)한다.
    /// </summary>
    public sealed class BiomeClassifierSettings
    {
        /// <summary>대륙도가 이 값 이하면 완전 바다. (해안 전이대 시작)</summary>
        public float CoastStart { get; set; } = 0.40f;

        /// <summary>대륙도가 이 값 이상이면 완전 육지. (해안 전이대 끝)</summary>
        public float CoastEnd { get; set; } = 0.55f;

        /// <summary>기후 거리 → 가중치 변환의 날카로움. 클수록 바이옴 경계가 또렷해진다.</summary>
        public float BlendSharpness { get; set; } = 8.0f;

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
