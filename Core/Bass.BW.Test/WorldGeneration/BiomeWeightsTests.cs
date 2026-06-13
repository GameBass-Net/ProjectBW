using Bass.BW.WorldGeneration;
using Xunit;

namespace Bass.BW.Test.WorldGeneration
{
    /// <summary>
    /// <see cref="BiomeWeights"/> 인덱서·지배 바이옴·합 검증.
    /// </summary>
    public sealed class BiomeWeightsTests
    {
        [Fact]
        public void Indexer_ReturnsMatchingFieldPerBiome()
        {
            var w = new BiomeWeights(0.1f, 0.2f, 0.3f, 0.15f, 0.25f);
            Assert.Equal(0.1f, w[EBiomeId.Grassland]);
            Assert.Equal(0.2f, w[EBiomeId.SnowMountain]);
            Assert.Equal(0.3f, w[EBiomeId.Desert]);
            Assert.Equal(0.15f, w[EBiomeId.Rocky]);
            Assert.Equal(0.25f, w[EBiomeId.Ocean]);
        }

        [Fact]
        public void Dominant_ReturnsHighestWeightedBiome()
        {
            var w = new BiomeWeights(0.1f, 0.2f, 0.5f, 0.1f, 0.1f);
            Assert.Equal(EBiomeId.Desert, w.Dominant);
        }

        [Fact]
        public void Dominant_OceanWins_WhenOceanHighest()
        {
            var w = new BiomeWeights(0f, 0f, 0f, 0.2f, 0.8f);
            Assert.Equal(EBiomeId.Ocean, w.Dominant);
        }

        [Fact]
        public void Sum_AddsAllWeights()
        {
            var w = new BiomeWeights(0.1f, 0.2f, 0.3f, 0.15f, 0.25f);
            Assert.Equal(1.0f, w.Sum, 5);
        }
    }
}
