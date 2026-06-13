/// <summary>
/// Project : Easy Build System
/// Class : SocketSnapResult.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets.Data
{
    public struct SocketSnapResult
    {
        public bool IsValid;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;

        public SocketSnapResult(
            bool isValid,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale)
        {
            IsValid = isValid;
            Position = position;
            Rotation = rotation;
            Scale = scale;
            LocalPosition = localPosition;
            LocalRotation = localRotation;
            LocalScale = localScale;
        }

        public static readonly SocketSnapResult Invalid =
            new SocketSnapResult(
                false,
                Vector3.zero,
                Quaternion.identity,
                Vector3.one,
                Vector3.zero,
                Quaternion.identity,
                Vector3.one);
    }
}