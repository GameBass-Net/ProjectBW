using System;

namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// 전역좌표 기반 높이 합성기(T6, 접근 A).<br/>
    /// 베이스 고도(elevation 노이즈) → 바이옴 가중 셰이핑 → P1 섬 마스크(반경 감쇠) → 최종 [0,1] 높이.<br/>
    /// 대양 재판정(높이&lt;SeaLevel)은 후보정으로 미룸 — 여기선 마스크가 가장자리 높이만 낮춘다.
    /// </summary>
    public sealed class HeightSynthesizer
    {
        private readonly NoiseField _elevation;
        private readonly BiomeClassifier _biomes;
        private readonly HeightSynthesisSettings _settings;

        /// <summary>Config의 elevation 레이어·바이옴 분류·높이 설정으로 합성기를 만든다.</summary>
        public HeightSynthesizer(WorldGenConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            _elevation = NoiseField.Create(config, EFieldLayer.Elevation);
            _biomes = new BiomeClassifier(config);
            _settings = config.Height;
        }

        /// <summary>전역 월드좌표 (worldX, worldZ)의 최종 정규화 높이 [0,1].</summary>
        public float HeightAt(float worldX, float worldZ)
        {
            float baseHeight = _elevation.Sample(worldX, worldZ);

            BiomeWeights w = _biomes.BiomeAt(worldX, worldZ);
            float shaping =
                w.Grassland * _settings.GrasslandHeightBias +
                w.SnowMountain * _settings.SnowMountainHeightBias +
                w.Desert * _settings.DesertHeightBias +
                w.Rocky * _settings.RockyHeightBias;

            float h = (baseHeight + shaping) * IslandMask(worldX, worldZ);
            return Math.Clamp(h, 0f, 1f);
        }

        /// <summary>
        /// (originX, originZ)를 한 모서리로 하는 한 변 <paramref name="worldSize"/>(미터) 정사각 영역을
        /// <paramref name="resolution"/>×<paramref name="resolution"/> 그리드로 샘플해 높이 필드를 만든다.
        /// </summary>
        public HeightField GenerateHeightField(float originX, float originZ, float worldSize, int resolution)
        {
            var field = new HeightField(resolution, resolution);
            float step = resolution > 1 ? worldSize / (resolution - 1) : 0f;

            for (int y = 0; y < resolution; y++)
            {
                float worldZ = originZ + y * step;
                for (int x = 0; x < resolution; x++)
                {
                    float worldX = originX + x * step;
                    field[x, y] = HeightAt(worldX, worldZ);
                }
            }

            return field;
        }

        /// <summary>섬 마스크 [0,1]. 중심 근처 1, 감쇠 끝 너머 0.</summary>
        private float IslandMask(float worldX, float worldZ)
        {
            float dx = worldX - _settings.IslandCenterX;
            float dz = worldZ - _settings.IslandCenterZ;
            float dist = (float)Math.Sqrt(dx * dx + dz * dz);
            return 1f - SmoothStep(_settings.IslandFalloffStart, _settings.IslandFalloffEnd, dist);
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
