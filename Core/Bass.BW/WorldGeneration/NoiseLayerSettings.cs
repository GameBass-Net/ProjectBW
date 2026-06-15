using Bass.Core;

namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// 한 노이즈 레이어(<see cref="EFieldLayer"/>)의 프랙탈 노이즈 파라미터.<br/>
    /// <see cref="NoiseField"/> 생성 시 <see cref="FastNoiseLite"/>에 그대로 적용된다.
    /// 시드는 여기에 두지 않는다(월드 시드에서 파생, <see cref="NoiseField.DeriveLayerSeed"/>).<br/>
    /// 값은 튜닝 단계에서 조정한다(스케일/주파수 확정은 D5 보류 항목).
    /// Cellular·DomainWarp 계열 옵션은 이 연속 필드들에 쓰지 않으므로 노출하지 않는다.
    /// </summary>
    public sealed class NoiseLayerSettings
    {
        /// <summary>노이즈 종류.</summary>
        public FastNoiseLite.NoiseType NoiseType { get; set; } = FastNoiseLite.NoiseType.OpenSimplex2;

        /// <summary>주파수(전역 월드좌표 기준). 클수록 패턴이 잘게 반복된다.</summary>
        public float Frequency { get; set; } = 0.01f;

        /// <summary>프랙탈(옥타브 합성) 방식.</summary>
        public FastNoiseLite.FractalType FractalType { get; set; } = FastNoiseLite.FractalType.FBm;

        /// <summary>프랙탈 옥타브 수.</summary>
        public int FractalOctaves { get; set; } = 3;

        /// <summary>옥타브마다 주파수 배수.</summary>
        public float FractalLacunarity { get; set; } = 2.0f;

        /// <summary>옥타브마다 진폭 배수.</summary>
        public float FractalGain { get; set; } = 0.5f;

        /// <summary>고진폭 옥타브 가중(0이면 표준 합성).</summary>
        public float FractalWeightedStrength { get; set; } = 0.0f;

        /// <summary>PingPong 프랙탈 강도(<see cref="FractalType"/>가 PingPong일 때만 의미).</summary>
        public float FractalPingPongStrength { get; set; } = 2.0f;
    }
}
