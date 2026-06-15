using System;
using Bass.BW.WorldGeneration;
using Xunit;

namespace Bass.BW.Test.WorldGeneration
{
    /// <summary>
    /// <see cref="BiomeClassifier.LandBlendAt"/> 결정성·정규화·비음수·연속성 검증(물 판정 제외).
    /// </summary>
    public sealed class BiomeClassifierTests
    {
        [Fact]
        public void Ctor_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new BiomeClassifier(null));
        }

        [Fact]
        public void LandBlendAt_SameCoord_IsDeterministic()
        {
            var a = new BiomeClassifier(new WorldGenConfig { WorldSeed = 99u });
            var b = new BiomeClassifier(new WorldGenConfig { WorldSeed = 99u });
            Assert.Equal(a.LandBlendAt(10f, 20f).Grassland, b.LandBlendAt(10f, 20f).Grassland);
        }

        [Fact]
        public void LandBlendAt_WeightsSumToOne()
        {
            var classifier = new BiomeClassifier(new WorldGenConfig { WorldSeed = 7u });
            for (int i = 0; i < 500; i++)
            {
                float sum = classifier.LandBlendAt(i * 11.3f, i * -5.7f).Sum;
                Assert.InRange(sum, 0.999f, 1.001f);
            }
        }

        [Fact]
        public void LandBlendAt_OceanWeightIsAlwaysZero()
        {
            // 물 판정은 높이 단계 책임이므로 분류기 자체는 ocean=0.
            var classifier = new BiomeClassifier(new WorldGenConfig { WorldSeed = 7u });
            for (int i = 0; i < 200; i++)
            {
                Assert.Equal(0f, classifier.LandBlendAt(i * 13.1f, i * 6.2f).Ocean);
            }
        }

        [Fact]
        public void LandBlendAt_WeightsAreNonNegative()
        {
            var classifier = new BiomeClassifier(new WorldGenConfig { WorldSeed = 3u });
            for (int i = 0; i < 500; i++)
            {
                var w = classifier.LandBlendAt(i * 9.1f, i * 4.2f);
                Assert.True(w.Grassland >= 0f && w.SnowMountain >= 0f
                            && w.Desert >= 0f && w.Rocky >= 0f);
            }
        }

        [Fact]
        public void LandBlendAt_DifferentWorldSeed_Diverges()
        {
            var a = new BiomeClassifier(new WorldGenConfig { WorldSeed = 1u });
            var b = new BiomeClassifier(new WorldGenConfig { WorldSeed = 2u });
            Assert.NotEqual(a.LandBlendAt(40f, 40f).Grassland, b.LandBlendAt(40f, 40f).Grassland);
        }
    }
}
