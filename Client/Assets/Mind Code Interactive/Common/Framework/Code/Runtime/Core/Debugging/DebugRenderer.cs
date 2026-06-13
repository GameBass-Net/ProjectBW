/// <summary>
/// Project : Mind Code Interactive
/// Class : DebugRenderer.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging
{
    public static class DebugRenderer
    {
        [Flags]
        public enum ViewFlags
        {
            None = 0,
            SceneView = 1 << 0,
            GameView = 1 << 1,
            All = SceneView | GameView
        }

        private struct LineEntry
        {
            public Vector3 Start;
            public Vector3 End;
            public Color Color;
            public float Thickness;
        }

        private struct SolidEntry
        {
            public bool IsSphere;
            public Vector3 Center;
            public Vector3 Size;
            public Quaternion Rotation;
            public Color Color;
        }

        private struct MeshEntry
        {
            public Mesh Mesh;
            public Vector3[] CachedVertices;
            public int[] CachedTriangles;
            public Color FillColor;
            public Color WireColor;
            public Vector3[] WireVertices;
        }

        private static readonly List<LineEntry> s_lines = new List<LineEntry>(1024);
        private static readonly List<SolidEntry> s_solids = new List<SolidEntry>(256);
        private static readonly List<MeshEntry> s_meshes = new List<MeshEntry>(16);

        private static Mesh s_cubeMesh;
        private static Mesh s_sphereMesh;

        private static Vector3[] s_cubeVertices;
        private static int[] s_cubeTriangles;
        private static Vector3[] s_sphereVertices;
        private static int[] s_sphereTriangles;

        private static readonly Vector3[] s_wireCubeBaseVerts = new Vector3[8];
        private static readonly Vector3[] s_wireCubeWorldVerts = new Vector3[8];

        private static Material s_solidMaterial;
        private static Material s_lineMaterial;

        private static float CurrentTime
        {
            get
            {
                if (Application.isPlaying)
                {
                    return Time.time;
                }

#if UNITY_EDITOR
                return (float)UnityEditor.EditorApplication.timeSinceStartup;
#else
                return 0f;
#endif
            }
        }

        internal static void InitMaterial()
        {
            bool needsRebuild = s_solidMaterial == null || s_lineMaterial == null
                || s_cubeMesh == null || s_sphereMesh == null;

            if (!needsRebuild)
            {
                return;
            }

            if (s_solidMaterial != null)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(s_solidMaterial);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(s_solidMaterial);
                }
                s_solidMaterial = null;
            }

            if (s_lineMaterial != null)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(s_lineMaterial);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(s_lineMaterial);
                }
                s_lineMaterial = null;
            }

            Shader hiddenColored = Shader.Find("Hidden/Internal-Colored");
            Shader fallback = hiddenColored != null ? hiddenColored : Shader.Find("Unlit/Color");

            s_solidMaterial = new Material(fallback) { hideFlags = HideFlags.HideAndDontSave };
            s_solidMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            s_solidMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            s_solidMaterial.SetInt("_Cull", (int)CullMode.Back);
            s_solidMaterial.SetInt("_ZWrite", 0);
            s_solidMaterial.SetInt("_ZTest", (int)CompareFunction.Always);
            s_solidMaterial.renderQueue = (int)RenderQueue.Transparent;

            s_lineMaterial = new Material(fallback) { hideFlags = HideFlags.HideAndDontSave };
            s_lineMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            s_lineMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            s_lineMaterial.SetInt("_Cull", (int)CullMode.Off);
            s_lineMaterial.SetInt("_ZWrite", 0);
            s_lineMaterial.SetInt("_ZTest", (int)CompareFunction.Always);

            BuildCubeMesh();
            BuildSphereMesh();
        }

        private static void BuildCubeMesh()
        {
            if (s_cubeMesh != null)
            {
                return;
            }

            s_cubeMesh = new Mesh { name = "DebugRenderer_Cube", hideFlags = HideFlags.HideAndDontSave };
            s_cubeVertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f), new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(-0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f), new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(-0.5f,  0.5f,  0.5f),
            };
            s_cubeTriangles = new int[]
            {
                0, 2, 1, 0, 3, 2,
                4, 5, 6, 4, 6, 7,
                0, 1, 5, 0, 5, 4,
                2, 3, 7, 2, 7, 6,
                1, 2, 6, 1, 6, 5,
                0, 4, 7, 0, 7, 3
            };
            s_cubeMesh.vertices = s_cubeVertices;
            s_cubeMesh.triangles = s_cubeTriangles;
            s_cubeMesh.RecalculateNormals();
            s_cubeMesh.RecalculateBounds();
        }

        private static void BuildSphereMesh()
        {
            if (s_sphereMesh != null)
            {
                return;
            }

            s_sphereMesh = new Mesh { name = "DebugRenderer_Sphere", hideFlags = HideFlags.HideAndDontSave };
            int latSegments = 12;
            int lonSegments = 16;
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float v = (float)lat / latSegments;
                float theta = v * Mathf.PI;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float u = (float)lon / lonSegments;
                    float phi = u * 2f * Mathf.PI;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);

                    verts.Add(new Vector3(cosPhi * sinTheta * 0.5f, cosTheta * 0.5f, sinPhi * sinTheta * 0.5f));
                }
            }

            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int lon = 0; lon < lonSegments; lon++)
                {
                    int first = lat * (lonSegments + 1) + lon;
                    int second = first + lonSegments + 1;

                    tris.Add(first);
                    tris.Add(second);
                    tris.Add(first + 1);

                    tris.Add(second);
                    tris.Add(second + 1);
                    tris.Add(first + 1);
                }
            }

            s_sphereVertices = verts.ToArray();
            s_sphereTriangles = tris.ToArray();
            s_sphereMesh.vertices = s_sphereVertices;
            s_sphereMesh.triangles = s_sphereTriangles;
            s_sphereMesh.RecalculateNormals();
            s_sphereMesh.RecalculateBounds();
        }

        internal static void ForceRebuildMaterials()
        {
            if (s_solidMaterial != null)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(s_solidMaterial);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(s_solidMaterial);
                }
                s_solidMaterial = null;
            }

            if (s_lineMaterial != null)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(s_lineMaterial);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(s_lineMaterial);
                }
                s_lineMaterial = null;
            }
        }

        internal static void ClearAll()
        {
            s_lines.Clear();
            s_solids.Clear();
            s_meshes.Clear();
        }

        internal static void RenderForCamera(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            InitMaterial();

            if (s_lines.Count == 0 && s_solids.Count == 0 && s_meshes.Count == 0)
            {
                return;
            }

            GL.PushMatrix();
            GL.LoadProjectionMatrix(camera.projectionMatrix);
            GL.modelview = camera.worldToCameraMatrix;

            DrawSolidsImmediate();
            DrawMeshesImmediate();
            DrawLinesImmediate();

            GL.PopMatrix();
        }

        private static void DrawSolidsImmediate()
        {
            if (s_solids.Count == 0)
            {
                return;
            }

            s_solidMaterial.SetPass(0);

            GL.Begin(GL.TRIANGLES);
            for (int i = 0; i < s_solids.Count; i++)
            {
                SolidEntry entry = s_solids[i];
                Vector3[] verts = entry.IsSphere ? s_sphereVertices : s_cubeVertices;
                int[] tris = entry.IsSphere ? s_sphereTriangles : s_cubeTriangles;
                Matrix4x4 trs = Matrix4x4.TRS(entry.Center, entry.Rotation, entry.Size);

                GL.Color(entry.Color);
                for (int t = 0; t < tris.Length; t++)
                {
                    GL.Vertex(trs.MultiplyPoint3x4(verts[tris[t]]));
                }
            }
            GL.End();
        }

        private static void DrawMeshesImmediate()
        {
            if (s_meshes.Count == 0)
            {
                return;
            }

            s_solidMaterial.SetPass(0);
            GL.Begin(GL.TRIANGLES);
            for (int i = 0; i < s_meshes.Count; i++)
            {
                MeshEntry entry = s_meshes[i];
                if (entry.CachedVertices == null || entry.CachedTriangles == null)
                {
                    continue;
                }
                GL.Color(entry.FillColor);
                for (int t = 0; t < entry.CachedTriangles.Length; t++)
                {
                    GL.Vertex(entry.CachedVertices[entry.CachedTriangles[t]]);
                }
            }
            GL.End();
        }

        private static void DrawLinesImmediate()
        {
            if (s_lines.Count == 0 && s_meshes.Count == 0)
            {
                return;
            }

            s_lineMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            for (int i = 0; i < s_lines.Count; i++)
            {
                LineEntry entry = s_lines[i];
                GL.Color(entry.Color);
                GL.Vertex(entry.Start);
                GL.Vertex(entry.End);
            }

            for (int i = 0; i < s_meshes.Count; i++)
            {
                MeshEntry entry = s_meshes[i];
                if (entry.WireVertices == null || entry.WireVertices.Length < 2)
                {
                    continue;
                }
                GL.Color(entry.WireColor);
                for (int v = 0; v < entry.WireVertices.Length - 1; v++)
                {
                    GL.Vertex(entry.WireVertices[v]);
                    GL.Vertex(entry.WireVertices[v + 1]);
                }
            }
            GL.End();
        }

        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f, float thickness = 1f, bool persistent = false)
        {
            s_lines.Add(new LineEntry { Start = start, End = end, Color = color, Thickness = thickness });
        }

        public static void DrawRay(Vector3 start, Vector3 direction, Color color, float length = 1f, float duration = 0f, float thickness = 1f, bool persistent = false)
        {
            DrawLine(start, start + direction.normalized * length, color, duration, thickness, persistent);
        }

        public static void DrawPolyLine(Color color, float duration = 0f, float thickness = 1f, bool persistent = false, params Vector3[] points)
        {
            if (points == null || points.Length < 2)
            {
                return;
            }
            for (int i = 0; i < points.Length - 1; i++)
            {
                DrawLine(points[i], points[i + 1], color, duration, thickness, persistent);
            }
        }

        public static void DrawWireCube(Vector3 center, Vector3 size, Color color, float duration = 0f, float thickness = 1f, bool persistent = false)
        {
            DrawWireCube(center, size, Quaternion.identity, Vector3.one, color, duration, thickness, persistent);
        }

        public static void DrawWireCube(Vector3 center, Vector3 size, Quaternion rotation, Color color, float duration = 0f, float thickness = 1f, bool persistent = false)
        {
            DrawWireCube(center, size, rotation, Vector3.one, color, duration, thickness, persistent);
        }

        public static void DrawWireCube(Vector3 center, Vector3 size, Quaternion rotation, Vector3 scale, Color color, float duration = 0f, float thickness = 1f, bool persistent = false)
        {
            Vector3 h = size * 0.5f;
            s_wireCubeBaseVerts[0] = new Vector3(-h.x, -h.y, -h.z); s_wireCubeBaseVerts[1] = new Vector3(h.x, -h.y, -h.z);
            s_wireCubeBaseVerts[2] = new Vector3(h.x, -h.y, h.z); s_wireCubeBaseVerts[3] = new Vector3(-h.x, -h.y, h.z);
            s_wireCubeBaseVerts[4] = new Vector3(-h.x, h.y, -h.z); s_wireCubeBaseVerts[5] = new Vector3(h.x, h.y, -h.z);
            s_wireCubeBaseVerts[6] = new Vector3(h.x, h.y, h.z); s_wireCubeBaseVerts[7] = new Vector3(-h.x, h.y, h.z);

            for (int i = 0; i < 8; i++)
            {
                s_wireCubeWorldVerts[i] = rotation * Vector3.Scale(s_wireCubeBaseVerts[i], scale) + center;
            }

            Vector3[] v = s_wireCubeWorldVerts;
            DrawLine(v[0], v[1], color, duration, thickness, persistent); DrawLine(v[1], v[2], color, duration, thickness, persistent);
            DrawLine(v[2], v[3], color, duration, thickness, persistent); DrawLine(v[3], v[0], color, duration, thickness, persistent);
            DrawLine(v[4], v[5], color, duration, thickness, persistent); DrawLine(v[5], v[6], color, duration, thickness, persistent);
            DrawLine(v[6], v[7], color, duration, thickness, persistent); DrawLine(v[7], v[4], color, duration, thickness, persistent);
            DrawLine(v[0], v[4], color, duration, thickness, persistent); DrawLine(v[1], v[5], color, duration, thickness, persistent);
            DrawLine(v[2], v[6], color, duration, thickness, persistent); DrawLine(v[3], v[7], color, duration, thickness, persistent);
        }

        public static void DrawCube(Vector3 center, Vector3 size, Color color, float duration = 0f, bool persistent = false)
        {
            DrawCube(center, size, Quaternion.identity, Vector3.one, color, duration, persistent);
        }

        public static void DrawCube(Vector3 center, Vector3 size, Quaternion rotation, Color color, float duration = 0f, bool persistent = false)
        {
            DrawCube(center, size, rotation, Vector3.one, color, duration, persistent);
        }

        public static void DrawCube(Vector3 center, Vector3 size, Quaternion rotation, Vector3 scale, Color color, float duration = 0f, bool persistent = false)
        {
            Vector3 finalSize = Vector3.Scale(size, scale);
            s_solids.Add(new SolidEntry
            {
                IsSphere = false,
                Center = center,
                Size = finalSize,
                Rotation = rotation,
                Color = color,
            });
        }

        public static void DrawWireSphere(Vector3 center, float radius, Color color, int segments = 24, float duration = 0f, float thickness = 1f, bool persistent = false)
        {
            if (segments < 3)
            {
                segments = 3;
            }

            float step = 360f / segments;
            for (int i = 0; i < segments; i++)
            {
                float a0 = Mathf.Deg2Rad * (i * step);
                float a1 = Mathf.Deg2Rad * ((i + 1) * step);
                DrawLine(center + new Vector3(Mathf.Cos(a0), Mathf.Sin(a0), 0f) * radius, center + new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0f) * radius, color, duration, thickness, persistent);
                DrawLine(center + new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * radius, center + new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * radius, color, duration, thickness, persistent);
                DrawLine(center + new Vector3(0f, Mathf.Cos(a0), Mathf.Sin(a0)) * radius, center + new Vector3(0f, Mathf.Cos(a1), Mathf.Sin(a1)) * radius, color, duration, thickness, persistent);
            }
        }

        public static void DrawSphere(Vector3 center, float radius, Color color, int segments = 8, float duration = 0f, bool persistent = false)
        {
            float diameter = radius * 2f;
            s_solids.Add(new SolidEntry
            {
                IsSphere = true,
                Center = center,
                Size = new Vector3(diameter, diameter, diameter),
                Rotation = Quaternion.identity,
                Color = color,
            });
        }

        public static void DrawWireDisc(Vector3 center, float radius, Color color, int segments = 32, float duration = 0f, float thickness = 1f, bool persistent = false)
        {
            DrawWireDisc(center, Vector3.up, radius, color, segments, duration, thickness, persistent);
        }

        public static void DrawWireDisc(Vector3 center, Vector3 normal, float radius, Color color, int segments = 32, float duration = 0f, float thickness = 1f, bool persistent = false)
        {
            if (radius <= 0f || segments < 3)
            {
                return;
            }

            normal = normal.sqrMagnitude < 1e-8f ? Vector3.up : normal.normalized;
            Vector3 refAxis = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) < 0.99f ? Vector3.up : Vector3.right;
            Vector3 tangent = Vector3.Normalize(Vector3.Cross(refAxis, normal));
            Vector3 bitangent = Vector3.Cross(normal, tangent);
            float step = 2f * Mathf.PI / segments;
            Vector3 prev = center + tangent * radius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = step * i;
                Vector3 curr = center + (Mathf.Cos(angle) * tangent + Mathf.Sin(angle) * bitangent) * radius;
                DrawLine(prev, curr, color, duration, thickness, persistent);
                prev = curr;
            }
        }

        public static void DrawFlatMesh(Vector3[] fillVertices, Vector3[] wireVertices, Color fillColor, Color wireColor, float duration = 0f, bool persistent = false)
        {
            if (fillVertices == null || fillVertices.Length < 3)
            {
                return;
            }

            int triCount = (fillVertices.Length - 2) * 3;
            int[] tris = new int[triCount];
            for (int i = 0; i < fillVertices.Length - 2; i++)
            {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2;
            }

            s_meshes.Add(new MeshEntry
            {
                CachedVertices = fillVertices,
                CachedTriangles = tris,
                FillColor = fillColor,
                WireColor = wireColor,
                WireVertices = wireVertices,
            });
        }

        public static void DrawMesh(Vector3[] vertices, int[] indices, Color fillColor, Color wireColor, float duration = 0f, bool persistent = false)
        {
            if (vertices == null || indices == null || indices.Length < 3)
            {
                return;
            }

            s_meshes.Add(new MeshEntry
            {
                CachedVertices = vertices,
                CachedTriangles = indices,
                FillColor = fillColor,
                WireColor = wireColor,
                WireVertices = null,
            });
        }
    }
}