namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// 한 육지 바이옴의 기후 이상점 — (고도, 온도, 습도) 공간에서 그 바이옴이 "사는" 좌표.<br/>
    /// 모두 [0,1]. <see cref="BiomeClassifier"/>가 샘플값과의 거리로 연속 가중치를 만든다.
    /// </summary>
    public readonly struct BiomeClimate
    {
        /// <summary>이상 고도.</summary>
        public readonly float Elevation;

        /// <summary>이상 온도.</summary>
        public readonly float Temperature;

        /// <summary>이상 습도.</summary>
        public readonly float Moisture;

        /// <summary>(고도, 온도, 습도) 이상점으로 생성한다.</summary>
        public BiomeClimate(float elevation, float temperature, float moisture)
        {
            Elevation = elevation;
            Temperature = temperature;
            Moisture = moisture;
        }
    }
}
