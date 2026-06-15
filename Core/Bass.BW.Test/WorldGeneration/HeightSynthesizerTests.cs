using System;
using Bass.BW.WorldGeneration;
using Xunit;

namespace Bass.BW.Test.WorldGeneration
{
    /// <summary>
    /// <see cref="HeightSynthesizer"/> 결정성·범위·격자·섬 마스크 검증.
    /// </summary>
    public sealed class HeightSynthesizerTests
    {
        [Fact]
        public void Ctor_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new HeightSynthesizer(null));
        }

        [Fact]
        public void GenerateHeightField_HasRequestedDimensions()
        {
            var synth = new HeightSynthesizer(new WorldGenConfig { WorldSeed = 1u });
            var field = synth.GenerateHeightField(0f, 0f, 128f, 65);
            Assert.Equal(65, field.Width);
            Assert.Equal(65, field.Height);
        }

        [Fact]
        public void GenerateHeightField_ValuesStayInUnitRange()
        {
            var synth = new HeightSynthesizer(new WorldGenConfig { WorldSeed = 5u });
            var field = synth.GenerateHeightField(0f, 0f, 128f, 33);
            foreach (float v in field.Values)
            {
                Assert.InRange(v, 0f, 1f);
            }
        }

        [Fact]
        public void GenerateHeightField_SingleSample_NoDivideByZero()
        {
            var synth = new HeightSynthesizer(new WorldGenConfig { WorldSeed = 1u });
            var field = synth.GenerateHeightField(10f, 10f, 128f, 1);
            Assert.Equal(1, field.Width);
            Assert.InRange(field[0, 0], 0f, 1f);
        }

        [Fact]
        public void HeightAt_SameCoord_IsDeterministic()
        {
            var a = new HeightSynthesizer(new WorldGenConfig { WorldSeed = 42u });
            var b = new HeightSynthesizer(new WorldGenConfig { WorldSeed = 42u });
            Assert.Equal(a.HeightAt(30f, 40f), b.HeightAt(30f, 40f));
        }

        [Fact]
        public void HeightAt_BeyondIslandFalloff_IsZero()
        {
            // 기본 섬 중심(64,64)·감쇠끝 64에서 멀리 떨어진 점 → 마스크 0 → 높이 0.
            var synth = new HeightSynthesizer(new WorldGenConfig { WorldSeed = 9u });
            Assert.Equal(0f, synth.HeightAt(10000f, 10000f));
        }

        [Fact]
        public void HeightAt_DifferentWorldSeed_Diverges()
        {
            // 마스크 1 영역(섬 중심)에서 시드만 다르면 베이스 고도가 달라 높이도 갈린다.
            var a = new HeightSynthesizer(new WorldGenConfig { WorldSeed = 1u });
            var b = new HeightSynthesizer(new WorldGenConfig { WorldSeed = 2u });
            Assert.NotEqual(a.HeightAt(64f, 64f), b.HeightAt(64f, 64f));
        }
    }
}
