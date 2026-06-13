/// <summary>
/// Project : Mind Code Interactive
/// Class : CompressionExtensions.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.IO;
using System.IO.Compression;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions
{
    public static class CompressionExtensions
    {
        public static byte[] Compress(this byte[] data)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress))
                {
                    gzip.Write(data, 0, data.Length);
                }

                return output.ToArray();
            }
        }

        public static byte[] Decompress(this byte[] data)
        {
            using (MemoryStream input = new MemoryStream(data))
            using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
            using (MemoryStream output = new MemoryStream())
            {
                gzip.CopyTo(output);
                return output.ToArray();
            }
        }
    }
}