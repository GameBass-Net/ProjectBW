namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// 월드 생성 파라미터. 같은 Config + 같은 시드 → 같은 월드(결정성).<br/>
    /// 레이어별 노이즈 세부 파라미터는 필드 샘플러(T4) 단계에서 확장한다.
    /// </summary>
    public sealed class WorldGenConfig
    {
        /// <summary>월드 시드. 모든 전역 노이즈 필드의 기반(레이어별로 오프셋해 파생).</summary>
        public uint WorldSeed { get; set; }

        /// <summary>존 한 변 크기(미터). 월드 = 균일 존 그리드.</summary>
        public int ZoneSize { get; set; } = 128;

        /// <summary>존당 하이트맵 해상도(한 변 샘플 수). Unity 터레인 관례상 2^n+1 권장(예: 129).</summary>
        public int HeightmapResolution { get; set; } = 129;

        /// <summary>해수면 높이(정규화 [0,1]). 이보다 낮으면 수중/대양 판정 기준.</summary>
        public float SeaLevel { get; set; } = 0.3f;

        /// <summary>정규화 높이 1.0에 해당하는 절대 높이(미터). 절대 높이 = 정규값 × MaxHeight.</summary>
        public float MaxHeight { get; set; } = 200f;

        // --- P1 섬 마스크 / 레이어별 노이즈 파라미터는 T4~T6에서 추가 ---
    }
}
