/// <summary>
/// Project : Thumbnail Generator
/// Class : ThumbnailPreviewHandler.cs
/// Namespace : MindCodeInteractive.ThumbnailGenerator.Code.Editor
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;
using UnityEditor;

namespace MindCodeInteractive.ThumbnailGenerator.Code.Editor
{
    public static class ThumbnailPreviewHandler
    {
        private static bool s_isDragging;
        private static bool s_isPanning;

        public static float Zoom { get; set; } = 1f;
        public static Vector2 Pan { get; set; } = Vector2.zero;

        public static void Reset()
        {
            Zoom = 1f;
            Pan = Vector2.zero;
            s_isDragging = false;
            s_isPanning = false;
        }

        public static void DrawTexture(Rect inner, Texture2D texture, ThumbnailSettings settings, Texture2D checkerTexture)
        {
            float texAspect = (float)texture.width / texture.height;
            float rectAspect = inner.width / inner.height;
            float scaledWidth, scaledHeight;

            if (texAspect > rectAspect)
            {
                scaledWidth = inner.width * Zoom;
                scaledHeight = scaledWidth / texAspect;
            }
            else
            {
                scaledHeight = inner.height * Zoom;
                scaledWidth = scaledHeight * texAspect;
            }

            Rect texRect = new Rect(
                inner.center.x - scaledWidth * 0.5f + Pan.x,
                inner.center.y - scaledHeight * 0.5f + Pan.y,
                scaledWidth, scaledHeight);

            GUI.BeginClip(inner);
            Rect clipped = new Rect(texRect.x - inner.x, texRect.y - inner.y, texRect.width, texRect.height);

            if (settings.BackgroundColor.a < 1f)
            {
                GUI.DrawTexture(clipped, checkerTexture, ScaleMode.ScaleToFit, false);
                GUI.DrawTexture(clipped, texture, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.DrawTexture(clipped, texture, ScaleMode.StretchToFill, true);
            }

            GUI.EndClip();

            GUIStyle infoStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.LowerRight,
                padding = new RectOffset(0, 5, 0, 5)
            };
            GUI.Label(inner, $"{texture.width}x{texture.height}", infoStyle);
        }

        public static void DrawPlaceholder(Rect inner)
        {
            GUIStyle label = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Italic
            };

            label.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.7f, 0.7f, 0.7f, 0.5f)
                : new Color(0.3f, 0.3f, 0.3f, 0.5f);

            GUI.Label(inner, "Drag a GameObject here\nor select one in the Model field", label);
        }

        public static bool HandleInteraction(Rect rect, ThumbnailSettings settings, EditorWindow window, out bool needsRender)
        {
            needsRender = false;

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            Event e = Event.current;

            switch (e.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (!rect.Contains(e.mousePosition))
                    {
                        break;
                    }

                    if (e.button == 0)
                    {
                        GUIUtility.hotControl = controlID;
                        s_isDragging = true;
                        e.Use();
                    }
                    else if (e.button == 1 && e.alt)
                    {
                        GUIUtility.hotControl = controlID;
                        s_isPanning = true;
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl != controlID)
                    {
                        break;
                    }

                    GUIUtility.hotControl = 0;
                    s_isDragging = false;
                    s_isPanning = false;
                    e.Use();
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl != controlID)
                    {
                        break;
                    }

                    if (s_isDragging)
                    {
                        Undo.RecordObject(settings, "Rotate Preview");
                        float yaw = settings.OrbitAngles.y + e.delta.x * 0.5f;
                        if (yaw > 180f)
                        {
                            yaw -= 360f;
                        }

                        if (yaw < -180f)
                        {
                            yaw += 360f;
                        }

                        float pitch = Mathf.Clamp(settings.OrbitAngles.x - e.delta.y * 0.5f, -89f, 89f);
                        settings.OrbitAngles = new Vector2(pitch, yaw);
                        needsRender = true;
                    }
                    else if (s_isPanning)
                    {
                        Pan += e.delta;
                    }
                    e.Use();
                    window.Repaint();
                    break;

                case EventType.ScrollWheel:
                    if (!rect.Contains(e.mousePosition))
                    {
                        break;
                    }

                    if (e.control || e.command)
                    {
                        Zoom = Mathf.Clamp(Zoom - e.delta.y * 0.05f, 0.5f, 3f);
                    }
                    else
                    {
                        Undo.RecordObject(settings, "Zoom Preview");
                        settings.OrbitDistance = Mathf.Clamp(settings.OrbitDistance + e.delta.y * 0.05f, 0.5f, 10f);
                        needsRender = true;
                    }
                    e.Use();
                    window.Repaint();
                    break;
            }

            return needsRender;
        }

        public static GameObject HandleDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;

            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            {
                return null;
            }

            if (!dropArea.Contains(evt.mousePosition))
            {
                return null;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (Object draggedObject in DragAndDrop.objectReferences)
                {
                    if (draggedObject is GameObject go)
                    {
                        evt.Use();
                        return go;
                    }
                }
            }

            evt.Use();
            return null;
        }

        public static Texture2D EnsureReadable(Texture2D tex)
        {
            if (tex == null)
            {
                return null;
            }

            RenderTexture rt = RenderTexture.GetTemporary(tex.width, tex.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);

            Graphics.Blit(tex, rt);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            bool linear = QualitySettings.activeColorSpace == ColorSpace.Linear;
            Texture2D readable = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false, linear);
            readable.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            readable.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            return readable;
        }
    }
}