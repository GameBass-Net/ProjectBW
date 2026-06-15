using System;

namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// 전역좌표를 입력받는 바이옴 분류기(D4의 순수 함수 <c>BiomeAt</c>).<br/>
    /// ① 대륙도 → 바다/육지 연속 전이, ② 육지면 (고도·온도·습도) 기후 거리로 4종 연속 블렌딩.
    /// 좌표만으로 결정되며 저장이 필요 없다(같은 Config·시드 → 같은 결과).
    /// </summary>
    public sealed class BiomeClassifier
    {
        private readonly NoiseField _continentalness;
        private readonly NoiseField _elevation;
        private readonly NoiseField _temperature;
        private readonly NoiseField _moisture;
        private readonly BiomeClassifierSettings _settings;

        /// <summary>Config의 노이즈 레이어·분류 설정으로 분류기를 만든다.</summary>
        public BiomeClassifier(WorldGenConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            _continentalness = NoiseField.Create(config, EFieldLayer.Continentalness);
            _elevation = NoiseField.Create(config, EFieldLayer.Elevation);
            _temperature = NoiseField.Create(config, EFieldLayer.Temperature);
            _moisture = NoiseField.Create(config, EFieldLayer.Moisture);
            _settings = config.Biome;
        }

        /// <summary>전역 월드좌표 (worldX, worldZ)의 바이옴 가중치. 합은 1에 수렴.</summary>
        public BiomeWeights BiomeAt(float worldX, float worldZ)
        {
            float c = _continentalness.Sample(worldX, worldZ);
            float e = _elevation.Sample(worldX, worldZ);
            float t = _temperature.Sample(worldX, worldZ);
            float m = _moisture.Sample(worldX, worldZ);

            float land = SmoothStep(_settings.CoastStart, _settings.CoastEnd, c);
            float ocean = 1f - land;

            float wGrass = ClimateWeight(_settings.Grassland, e, t, m);
            float wSnow = ClimateWeight(_settings.SnowMountain, e, t, m);
            float wDesert = ClimateWeight(_settings.Desert, e, t, m);
            float wRocky = ClimateWeight(_settings.Rocky, e, t, m);

            // 가우시안 가중치라 합은 항상 양수. 정규화 후 육지 비율(land)을 곱한다.
            float scale = land / (wGrass + wSnow + wDesert + wRocky);
            return new BiomeWeights(wGrass * scale, wSnow * scale, wDesert * scale, wRocky * scale, ocean);
        }

        /// <summary>기후 이상점과의 제곱거리를 가우시안으로 가중치화한다.</summary>
        private float ClimateWeight(BiomeClimate target, float e, float t, float m)
        {
            float de = e - target.Elevation;
            float dt = t - target.Temperature;
            float dm = m - target.Moisture;
            float dist2 = de * de + dt * dt + dm * dm;
            return (float)Math.Exp(-_settings.BlendSharpness * dist2);
        }

        /// <summary>Hermite smoothstep. edge0 ≥ edge1이면 edge0 기준 계단으로 폴백.</summary>
        private static float SmoothStep(float edge0, float edge1, float x)
        {
            if (edge0 >= edge1) return x < edge0 ? 0f : 1f;
            float u = Math.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
            return u * u * (3f - 2f * u);
        }
    }
}
