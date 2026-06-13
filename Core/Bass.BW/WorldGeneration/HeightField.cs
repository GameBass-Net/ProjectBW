using System;

namespace Bass.BW.WorldGeneration
{
    /// <summary>
    /// 정규화된 높이값 2D 그리드. 값 범위는 [0, 1]이며, 절대 높이는 <c>value * WorldGenConfig.MaxHeight</c>(미터)로 복원한다.<br/>
    /// row-major(인덱스 = y * Width + x)로 저장한다. 엔진 비종속(순수 C#)이며,
    /// Unity 측은 이 데이터를 받아 <c>TerrainData.SetHeights</c>용 <c>float[,]</c>로 변환한다.<br/>
    /// 생성 단계에서 채워지는 버퍼이므로 가변(읽기/쓰기)이다.
    /// </summary>
    public sealed class HeightField
    {
        /// <summary>가로 샘플 수.</summary>
        public int Width { get; }

        /// <summary>세로 샘플 수.</summary>
        public int Height { get; }

        private readonly float[] _values;

        /// <summary>지정 크기의 0으로 초기화된 높이 그리드를 만든다.</summary>
        /// <param name="width">가로 샘플 수(1 이상).</param>
        /// <param name="height">세로 샘플 수(1 이상).</param>
        public HeightField(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "width는 1 이상이어야 한다.");
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "height는 1 이상이어야 한다.");
            }
            Width = width;
            Height = height;
            _values = new float[width * height];
        }

        /// <summary>(x, y) 위치의 정규화 높이값[0,1]에 접근한다.</summary>
        public float this[int x, int y]
        {
            get => _values[(y * Width) + x];
            set => _values[(y * Width) + x] = value;
        }

        /// <summary>내부 row-major 버퍼(읽기 전용 참조 제공). Unity 변환·복사용.</summary>
        public float[] Values => _values;
    }
}
