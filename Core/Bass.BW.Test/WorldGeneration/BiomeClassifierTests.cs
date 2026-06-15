using System;
using Bass.BW.WorldGeneration;
using Xunit;

namespace Bass.BW.Test.WorldGeneration
{
    /// <summary>
    /// <see cref="BiomeClassifier"/> 결정성·정규화·바다/육지 분기·연속성 검증.
    /// </summary>
    public sealed class BiomeClassifierTests
    {
        [Fact]
        public void Ctor_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new BiomeClassifier(null));
        }

        [Fact]
        public void BiomeAt_SameCoord_IsDeterministic()
        {
            var a = new BiomeClassifier(new WorldGenConfig { WorldSeed = 99u });
            var b = new BiomeClassifier(new WorldGenConfig { WorldSeed = 99u });
            Assert.Equal(a.BiomeAt(10f, 20f).Grassland, b.BiomeAt(10f, 20f).Grassland);
        }

        [Fact]
        public void BiomeAt_WeightsSumToOne()
        {
            var classifier = new BiomeClassifier(new WorldGenConfig { WorldSeed = 7u });
            for (int i = 0; i < 500; i++)
            {
                float sum = classifier.BiomeAt(i * 11.3f, i * -5.7f).Sum;
                Assert.InRange(sum, 0.999f, 1.001f);
            }
        }

        [Fact]
        public void BiomeAt_WeightsAreNonNegative()
        {
            var classifier = new BiomeClassifier(new WorldGenConfig { WorldSeed = 3u });
            for (int i = 0; i < 500; i++)
            {
                var w = classifier.BiomeAt(i * 9.1f, i * 4.2f);
                Assert.True(w.Grassland >= 0f && w.SnowMountain >= 0f && w.Desert >= 0f
                            && w.Rocky >= 0f && w.Ocean >= 0f);
            }
        }

        [Fact]
        public void BiomeAt_LowContinentalnessBand_IsFullOcean()
        {
            // CoastStart/End를 1 위로 밀면 대륙도(≤1)는 항상 전이대 아래 → 전부 바다.
            var config = new WorldGenConfig { WorldSeed = 1u };
            config.Biome.CoastStart = 1.1f;
            config.Biome.CoastEnd = 1.2f;
            var classifier = new BiomeClassifier(config);

            var w = classifier.BiomeAt(123f, 456f);
            Assert.Equal(1f, w.Ocean, 3);
            Assert.Equal(EBiomeId.Ocean, w.Dominant);
        }

        [Fact]
        public void BiomeAt_HighContinentalnessBand_IsFullLand()
        {
            // 전이대를 0 아래로 밀면 대륙도(≥0)는 항상 전이대 위 → 바다 0, 육지 가중치 합 1.
            var config = new WorldGenConfig { WorldSeed = 1u };
            config.Biome.CoastStart = -0.2f;
            config.Biome.CoastEnd = -0.1f;
            var classifier = new BiomeClassifier(config);

            var w = classifier.BiomeAt(123f, 456f);
            Assert.Equal(0f, w.Ocean, 3);
            float landSum = w.Grassland + w.SnowMountain + w.Desert + w.Rocky;
            Assert.Equal(1f, landSum, 3);
        }

        [Fact]
        public void BiomeAt_DifferentWorldSeed_Diverges()
        {
            var a = new BiomeClassifier(new WorldGenConfig { WorldSeed = 1u });
            var b = new BiomeClassifier(new WorldGenConfig { WorldSeed = 2u });
            Assert.NotEqual(a.BiomeAt(40f, 40f).Grassland, b.BiomeAt(40f, 40f).Grassland);
        }
    }
}
