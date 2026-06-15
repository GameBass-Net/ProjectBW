namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// 전역 연속 노이즈 필드의 레이어 식별자(P1: Erosion 제외 4종).<br/>
    /// 정수값은 <see cref="WorldGenConfig.NoiseLayers"/> 인덱싱과 레이어 시드 파생
    /// (<see cref="NoiseField.DeriveLayerSeed"/>)에 직접 쓰이므로 0부터 연속이며 안정적이어야 한다.
    /// </summary>
    public enum EFieldLayer
    {
        /// <summary>대륙도 — 바다/해안/내륙, 섬 형태.</summary>
        Continentalness = 0,

        /// <summary>고도 — 러프 하이트맵 베이스.</summary>
        Elevation = 1,

        /// <summary>온도 — 기후.</summary>
        Temperature = 2,

        /// <summary>습도 — 건/습.</summary>
        Moisture = 3,
    }
}
