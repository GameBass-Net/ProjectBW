using System;
using Bass.Core;

namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// 전역 월드좌표를 입력받는 단일 노이즈 필드 래퍼(레이어 무관, 설정+시드로 분화).<br/>
    /// <see cref="Sample"/>은 전역 좌표의 순수 함수라 존 경계가 자동으로 이어진다(C3).
    /// 출력은 [0,1]로 정규화한다.
    /// </summary>
    public sealed class NoiseField
    {
        private readonly FastNoiseLite _noise;

        /// <summary>설정과 (이미 파생된) 시드로 필드를 만든다.</summary>
        public NoiseField(NoiseLayerSettings settings, uint seed)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _noise = new FastNoiseLite(unchecked((int)seed));
            _noise.SetNoiseType(settings.NoiseType);
            _noise.SetFrequency(settings.Frequency);
            _noise.SetFractalType(settings.FractalType);
            _noise.SetFractalOctaves(settings.FractalOctaves);
            _noise.SetFractalLacunarity(settings.FractalLacunarity);
            _noise.SetFractalGain(settings.FractalGain);
            _noise.SetFractalWeightedStrength(settings.FractalWeightedStrength);
            _noise.SetFractalPingPongStrength(settings.FractalPingPongStrength);
        }

        /// <summary>전역 월드좌표 (worldX, worldZ)에서 평가한 값. [0,1] 정규화.</summary>
        public float Sample(float worldX, float worldZ)
        {
            // FastNoiseLite는 [-1,1] 범위를 반환한다. [0,1]로 옮기고 안전하게 클램프.
            float v = _noise.GetNoise(worldX, worldZ) * 0.5f + 0.5f;
            return Math.Clamp(v, 0f, 1f);
        }

        /// <summary>
        /// 월드 시드 하나와 레이어로부터 레이어 시드를 결정적으로 파생한다(순수 함수).<br/>
        /// 레이어마다 MT를 한 번 돌려 시드를 흩뿌린다. 레이어값은 명시 상수라
        /// 레이어 셋을 늘려도 기존 레이어의 시드는 변하지 않는다.
        /// </summary>
        public static uint DeriveLayerSeed(uint worldSeed, EFieldLayer layer)
            => new MT19937(worldSeed + (uint)layer).NextUInt();

        /// <summary>Config의 레이어 설정과 월드 시드로부터 필드를 만든다.</summary>
        public static NoiseField Create(WorldGenConfig config, EFieldLayer layer)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            NoiseLayerSettings settings = config.NoiseLayers[(int)layer];
            uint seed = DeriveLayerSeed(config.WorldSeed, layer);
            return new NoiseField(settings, seed);
        }
    }
}
