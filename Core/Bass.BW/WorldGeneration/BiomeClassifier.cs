using System;

namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// 전역좌표 → 육지 기후 블렌드(순수 함수). (고도·온도·습도) 기후 거리로 육지 4종을 연속 블렌딩한다.<br/>
    /// 물(대양) 판정은 여기서 하지 않는다 — 최종 높이 기준으로 <see cref="HeightSynthesizer.BiomeAt"/>가 합성.
    /// 좌표만으로 결정되며 저장이 필요 없다(같은 Config·시드 → 같은 결과).
    /// </summary>
    public sealed class BiomeClassifier
    {
        private readonly NoiseField _elevation;
        private readonly NoiseField _temperature;
        private readonly NoiseField _moisture;
        private readonly BiomeClassifierSettings _settings;

        /// <summary>Config의 노이즈 레이어·분류 설정으로 분류기를 만든다.</summary>
        public BiomeClassifier(WorldGenConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            // 대륙도(Continentalness) 레이어는 P1 분류에 미사용(대양은 높이 기반). 향후 전체 월드용으로 enum/Config에 보존.
            _elevation = NoiseField.Create(config, EFieldLayer.Elevation);
            _temperature = NoiseField.Create(config, EFieldLayer.Temperature);
            _moisture = NoiseField.Create(config, EFieldLayer.Moisture);
            _settings = config.Biome;
        }

        /// <summary>전역 월드좌표 (worldX, worldZ)의 육지 4종 기후 블렌드. 합은 1(ocean=0).</summary>
        public BiomeWeights LandBlendAt(float worldX, float worldZ)
        {
            float e = _elevation.Sample(worldX, worldZ);
            float t = _temperature.Sample(worldX, worldZ);
            float m = _moisture.Sample(worldX, worldZ);

            float wGrass = ClimateWeight(_settings.Grassland, e, t, m);
            float wSnow = ClimateWeight(_settings.SnowMountain, e, t, m);
            float wDesert = ClimateWeight(_settings.Desert, e, t, m);
            float wRocky = ClimateWeight(_settings.Rocky, e, t, m);

            // 가우시안 가중치라 합은 항상 양수. 1로 정규화한다.
            float scale = 1f / (wGrass + wSnow + wDesert + wRocky);
            return new BiomeWeights(wGrass * scale, wSnow * scale, wDesert * scale, wRocky * scale, 0f);
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
    }
}
