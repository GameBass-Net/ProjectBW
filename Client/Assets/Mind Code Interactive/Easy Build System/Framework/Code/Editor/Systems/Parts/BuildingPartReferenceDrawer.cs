/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartReferenceDrawer.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Collections;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Attributes;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
{
    [CustomPropertyDrawer(typeof(BuildingPartReferenceAttribute))]
    public class BuildingPartReferenceDrawer : PropertyDrawer
    {
        private string[] m_prefabIds = Array.Empty<string>();
        private GUIContent[] m_prefabOptions = Array.Empty<GUIContent>();
        private Dictionary<string, List<PartItem>> m_categorizedItems = new Dictionary<string, List<PartItem>>();
        private Dictionary<string, List<PartItem>> m_collectionItems = new Dictionary<string, List<PartItem>>();
        private List<PartItem> m_allItems = new List<PartItem>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            if (!property.isArray)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float height = EditorGUIUtility.singleLineHeight;

            if (!property.isExpanded)
            {
                return height;
            }

            height += EditorGUIUtility.singleLineHeight + 2;

            for (int i = 0; i < property.arraySize; i++)
            {
                height += EditorGUIUtility.singleLineHeight + 2;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnsureOptionsLoaded();

            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.String)
            {
                DrawPopup(position, label, property);
                EditorGUI.EndProperty();
                return;
            }

            if (property.isArray)
            {
                Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

                if (!property.isExpanded)
                {
                    EditorGUI.EndProperty();
                    return;
                }

                EditorGUI.indentLevel++;

                Rect lineRect = new Rect(
                    position.x,
                    position.y + EditorGUIUtility.singleLineHeight + 2,
                    position.width,
                    EditorGUIUtility.singleLineHeight
                );

                int oldSize = property.arraySize;
                int newSize = EditorGUI.IntField(lineRect, "Size", oldSize);

                if (newSize != oldSize && newSize >= 0)
                {
                    if (newSize > oldSize)
                    {
                        for (int i = oldSize; i < newSize; i++)
                        {
                            property.InsertArrayElementAtIndex(i);
                            SerializedProperty element = property.GetArrayElementAtIndex(i);
                            element.stringValue = m_prefabIds.Length > 0 ? m_prefabIds[0] : string.Empty;
                        }
                    }
                    else
                    {
                        property.arraySize = newSize;
                    }

                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }

                for (int i = 0; i < property.arraySize; i++)
                {
                    lineRect.y += EditorGUIUtility.singleLineHeight + 2;
                    SerializedProperty element = property.GetArrayElementAtIndex(i);

                    if (element.propertyType != SerializedPropertyType.String)
                    {
                        EditorGUI.LabelField(lineRect, "Element " + i, "Not a string");
                        continue;
                    }

                    if (string.IsNullOrEmpty(element.stringValue) && m_prefabIds.Length > 0)
                    {
                        element.stringValue = m_prefabIds[0];
                        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    }

                    DrawPopup(lineRect, new GUIContent("Element " + i), element);
                }

                EditorGUI.indentLevel--;
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.LabelField(position, label.text, "Unsupported type");
            EditorGUI.EndProperty();
        }

        private void DrawPopup(Rect rect, GUIContent label, SerializedProperty stringProperty)
        {
            if (m_prefabIds.Length == 0 || m_prefabOptions.Length == 0)
            {
                EditorGUI.LabelField(rect, label, new GUIContent("No Building Parts Found"));
                return;
            }

            if (string.IsNullOrEmpty(stringProperty.stringValue))
            {
                stringProperty.stringValue = m_prefabIds[0];
                stringProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            int currentIndex = IndexOfPrefabId(stringProperty.stringValue);
            GUIContent displayContent = GetDisplayContent(currentIndex);

            Rect popupRect = EditorGUI.PrefixLabel(rect, label);
            if (GUI.Button(popupRect, displayContent, EditorStyles.popup))
            {
                PartSelectorPopup popup = new PartSelectorPopup(
                    m_categorizedItems,
                    m_collectionItems,
                    m_allItems,
                    currentIndex,
                    (selectedIndex) =>
                    {
                        stringProperty.stringValue = m_prefabIds[selectedIndex];
                        stringProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(stringProperty.serializedObject.targetObject);
                    }
                );

                PopupWindow.Show(popupRect, popup);
            }
        }

        private int IndexOfPrefabId(string prefabId)
        {
            if (m_prefabIds == null || m_prefabIds.Length == 0)
            {
                return -1;
            }

            int index = Array.IndexOf(m_prefabIds, prefabId);
            return index >= 0 ? index : 0;
        }

        private GUIContent GetDisplayContent(int index)
        {
            if (index < 0 || index >= m_prefabOptions.Length)
            {
                return new GUIContent(string.Empty);
            }

            string fullPath = m_prefabOptions[index].text;
            string partName = fullPath.Contains("/")
                ? fullPath.Substring(fullPath.LastIndexOf('/') + 1)
                : fullPath;

            return new GUIContent(partName, m_prefabOptions[index].image);
        }

        private void EnsureOptionsLoaded()
        {
            if (m_prefabOptions != null && m_prefabOptions.Length > 0)
            {
                return;
            }

            BuildingPartRegistry registry = BuildingPartRegistry.Instance;

            if (registry == null)
            {
                m_prefabIds = Array.Empty<string>();
                m_prefabOptions = Array.Empty<GUIContent>();
                m_categorizedItems.Clear();
                m_collectionItems.Clear();
                m_allItems.Clear();
                return;
            }

            List<string> ids = new List<string>();
            List<GUIContent> options = new List<GUIContent>();
            m_categorizedItems.Clear();
            m_collectionItems.Clear();
            m_allItems.Clear();

            HashSet<string> addedIds = new HashSet<string>();

            List<BuildingPartReference> references = registry.PartReferences;
            Dictionary<string, List<BuildingPart>> categorizedParts = new Dictionary<string, List<BuildingPart>>();

            for (int i = 0; i < references.Count; i++)
            {
                BuildingPartReference reference = references[i];

                if (reference == null || reference.BuildingParts == null)
                {
                    continue;
                }

                string categoryName = string.IsNullOrEmpty(reference.Category) ? "Uncategorized" : reference.Category;

                if (!categorizedParts.ContainsKey(categoryName))
                {
                    categorizedParts[categoryName] = new List<BuildingPart>();
                }

                BuildingPart[] parts = reference.BuildingParts;
                for (int j = 0; j < parts.Length; j++)
                {
                    if (parts[j] != null)
                    {
                        categorizedParts[categoryName].Add(parts[j]);
                    }
                }
            }

            foreach (KeyValuePair<string, List<BuildingPart>> kvp in categorizedParts)
            {
                string categoryName = kvp.Key;
                List<BuildingPart> parts = kvp.Value;
                List<PartItem> categoryItems = new List<PartItem>();

                for (int i = 0; i < parts.Count; i++)
                {
                    BuildingPart part = parts[i];
                    if (part == null || string.IsNullOrEmpty(part.PrefabId))
                    {
                        continue;
                    }

                    if (!addedIds.Add(part.PrefabId))
                    {
                        continue;
                    }

                    int itemIndex = ids.Count;
                    ids.Add(part.PrefabId);

                    string display = $"By Category/{categoryName}/{part.Name}";
                    Texture2D icon = part.Thumbnail != null
                        ? part.Thumbnail
                        : (AssetPreview.GetAssetPreview(part.gameObject) ?? AssetPreview.GetMiniThumbnail(part.gameObject));

                    options.Add(new GUIContent(display, icon));
                    PartItem item = new PartItem { partName = part.Name, icon = icon, index = itemIndex, category = categoryName };
                    categoryItems.Add(item);
                    m_allItems.Add(item);
                }

                if (categoryItems.Count > 0)
                {
                    m_categorizedItems[categoryName] = categoryItems;
                }
            }

            string[] collectionGuids = AssetDatabase.FindAssets("t:BuildingCollection");

            for (int c = 0; c < collectionGuids.Length; c++)
            {
                string guid = collectionGuids[c];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                BuildingCollection collection =
                    AssetDatabase.LoadAssetAtPath<BuildingCollection>(assetPath);

                if (collection == null)
                {
                    continue;
                }

                string collectionName = collection.Name;
                string[] partReferences = collection.PartReferences;
                List<PartItem> collectionItemsList = new List<PartItem>();

                for (int p = 0; p < partReferences.Length; p++)
                {
                    string partId = partReferences[p];
                    if (string.IsNullOrEmpty(partId))
                    {
                        continue;
                    }

                    BuildingPart part = registry.GetPartByPrefabId(partId);
                    if (part == null || string.IsNullOrEmpty(part.PrefabId))
                    {
                        continue;
                    }

                    int itemIndex = ids.Count;
                    ids.Add(part.PrefabId);

                    string display = $"By Collection/{collectionName}/{part.Name}";
                    Texture2D icon = part.Thumbnail != null
                        ? part.Thumbnail
                        : (AssetPreview.GetAssetPreview(part.gameObject) ?? AssetPreview.GetMiniThumbnail(part.gameObject));

                    options.Add(new GUIContent(display, icon));
                    collectionItemsList.Add(new PartItem { partName = part.Name, icon = icon, index = itemIndex, category = collectionName });
                }

                if (collectionItemsList.Count > 0)
                {
                    m_collectionItems[collectionName] = collectionItemsList;
                }
            }

            m_prefabIds = ids.ToArray();
            m_prefabOptions = options.ToArray();
        }
    }

    public struct PartItem
    {
        public string partName;
        public Texture2D icon;
        public int index;
        public string category;
    }

    public enum MenuState
    {
        Main,
        CategoryList,
        CategoryItems,
        CollectionList,
        CollectionItems,
        SearchResults
    }

    public class PartSelectorPopup : PopupWindowContent
    {
        private Dictionary<string, List<PartItem>> m_categorizedItems;
        private Dictionary<string, List<PartItem>> m_collectionItems;
        private List<PartItem> m_allItems;
        private List<PartItem> m_searchResults = new List<PartItem>();
        private int m_currentIndex;
        private Action<int> m_onSelectCallback;
        private Vector2 m_scrollPosition = Vector2.zero;
        private MenuState m_menuState = MenuState.Main;
        private string m_selectedKey = null;
        private int m_keyboardIndex = 0;
        private bool m_keyboardActive = false;
        private string m_searchText = "";
        private bool m_searchFieldFocused = false;

        private static readonly Color s_bgColor = new Color(0.22f, 0.22f, 0.22f, 1f);
        private static readonly Color s_hoverColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        private static readonly Color s_selectedColor = new Color(0.17f, 0.36f, 0.53f, 1f);
        private static readonly Color s_separatorColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color s_headerColor = new Color(0.18f, 0.18f, 0.18f, 1f);
        private static readonly Color s_accentColor = new Color(0.3f, 0.5f, 0.8f, 1f);
        private static readonly Color s_keyboardHighlight = new Color(0.4f, 0.4f, 0.4f, 1f);
        private static readonly Color s_searchBgColor = new Color(0.16f, 0.16f, 0.16f, 1f);

        private const float THUMBNAIL_SIZE = 24f;
        private const float ITEM_SIZE = 25f;
        private const float HEADER_HEIGHT = 26f;
        private const float PADDING = 5f;
        private const float ARROW_WIDTH = 20f;
        private const float SEARCHBAR_HEIGHT = 24f;

        private GUIStyle m_labelStyle;
        private GUIStyle m_headerStyle;
        private GUIStyle m_arrowStyle;
        private GUIStyle m_searchFieldStyle;
        private GUIStyle m_searchPlaceholderStyle;

        public PartSelectorPopup(
            Dictionary<string, List<PartItem>> categorizedItems,
            Dictionary<string, List<PartItem>> collectionItems,
            List<PartItem> allItems,
            int currentIndex,
            Action<int> onSelectCallback)
        {
            m_categorizedItems = categorizedItems;
            m_collectionItems = collectionItems;
            m_allItems = allItems;
            m_currentIndex = currentIndex;
            m_onSelectCallback = onSelectCallback;
        }

        private void InitStyles()
        {
            if (m_labelStyle == null)
            {
                m_labelStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(4, 4, 0, 0)
                };
            }

            if (m_headerStyle == null)
            {
                m_headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(6, 6, 0, 0)
                };
            }

            if (m_arrowStyle == null)
            {
                m_arrowStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (m_searchFieldStyle == null)
            {
                m_searchFieldStyle = new GUIStyle(EditorStyles.toolbarSearchField)
                {
                    fontSize = 11,
                    fixedHeight = 18f,
                    margin = new RectOffset(4, 4, 3, 3)
                };
            }

            if (m_searchPlaceholderStyle == null)
            {
                m_searchPlaceholderStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Italic,
                    normal = { textColor = new Color(0.5f, 0.5f, 0.5f, 1f) },
                    padding = new RectOffset(16, 4, 0, 0),
                    alignment = TextAnchor.MiddleLeft
                };
            }
        }

        public override Vector2 GetWindowSize()
        {
            float height = SEARCHBAR_HEIGHT;

            if (!string.IsNullOrEmpty(m_searchText))
            {
                int resultCount = Mathf.Max(m_searchResults.Count, 5);
                height += HEADER_HEIGHT + (resultCount * ITEM_SIZE) + 2;
            }
            else
            {
                switch (m_menuState)
                {
                    case MenuState.Main:
                        height += ITEM_SIZE * 2 + 2;
                        break;

                    case MenuState.CategoryList:
                        height += HEADER_HEIGHT + (m_categorizedItems.Count * ITEM_SIZE) + 2;
                        break;

                    case MenuState.CategoryItems:
                        if (m_selectedKey != null && m_categorizedItems.ContainsKey(m_selectedKey))
                        {
                            height += HEADER_HEIGHT + (m_categorizedItems[m_selectedKey].Count * ITEM_SIZE) + 2;
                        }

                        break;

                    case MenuState.CollectionList:
                        height += HEADER_HEIGHT + (m_collectionItems.Count * ITEM_SIZE) + 2;
                        break;

                    case MenuState.CollectionItems:
                        if (m_selectedKey != null && m_collectionItems.ContainsKey(m_selectedKey))
                        {
                            height += HEADER_HEIGHT + (m_collectionItems[m_selectedKey].Count * ITEM_SIZE) + 2;
                        }

                        break;
                }
            }

            height = Mathf.Clamp(height, 60f + SEARCHBAR_HEIGHT, 320f);
            return new Vector2(260f, height);
        }

        public override void OnGUI(Rect rect)
        {
            InitStyles();

            EditorGUI.DrawRect(rect, s_bgColor);

            Rect searchBarRect = new Rect(0, 0, rect.width, SEARCHBAR_HEIGHT);
            DrawSearchBar(searchBarRect);

            Rect contentRect = new Rect(0, SEARCHBAR_HEIGHT, rect.width, rect.height - SEARCHBAR_HEIGHT);

            if (!string.IsNullOrEmpty(m_searchText))
            {
                HandleKeyboardInput(contentRect);
                DrawSearchResults(contentRect);
            }
            else
            {
                HandleKeyboardInput(contentRect);

                switch (m_menuState)
                {
                    case MenuState.Main:
                        DrawMainMenu(contentRect);
                        break;

                    case MenuState.CategoryList:
                        DrawCategoryList(contentRect);
                        break;

                    case MenuState.CategoryItems:
                        if (m_selectedKey != null && m_categorizedItems.ContainsKey(m_selectedKey))
                        {
                            DrawItemsMenu(contentRect, m_categorizedItems[m_selectedKey], m_selectedKey);
                        }

                        break;

                    case MenuState.CollectionList:
                        DrawCollectionList(contentRect);
                        break;

                    case MenuState.CollectionItems:
                        if (m_selectedKey != null && m_collectionItems.ContainsKey(m_selectedKey))
                        {
                            DrawItemsMenu(contentRect, m_collectionItems[m_selectedKey], m_selectedKey);
                        }

                        break;
                }
            }

            if (Event.current.type == EventType.MouseMove)
            {
                m_keyboardActive = false;
                editorWindow.Repaint();
            }
        }

        private void DrawSearchBar(Rect rect)
        {
            EditorGUI.DrawRect(rect, s_searchBgColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), s_separatorColor);

            float clearButtonWidth = 40f;
            bool hasText = !string.IsNullOrEmpty(m_searchText);

            Rect searchFieldRect = new Rect(rect.x + 4, rect.y + 3, rect.width - 8 - (hasText ? clearButtonWidth : 0), 18);

            GUI.SetNextControlName("PartSearchField");
            string newSearchText = EditorGUI.TextField(searchFieldRect, m_searchText, m_searchFieldStyle);

            if (newSearchText != m_searchText)
            {
                m_searchText = newSearchText;
                UpdateSearchResults();
                m_keyboardIndex = 0;
                m_scrollPosition = Vector2.zero;
            }

            if (string.IsNullOrEmpty(m_searchText) && GUI.GetNameOfFocusedControl() != "PartSearchField")
            {
                GUI.Label(searchFieldRect, "Search parts...", m_searchPlaceholderStyle);
            }

            if (hasText)
            {
                Rect clearRect = new Rect(searchFieldRect.xMax + 2, rect.y + 3, clearButtonWidth, 18);

                bool isHovered = clearRect.Contains(Event.current.mousePosition);
                if (isHovered)
                {
                    EditorGUI.DrawRect(clearRect, s_hoverColor);
                }

                GUIStyle clearStyle = new GUIStyle(m_labelStyle)
                {
                    padding = new RectOffset(0, 0, -2, 0),
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11
                };
                GUI.Label(clearRect, "Clear", clearStyle);

                if (Event.current.type == EventType.MouseDown && isHovered)
                {
                    m_searchText = "";
                    m_searchResults.Clear();
                    m_keyboardIndex = 0;
                    m_scrollPosition = Vector2.zero;
                    Event.current.Use();
                }
            }

            if (!m_searchFieldFocused)
            {
                EditorGUI.FocusTextInControl("PartSearchField");
                m_searchFieldFocused = true;
            }
        }

        private void UpdateSearchResults()
        {
            m_searchResults.Clear();

            if (string.IsNullOrEmpty(m_searchText))
            {
                return;
            }

            string searchLower = m_searchText.ToLowerInvariant();

            for (int i = 0; i < m_allItems.Count; i++)
            {
                PartItem item = m_allItems[i];
                if (item.partName.ToLowerInvariant().Contains(searchLower) ||
                    (!string.IsNullOrEmpty(item.category) && item.category.ToLowerInvariant().Contains(searchLower)))
                {
                    m_searchResults.Add(item);
                }
            }
        }

        private void DrawSearchResults(Rect rect)
        {
            DrawHeader(new Rect(0, rect.y, rect.width, HEADER_HEIGHT), $"Results ({m_searchResults.Count})", false);

            Rect scrollRect = new Rect(0, rect.y + HEADER_HEIGHT, rect.width, rect.height - HEADER_HEIGHT);

            if (m_searchResults.Count == 0)
            {
                Rect noResultRect = new Rect(PADDING, scrollRect.y + PADDING, scrollRect.width - PADDING * 2, ITEM_SIZE);
                GUI.Label(noResultRect, "No parts found", m_labelStyle);
                return;
            }

            float contentHeight = m_searchResults.Count * ITEM_SIZE;
            Rect contentRect = new Rect(0, 0, scrollRect.width - (contentHeight > scrollRect.height ? 13 : 0), contentHeight);

            m_scrollPosition = GUI.BeginScrollView(scrollRect, m_scrollPosition, contentRect);

            for (int i = 0; i < m_searchResults.Count; i++)
            {
                Rect itemRect = new Rect(1, i * ITEM_SIZE, contentRect.width - 2, ITEM_SIZE);
                bool isKeyboardSelected = m_keyboardActive && m_keyboardIndex == i;
                if (DrawPartItem(itemRect, m_searchResults[i], isKeyboardSelected))
                {
                    m_onSelectCallback?.Invoke(m_searchResults[i].index);
                    editorWindow.Close();
                }
            }

            GUI.EndScrollView();
        }

        private void HandleKeyboardInput(Rect rect)
        {
            Event e = Event.current;
            if (e.type != EventType.KeyDown)
            {
                return;
            }

            int itemCount = GetCurrentItemCount();

            switch (e.keyCode)
            {
                case KeyCode.DownArrow:
                    m_keyboardActive = true;
                    m_keyboardIndex = Mathf.Min(m_keyboardIndex + 1, itemCount - 1);
                    EnsureKeyboardIndexVisible(rect);
                    e.Use();
                    break;

                case KeyCode.UpArrow:
                    m_keyboardActive = true;
                    m_keyboardIndex = Mathf.Max(m_keyboardIndex - 1, 0);
                    EnsureKeyboardIndexVisible(rect);
                    e.Use();
                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (m_keyboardActive || !string.IsNullOrEmpty(m_searchText))
                    {
                        ExecuteKeyboardSelection();
                        e.Use();
                    }
                    break;

                case KeyCode.RightArrow:
                    if (m_keyboardActive && HasSubMenu() && string.IsNullOrEmpty(m_searchText))
                    {
                        ExecuteKeyboardSelection();
                        e.Use();
                    }
                    break;

                case KeyCode.LeftArrow:
                    if (string.IsNullOrEmpty(m_searchText))
                    {
                        GoBack();
                        e.Use();
                    }
                    break;

                case KeyCode.Escape:
                    if (!string.IsNullOrEmpty(m_searchText))
                    {
                        m_searchText = "";
                        m_searchResults.Clear();
                        m_keyboardIndex = 0;
                        m_scrollPosition = Vector2.zero;
                        e.Use();
                    }
                    else
                    {
                        GoBack();
                        e.Use();
                    }
                    break;

                case KeyCode.Backspace:
                    if (string.IsNullOrEmpty(m_searchText))
                    {
                        GoBack();
                        e.Use();
                    }
                    break;
            }
        }

        private int GetCurrentItemCount()
        {
            if (!string.IsNullOrEmpty(m_searchText))
            {
                return m_searchResults.Count;
            }

            switch (m_menuState)
            {
                case MenuState.Main:
                    return 2;
                case MenuState.CategoryList:
                    return m_categorizedItems.Count;
                case MenuState.CategoryItems:
                    return m_selectedKey != null && m_categorizedItems.ContainsKey(m_selectedKey)
                        ? m_categorizedItems[m_selectedKey].Count : 0;
                case MenuState.CollectionList:
                    return m_collectionItems.Count;
                case MenuState.CollectionItems:
                    return m_selectedKey != null && m_collectionItems.ContainsKey(m_selectedKey)
                        ? m_collectionItems[m_selectedKey].Count : 0;
                default:
                    return 0;
            }
        }

        private bool HasSubMenu()
            => m_menuState == MenuState.Main ||
               m_menuState == MenuState.CategoryList ||
               m_menuState == MenuState.CollectionList;

        private void ExecuteKeyboardSelection()
        {
            if (!string.IsNullOrEmpty(m_searchText))
            {
                if (m_keyboardIndex < m_searchResults.Count)
                {
                    m_onSelectCallback?.Invoke(m_searchResults[m_keyboardIndex].index);
                    editorWindow.Close();
                }
                return;
            }

            switch (m_menuState)
            {
                case MenuState.Main:
                    if (m_keyboardIndex == 0)
                    {
                        m_menuState = MenuState.CategoryList;
                    }
                    else
                    {
                        m_menuState = MenuState.CollectionList;
                    }

                    m_keyboardIndex = 0;
                    m_scrollPosition = Vector2.zero;
                    break;

                case MenuState.CategoryList:
                    List<string> catKeys = new List<string>(m_categorizedItems.Keys);
                    if (m_keyboardIndex < catKeys.Count)
                    {
                        m_selectedKey = catKeys[m_keyboardIndex];
                        m_menuState = MenuState.CategoryItems;
                        m_keyboardIndex = 0;
                        m_scrollPosition = Vector2.zero;
                    }
                    break;

                case MenuState.CategoryItems:
                    if (m_selectedKey != null && m_categorizedItems.ContainsKey(m_selectedKey))
                    {
                        List<PartItem> items = m_categorizedItems[m_selectedKey];
                        if (m_keyboardIndex < items.Count)
                        {
                            m_onSelectCallback?.Invoke(items[m_keyboardIndex].index);
                            editorWindow.Close();
                        }
                    }
                    break;

                case MenuState.CollectionList:
                    List<string> colKeys = new List<string>(m_collectionItems.Keys);
                    if (m_keyboardIndex < colKeys.Count)
                    {
                        m_selectedKey = colKeys[m_keyboardIndex];
                        m_menuState = MenuState.CollectionItems;
                        m_keyboardIndex = 0;
                        m_scrollPosition = Vector2.zero;
                    }
                    break;

                case MenuState.CollectionItems:
                    if (m_selectedKey != null && m_collectionItems.ContainsKey(m_selectedKey))
                    {
                        List<PartItem> items = m_collectionItems[m_selectedKey];
                        if (m_keyboardIndex < items.Count)
                        {
                            m_onSelectCallback?.Invoke(items[m_keyboardIndex].index);
                            editorWindow.Close();
                        }
                    }
                    break;
            }
        }

        private void GoBack()
        {
            switch (m_menuState)
            {
                case MenuState.CategoryList:
                case MenuState.CollectionList:
                    m_menuState = MenuState.Main;
                    m_keyboardIndex = 0;
                    m_scrollPosition = Vector2.zero;
                    break;

                case MenuState.CategoryItems:
                    m_menuState = MenuState.CategoryList;
                    m_keyboardIndex = 0;
                    m_scrollPosition = Vector2.zero;
                    break;

                case MenuState.CollectionItems:
                    m_menuState = MenuState.CollectionList;
                    m_keyboardIndex = 0;
                    m_scrollPosition = Vector2.zero;
                    break;

                case MenuState.Main:
                    editorWindow.Close();
                    break;
            }
        }

        private void EnsureKeyboardIndexVisible(Rect rect)
        {
            float itemY = m_keyboardIndex * ITEM_SIZE;
            float viewHeight = rect.height - HEADER_HEIGHT;

            if (itemY < m_scrollPosition.y)
            {
                m_scrollPosition.y = itemY;
            }
            else if (itemY + ITEM_SIZE > m_scrollPosition.y + viewHeight)
            {
                m_scrollPosition.y = itemY + ITEM_SIZE - viewHeight;
            }
        }

        private void DrawMainMenu(Rect rect)
        {
            float yPos = rect.y + 1;

            Rect catRect = new Rect(1, yPos, rect.width - 2, ITEM_SIZE);
            bool catKeyboard = m_keyboardActive && m_keyboardIndex == 0;
            if (DrawMenuItem(catRect, "By Category", false, true, catKeyboard, 0))
            {
                m_menuState = MenuState.CategoryList;
                m_keyboardIndex = 0;
                m_scrollPosition = Vector2.zero;
            }
            yPos += ITEM_SIZE;

            EditorGUI.DrawRect(new Rect(PADDING, yPos, rect.width - PADDING * 2, 1), s_separatorColor);
            yPos += 1;

            Rect colRect = new Rect(1, yPos, rect.width - 2, ITEM_SIZE);
            bool colKeyboard = m_keyboardActive && m_keyboardIndex == 1;
            if (DrawMenuItem(colRect, "By Collection", false, true, colKeyboard, 1))
            {
                m_menuState = MenuState.CollectionList;
                m_keyboardIndex = 0;
                m_scrollPosition = Vector2.zero;
            }
        }

        private void DrawCategoryList(Rect rect)
        {
            DrawHeader(new Rect(0, rect.y, rect.width, HEADER_HEIGHT), "Categories", true);

            Rect scrollRect = new Rect(0, rect.y + HEADER_HEIGHT, rect.width, rect.height - HEADER_HEIGHT);
            List<string> categoryNames = new List<string>(m_categorizedItems.Keys);
            float contentHeight = categoryNames.Count * ITEM_SIZE;
            Rect contentRect = new Rect(0, 0, scrollRect.width - (contentHeight > scrollRect.height ? 13 : 0), contentHeight);

            m_scrollPosition = GUI.BeginScrollView(scrollRect, m_scrollPosition, contentRect);

            for (int i = 0; i < categoryNames.Count; i++)
            {
                Rect itemRect = new Rect(1, i * ITEM_SIZE, contentRect.width - 2, ITEM_SIZE);
                string categoryName = categoryNames[i];
                int count = m_categorizedItems[categoryName].Count;
                bool isKeyboardSelected = m_keyboardActive && m_keyboardIndex == i;

                if (DrawMenuItem(itemRect, $"{categoryName} ({count})", false, true, isKeyboardSelected, i))
                {
                    m_selectedKey = categoryName;
                    m_menuState = MenuState.CategoryItems;
                    m_keyboardIndex = 0;
                    m_scrollPosition = Vector2.zero;
                }
            }

            GUI.EndScrollView();
        }

        private void DrawCollectionList(Rect rect)
        {
            DrawHeader(new Rect(0, rect.y, rect.width, HEADER_HEIGHT), "Collections", true);

            Rect scrollRect = new Rect(0, rect.y + HEADER_HEIGHT, rect.width, rect.height - HEADER_HEIGHT);
            List<string> collectionNames = new List<string>(m_collectionItems.Keys);
            float contentHeight = collectionNames.Count * ITEM_SIZE;
            Rect contentRect = new Rect(0, 0, scrollRect.width - (contentHeight > scrollRect.height ? 13 : 0), contentHeight);

            m_scrollPosition = GUI.BeginScrollView(scrollRect, m_scrollPosition, contentRect);

            for (int i = 0; i < collectionNames.Count; i++)
            {
                Rect itemRect = new Rect(1, i * ITEM_SIZE, contentRect.width - 2, ITEM_SIZE);
                string collectionName = collectionNames[i];
                int count = m_collectionItems[collectionName].Count;
                bool isKeyboardSelected = m_keyboardActive && m_keyboardIndex == i;

                if (DrawMenuItem(itemRect, $"{collectionName} ({count})", false, true, isKeyboardSelected, i))
                {
                    m_selectedKey = collectionName;
                    m_menuState = MenuState.CollectionItems;
                    m_keyboardIndex = 0;
                    m_scrollPosition = Vector2.zero;
                }
            }

            GUI.EndScrollView();
        }

        private void DrawItemsMenu(Rect rect, List<PartItem> items, string title)
        {
            DrawHeader(new Rect(0, rect.y, rect.width, HEADER_HEIGHT), title, true);

            Rect scrollRect = new Rect(0, rect.y + HEADER_HEIGHT, rect.width, rect.height - HEADER_HEIGHT);
            float contentHeight = items.Count * ITEM_SIZE;
            Rect contentRect = new Rect(0, 0, scrollRect.width - (contentHeight > scrollRect.height ? 13 : 0), contentHeight);

            m_scrollPosition = GUI.BeginScrollView(scrollRect, m_scrollPosition, contentRect);

            for (int i = 0; i < items.Count; i++)
            {
                Rect itemRect = new Rect(1, i * ITEM_SIZE, contentRect.width - 2, ITEM_SIZE);
                bool isKeyboardSelected = m_keyboardActive && m_keyboardIndex == i;
                if (DrawPartItem(itemRect, items[i], isKeyboardSelected))
                {
                    m_onSelectCallback?.Invoke(items[i].index);
                    editorWindow.Close();
                }
            }

            GUI.EndScrollView();
        }

        private void DrawHeader(Rect rect, string text, bool showBack)
        {
            EditorGUI.DrawRect(rect, s_headerColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), s_separatorColor);

            if (showBack)
            {
                Rect backRect = new Rect(rect.x + 2, rect.y + 2, HEADER_HEIGHT - 4, HEADER_HEIGHT - 4);
                bool backHovered = backRect.Contains(Event.current.mousePosition);

                if (backHovered)
                {
                    EditorGUI.DrawRect(backRect, s_hoverColor);
                }

                GUI.Label(backRect, "◀", m_arrowStyle);

                if (Event.current.type == EventType.MouseDown && backRect.Contains(Event.current.mousePosition))
                {
                    if (m_menuState == MenuState.CategoryItems || m_menuState == MenuState.CollectionItems)
                    {
                        m_menuState = m_menuState == MenuState.CategoryItems ? MenuState.CategoryList : MenuState.CollectionList;
                    }
                    else
                    {
                        m_menuState = MenuState.Main;
                    }

                    m_scrollPosition = Vector2.zero;
                    Event.current.Use();
                }

                Rect textRect = new Rect(backRect.xMax + 4, rect.y, rect.width - backRect.width - 10, rect.height);
                GUI.Label(textRect, text, m_headerStyle);
            }
            else
            {
                GUI.Label(rect, text, m_headerStyle);
            }
        }

        private bool DrawMenuItem(Rect rect, string text, bool isSelected, bool showArrow, bool isKeyboardHighlight = false, int itemIndex = -1)
        {
            bool isHovered = rect.Contains(Event.current.mousePosition);

            if (isSelected)
            {
                EditorGUI.DrawRect(rect, s_selectedColor);
            }
            else if (isKeyboardHighlight)
            {
                EditorGUI.DrawRect(rect, s_keyboardHighlight);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 2, rect.height), s_accentColor);
            }
            else if (isHovered)
            {
                EditorGUI.DrawRect(rect, s_hoverColor);
            }

            Rect textRect = new Rect(rect.x + PADDING + (isKeyboardHighlight ? 2 : 0), rect.y, rect.width - PADDING * 2 - (showArrow ? ARROW_WIDTH : 0), rect.height);
            GUI.Label(textRect, text, m_labelStyle);

            if (showArrow)
            {
                Rect arrowRect = new Rect(rect.xMax - ARROW_WIDTH, rect.y, ARROW_WIDTH, rect.height);
                Color oldColor = GUI.color;
                GUI.color = (isHovered || isKeyboardHighlight) ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);
                GUI.Label(arrowRect, "▶", m_arrowStyle);
                GUI.color = oldColor;
            }

            if (Event.current.type == EventType.MouseDown && isHovered)
            {
                m_keyboardActive = false;
                Event.current.Use();
                return true;
            }

            return false;
        }

        private bool DrawPartItem(Rect rect, PartItem item, bool isKeyboardHighlight = false)
        {
            bool isHovered = rect.Contains(Event.current.mousePosition);
            bool isSelected = item.index == m_currentIndex;

            if (isSelected)
            {
                EditorGUI.DrawRect(rect, s_selectedColor);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3, rect.height), s_accentColor);
            }
            else if (isKeyboardHighlight)
            {
                EditorGUI.DrawRect(rect, s_keyboardHighlight);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3, rect.height), s_accentColor);
            }
            else if (isHovered)
            {
                EditorGUI.DrawRect(rect, s_hoverColor);
            }

            float thumbX = rect.x + PADDING + ((isSelected || isKeyboardHighlight) ? 3 : 0);
            Rect thumbRect = new Rect(thumbX, rect.y + (rect.height - THUMBNAIL_SIZE) / 2, THUMBNAIL_SIZE, THUMBNAIL_SIZE);

            if (item.icon != null)
            {
                GUI.DrawTexture(thumbRect, item.icon, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUI.DrawRect(thumbRect, new Color(0.3f, 0.3f, 0.3f, 1f));
            }

            Rect textRect = new Rect(thumbRect.xMax + PADDING, rect.y, rect.width - thumbRect.xMax - PADDING * 2, rect.height);
            GUI.Label(textRect, item.partName, m_labelStyle);

            if (Event.current.type == EventType.MouseDown && isHovered)
            {
                m_keyboardActive = false;
                Event.current.Use();
                return true;
            }

            return false;
        }
    }
}