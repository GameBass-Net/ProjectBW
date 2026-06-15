using System;
using System.Linq;
using Bass.BW.WorldGeneration;
using Xunit;

namespace Bass.BW.Test.WorldGeneration
{
    /// <summary>
    /// <see cref="NoiseField"/> 결정성·범위·시드 파생 검증.
    /// </summary>
    public sealed class NoiseFieldTests
    {
        private static NoiseField MakeField(uint seed)
            => new NoiseField(new NoiseLayerSettings(), seed);

        [Fact]
        public void Sample_SameSeedAndCoord_IsDeterministic()
        {
            var a = MakeField(123u);
            var b = MakeField(123u);
            Assert.Equal(a.Sample(12.5f, -7.25f), b.Sample(12.5f, -7.25f));
        }

        [Fact]
        public void Sample_StaysInUnitRange()
        {
            var field = MakeField(42u);
            for (int i = 0; i < 1000; i++)
            {
                float v = field.Sample(i * 13.7f, i * -9.1f);
                Assert.InRange(v, 0f, 1f);
            }
        }

        [Fact]
        public void Sample_DifferentSeed_DivergesAtSameCoord()
        {
            var a = MakeField(1u);
            var b = MakeField(2u);
            Assert.NotEqual(a.Sample(50f, 50f), b.Sample(50f, 50f));
        }

        [Fact]
        public void Sample_DifferentCoord_Varies()
        {
            var field = MakeField(7u);
            Assert.NotEqual(field.Sample(0f, 0f), field.Sample(500f, 500f));
        }

        [Fact]
        public void DeriveLayerSeed_IsDeterministic()
        {
            Assert.Equal(
                NoiseField.DeriveLayerSeed(1000u, EFieldLayer.Temperature),
                NoiseField.DeriveLayerSeed(1000u, EFieldLayer.Temperature));
        }

        [Fact]
        public void DeriveLayerSeed_DifferentLayers_Differ()
        {
            uint cont = NoiseField.DeriveLayerSeed(1000u, EFieldLayer.Continentalness);
            uint elev = NoiseField.DeriveLayerSeed(1000u, EFieldLayer.Elevation);
            uint temp = NoiseField.DeriveLayerSeed(1000u, EFieldLayer.Temperature);
            uint moist = NoiseField.DeriveLayerSeed(1000u, EFieldLayer.Moisture);
            Assert.Equal(4, new[] { cont, elev, temp, moist }.Distinct().Count());
        }

        [Fact]
        public void DeriveLayerSeed_DifferentWorldSeed_Differs()
        {
            Assert.NotEqual(
                NoiseField.DeriveLayerSeed(1u, EFieldLayer.Elevation),
                NoiseField.DeriveLayerSeed(2u, EFieldLayer.Elevation));
        }

        [Fact]
        public void Ctor_NullSettings_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new NoiseField(null, 1u));
        }

        [Fact]
        public void Create_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => NoiseField.Create(null, EFieldLayer.Elevation));
        }

        [Fact]
        public void Config_NoiseLayers_CoversEveryLayer()
        {
            var config = new WorldGenConfig();
            int layerCount = Enum.GetValues(typeof(EFieldLayer)).Length;
            Assert.Equal(layerCount, config.NoiseLayers.Length);
        }

        [Fact]
        public void Create_FromConfig_UsesDerivedLayerSeed()
        {
            var config = new WorldGenConfig { WorldSeed = 555u };
            var fromConfig = NoiseField.Create(config, EFieldLayer.Moisture);
            uint expectedSeed = NoiseField.DeriveLayerSeed(555u, EFieldLayer.Moisture);
            var expected = new NoiseField(config.NoiseLayers[(int)EFieldLayer.Moisture], expectedSeed);

            Assert.Equal(expected.Sample(33f, 44f), fromConfig.Sample(33f, 44f));
        }
    }
}
