/// <summary>
/// Project : Easy Build System
/// Class : GridHelper.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Helpers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Helpers
{
    public static class GridHelper
    {
        public static bool PointInPolygon(Vector2 point, Vector2[] polygon)
        {
            bool isInside = false;
            int previousVertexIndex = polygon.Length - 1;

            for (int i = 0; i < polygon.Length; i++)
            {
                if ((polygon[i].y > point.y) != (polygon[previousVertexIndex].y > point.y) &&
                    point.x < (polygon[previousVertexIndex].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[previousVertexIndex].y - polygon[i].y) + polygon[i].x)
                {
                    isInside = !isInside;
                }

                previousVertexIndex = i;
            }

            return isInside;
        }

        public static HashSet<Vector2Int> GetOccupancyCells(GridStageData stage,
            Vector3 position, Quaternion rotation,
            float cellCountX, float cellCountZ, Transform gridTransform)
        {
            HashSet<Vector2Int> occupancyCells = new HashSet<Vector2Int>();

            float cellSize = stage.CellSize;

            Vector3 localPos = gridTransform.InverseTransformPoint(position);
            Vector3 pivotOffset = GetCenterPivotOffset(stage);
            localPos.x -= pivotOffset.x;
            localPos.z -= pivotOffset.z;

            Vector2 occupancyCenter = new Vector2(localPos.x / cellSize, localPos.z / cellSize);
            float occupancyAngle = -rotation.eulerAngles.y * Mathf.Deg2Rad;

            Vector2[] rotatedCorners = GetRotatedCorners(occupancyCenter, cellCountX, cellCountZ, occupancyAngle);

            GetMinMaxBounds(rotatedCorners, out float minBoundX, out float maxBoundX, out float minBoundZ, out float maxBoundZ);

            int startX = Mathf.FloorToInt(minBoundX);
            int endX = Mathf.CeilToInt(maxBoundX);
            int startZ = Mathf.FloorToInt(minBoundZ);
            int endZ = Mathf.CeilToInt(maxBoundZ);

            for (int x = startX; x < endX; x++)
            {
                for (int z = startZ; z < endZ; z++)
                {
                    Vector2 cellCenter = new Vector2(x + 0.5f, z + 0.5f);

                    if (PointInPolygon(cellCenter, rotatedCorners))
                    {
                        occupancyCells.Add(new Vector2Int(x, z));
                    }
                }
            }

            return occupancyCells;
        }

        public static Vector3 GetCenterPivotOffset(GridStageData stage)
        {
            float gridWidth = stage.Columns * stage.CellSize;
            float gridHeight = stage.Rows * stage.CellSize;
            return new Vector3(-gridWidth / 2f, 0f, -gridHeight / 2f);
        }

        public static void GetMinMaxBounds(Vector2[] corners, out float minX, out float maxX, out float minZ, out float maxZ)
        {
            minX = float.MaxValue;
            maxX = float.MinValue;
            minZ = float.MaxValue;
            maxZ = float.MinValue;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 corner = corners[i];
                minX = Mathf.Min(minX, corner.x);
                maxX = Mathf.Max(maxX, corner.x);
                minZ = Mathf.Min(minZ, corner.y);
                maxZ = Mathf.Max(maxZ, corner.y);
            }
        }

        public static Vector2[] GetRotatedCorners(Vector2 center, float width, float height, float angleRad)
        {
            Vector2[] corners = new Vector2[4];

            float cosAngle = Mathf.Cos(angleRad);
            float sinAngle = Mathf.Sin(angleRad);
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            corners[0] = center + new Vector2(-halfWidth * cosAngle + halfHeight * sinAngle, -halfWidth * sinAngle - halfHeight * cosAngle);
            corners[1] = center + new Vector2(halfWidth * cosAngle + halfHeight * sinAngle, halfWidth * sinAngle - halfHeight * cosAngle);
            corners[2] = center + new Vector2(halfWidth * cosAngle - halfHeight * sinAngle, halfWidth * sinAngle + halfHeight * cosAngle);
            corners[3] = center + new Vector2(-halfWidth * cosAngle - halfHeight * sinAngle, -halfWidth * sinAngle + halfHeight * cosAngle);

            return corners;
        }

        public static bool IsInsideGridBounds(int x, int z, int rows, int cols)
        {
            return x >= 0 && z >= 0 && x < rows && z < cols;
        }
    }
}