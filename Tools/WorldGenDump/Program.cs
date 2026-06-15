using System;
using System.Collections.Generic;
using System.IO;
using Bass.BW.WorldGeneration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Bass.BW.WorldGenDump
{
    /// <summary>
    /// 엔진 없이 월드 생성 코어(T4~T6)를 그리드 샘플해 하이트맵/바이옴맵 PNG로 덤프한다.<br/>
    /// "절차 생성 작동" 1차 증명용. 사용: <c>dotnet run -- [출력폴더] [시드...]</c>
    /// </summary>
    internal static class Program
    {
        private const int ImageSize = 512;
        private const float WorldSize = 128f; // P1 단일 존
        private const float OriginX = 0f;
        private const float OriginZ = 0f;

        private static int Main(string[] args)
        {
            string outDir = args.Length > 0 ? args[0] : "dump-out";
            uint[] seeds = ParseSeeds(args);

            Directory.CreateDirectory(outDir);

            foreach (uint seed in seeds)
            {
                var config = new WorldGenConfig { WorldSeed = seed };
                var height = new HeightSynthesizer(config);

                RenderSeed(outDir, seed, height);
                Console.WriteLine($"seed {seed}: height + biome PNG 저장");
            }

            Console.WriteLine($"완료 → {Path.GetFullPath(outDir)}");
            return 0;
        }

        private static uint[] ParseSeeds(string[] args)
        {
            if (args.Length <= 1)
            {
                return new uint[] { 1u, 2u, 3u };
            }

            var list = new List<uint>();
            for (int i = 1; i < args.Length; i++)
            {
                if (uint.TryParse(args[i], out uint s))
                {
                    list.Add(s);
                }
            }
            return list.Count > 0 ? list.ToArray() : new uint[] { 1u };
        }

        private static void RenderSeed(string outDir, uint seed, HeightSynthesizer height)
        {
            using var heightImg = new Image<Rgba32>(ImageSize, ImageSize);
            using var biomeImg = new Image<Rgba32>(ImageSize, ImageSize);
            float step = WorldSize / (ImageSize - 1);

            for (int py = 0; py < ImageSize; py++)
            {
                float worldZ = OriginZ + py * step;
                for (int px = 0; px < ImageSize; px++)
                {
                    float worldX = OriginX + px * step;

                    byte g = (byte)(height.HeightAt(worldX, worldZ) * 255f);
                    heightImg[px, py] = new Rgba32(g, g, g, 255);

                    biomeImg[px, py] = BiomeColor(height.BiomeAt(worldX, worldZ));
                }
            }

            heightImg.SaveAsPng(Path.Combine(outDir, $"height_seed{seed}.png"));
            biomeImg.SaveAsPng(Path.Combine(outDir, $"biome_seed{seed}.png"));
        }

        /// <summary>바이옴 가중치로 대표색을 선형 혼합한다(연속 블렌딩 확인용).</summary>
        private static Rgba32 BiomeColor(BiomeWeights w)
        {
            float r = w.Grassland * 80f + w.SnowMountain * 235f + w.Desert * 220f + w.Rocky * 130f + w.Ocean * 40f;
            float g = w.Grassland * 160f + w.SnowMountain * 240f + w.Desert * 200f + w.Rocky * 125f + w.Ocean * 90f;
            float b = w.Grassland * 60f + w.SnowMountain * 245f + w.Desert * 120f + w.Rocky * 120f + w.Ocean * 160f;
            return new Rgba32(ToByte(r), ToByte(g), ToByte(b), 255);
        }

        private static byte ToByte(float v) => (byte)Math.Clamp(v, 0f, 255f);
    }
}
