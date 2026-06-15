namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// 높이 합성(<see cref="HeightSynthesizer"/>) 튜닝 파라미터. 시작값은 임시(만들어보고 조정).<br/>
    /// 바이옴 셰이핑은 접근 A(베이스 고도 + 바이옴 가중 가감). P1 섬 마스크는 반경 감쇠.
    /// </summary>
    public sealed class HeightSynthesisSettings
    {
        /// <summary>초원 가중치당 높이 가감(기준 평지).</summary>
        public float GrasslandHeightBias { get; set; } = 0.0f;

        /// <summary>설산 가중치당 높이 가감(솟음).</summary>
        public float SnowMountainHeightBias { get; set; } = 0.35f;

        /// <summary>사막 가중치당 높이 가감(저지·평탄).</summary>
        public float DesertHeightBias { get; set; } = -0.05f;

        /// <summary>암석지대 가중치당 높이 가감(고지).</summary>
        public float RockyHeightBias { get; set; } = 0.25f;

        /// <summary>P1 섬 마스크 중심 X(월드좌표).</summary>
        public float IslandCenterX { get; set; } = 64f;

        /// <summary>P1 섬 마스크 중심 Z(월드좌표).</summary>
        public float IslandCenterZ { get; set; } = 64f;

        /// <summary>중심에서 이 거리까지는 마스크 1(높이 유지).</summary>
        public float IslandFalloffStart { get; set; } = 45f;

        /// <summary>이 거리 이상은 마스크 0(완전 침수).</summary>
        public float IslandFalloffEnd { get; set; } = 64f;
    }
}
