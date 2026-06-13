/// <summary>
/// Project : Mind Code Interactive
/// Class : UniqueIdGenerator.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject.Interfaces;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject
{
    public class UniqueIdGenerator : IUniqueIdGenerator
    {
        private const int SEGMENT_LENGTH = 4;
        private const int SEGMENT_COUNT = 3;

        public string GenerateNewId()
        {
            string rawGuid = Guid.NewGuid().ToString("N").ToLowerInvariant();
            string trimmedId = rawGuid.Substring(0, SEGMENT_LENGTH * SEGMENT_COUNT);

            return string.Join("-",
                trimmedId.Substring(0, SEGMENT_LENGTH),
                trimmedId.Substring(SEGMENT_LENGTH, SEGMENT_LENGTH),
                trimmedId.Substring(SEGMENT_LENGTH * 2, SEGMENT_LENGTH));
        }
    }
}