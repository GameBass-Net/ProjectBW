/// <summary>
/// Project : Easy Build System
/// Class : BuildingInputEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers.Inputs
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Inputs;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Collections;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers.Inputs
{
    [CustomEditor(typeof(BuildingInput), true)]
    public class BuildingInputEditor : BaseInspectorEditor<BuildingInput>
    {
        private ReorderableList.Code.Editor.ReorderableList m_customBuildingPartsList;
        private List<BuildingCollection> m_allLoadedCollections;
        private string[] m_allCollectionDisplayNames;
        private int m_selectedCollectionIndex;
        private SerializedProperty m_useCustomSelectionProperty;
        private SerializedProperty m_customPartReferencesProperty;

        protected override void OnInspectorEnable()
        {
            m_useCustomSelectionProperty = Properties.Get("m_useCustomPartsSelection");
            m_customPartReferencesProperty = Properties.Get("m_customPartReferences");
            m_customBuildingPartsList = new ReorderableList.Code.Editor.ReorderableList(m_customPartReferencesProperty, false);

            RefreshBuildingCollections();
        }

        protected override void OnInspectorDraw()
        {
#if ENABLE_INPUT_SYSTEM
            Properties.Draw("m_validateActionRef", new GUIContent("Validate Action", "Input action for validating/confirming actions."));
            Properties.Draw("m_cancelActionRef", new GUIContent("Cancel Action", "Input action for cancelling current mode."));

            Properties.Draw("m_rotateActionRef", new GUIContent("Rotate Action", "Input action for rotation."));
            Properties.Draw("m_selectActionRef", new GUIContent("Select Action", "Input action for selection."));
#else
            Properties.Draw("m_validateKey", new GUIContent("Validate Key", "Keyboard key for validating/confirming actions."));
            Properties.Draw("m_cancelKey", new GUIContent("Cancel Key", "Keyboard key for cancelling current mode."));

            Properties.Draw("m_useScrollWheelInput", new GUIContent("Use Scroll Wheel Input", "Use mouse scroll wheel for rotation and selection."));
#endif

            Properties.Draw("m_blockWhenPointerOverUI", new GUIContent("Block When Pointer Over UI", "Disable input when mouse is over UI elements."));
            Properties.Draw("m_enableDirectControls", new GUIContent("Enable Direct Controls", "Allow direct mode switching via keyboard/input."));

            if (Properties.Get("m_enableDirectControls").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
#if ENABLE_INPUT_SYSTEM
                    Properties.Draw("m_placementModeActionRef", new GUIContent("Placement Mode Action", "Input action to enter placement mode."));
                    Properties.Draw("m_destructionModeActionRef", new GUIContent("Destruction Mode Action", "Input action to enter destruction mode."));
                    Properties.Draw("m_adjustmentModeActionRef", new GUIContent("Adjustment Mode Action", "Input action to enter adjustment mode."));
#else
                    Properties.Draw("m_placementModeKey", new GUIContent("Placement Mode Key", "Keyboard key to enter placement mode."));
                    Properties.Draw("m_destructionModeKey", new GUIContent("Destruction Mode Key", "Keyboard key to enter destruction mode."));
                    Properties.Draw("m_adjustmentModeKey", new GUIContent("Adjustment Mode Key", "Keyboard key to enter adjustment mode."));
#endif

                    Properties.Draw("m_useCustomPartsSelection", new GUIContent("Use Custom Parts Selection", "Use custom part list instead of all available parts."));

                    if (Properties.Get("m_useCustomPartsSelection").boolValue)
                    {
                        DrawCustomPartsSelection();
                    }
                }
            }
        }

        private void DrawCustomPartsSelection()
        {
            using (EditorGUIExtended.IndentScope(1))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (m_allCollectionDisplayNames != null && m_allCollectionDisplayNames.Length > 0)
                    {
                        m_selectedCollectionIndex = EditorGUILayout.Popup("Building Collection", m_selectedCollectionIndex, m_allCollectionDisplayNames);
                    }
                    else
                    {
                        GUILayout.Label("No BuildingCollection found in project.", EditorStyles.miniLabel);
                    }

                    if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                    {
                        RefreshBuildingCollections();
                    }
                }

                using (EditorGUIExtended.IndentScope(1))
                {
                    bool isValidCollectionSelected = m_allLoadedCollections != null && m_allLoadedCollections.Count > 0 && m_selectedCollectionIndex > 0;

                    int leftMarginPixels = Mathf.RoundToInt(EditorGUI.indentLevel * 10f);
                    using (new EditorGUIScopes.MarginScope(new RectOffset(leftMarginPixels, 0, 0, 0)))
                    using (EditorGUIExtended.DisabledScope(!isValidCollectionSelected))
                    {
                        GUILayout.BeginHorizontal();
                        if (EditorGUIExtended.Button("Replace With Selected"))
                        {
                            ReplaceWithBuildingCollection(m_allLoadedCollections[m_selectedCollectionIndex - 1]);
                        }

                        if (EditorGUIExtended.Button("Append From Selected"))
                        {
                            AppendFromBuildingCollection(m_allLoadedCollections[m_selectedCollectionIndex - 1]);
                        }

                        GUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.Space();

                using (EditorGUIExtended.IndentScope(-2))
                {
                    if (m_customBuildingPartsList != null)
                    {
                        m_customBuildingPartsList.Layout();
                    }
                }
            }
        }

        private void RefreshBuildingCollections()
        {
            string[] allCollectionAssetGuids = AssetDatabase.FindAssets("t:BuildingCollection");
            m_allLoadedCollections = new List<BuildingCollection>(allCollectionAssetGuids.Length);

            foreach (string assetGuid in allCollectionAssetGuids)
            {
                string resolvedAssetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                BuildingCollection loadedBuildingCollection = AssetDatabase.LoadAssetAtPath<BuildingCollection>(resolvedAssetPath);
                if (loadedBuildingCollection)
                {
                    m_allLoadedCollections.Add(loadedBuildingCollection);
                }
            }

            List<string> generatedCollectionNames = m_allLoadedCollections.Select(collectionAsset =>
            {
                string displayNameForCollection = collectionAsset.Name;
                if (string.IsNullOrEmpty(displayNameForCollection))
                {
                    displayNameForCollection = collectionAsset.name;
                }

                return displayNameForCollection;
            }).ToList();

            generatedCollectionNames.Insert(0, "Select Collection...");
            m_allCollectionDisplayNames = generatedCollectionNames.ToArray();

            if (m_selectedCollectionIndex >= m_allCollectionDisplayNames.Length)
            {
                m_selectedCollectionIndex = 0;
            }
        }

        private void ReplaceWithBuildingCollection(BuildingCollection collectionToUse)
        {
            if (!collectionToUse)
            {
                return;
            }

            m_useCustomSelectionProperty.boolValue = true;
            m_customPartReferencesProperty.ClearArray();

            string[] collectionPartReferences = collectionToUse.PartReferences;
            if (collectionPartReferences != null)
            {
                foreach (string partReferenceId in collectionPartReferences)
                {
                    if (string.IsNullOrEmpty(partReferenceId))
                    {
                        continue;
                    }

                    m_customPartReferencesProperty.InsertArrayElementAtIndex(m_customPartReferencesProperty.arraySize);
                    m_customPartReferencesProperty.GetArrayElementAtIndex(m_customPartReferencesProperty.arraySize - 1).stringValue = partReferenceId;
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private void AppendFromBuildingCollection(BuildingCollection collectionToAppendFrom)
        {
            if (!collectionToAppendFrom)
            {
                return;
            }

            m_useCustomSelectionProperty.boolValue = true;

            HashSet<string> currentlyExistingPartReferences = new HashSet<string>();
            for (int currentReferenceIndex = 0; currentReferenceIndex < m_customPartReferencesProperty.arraySize; currentReferenceIndex++)
            {
                currentlyExistingPartReferences.Add(m_customPartReferencesProperty.GetArrayElementAtIndex(currentReferenceIndex).stringValue);
            }

            string[] collectionPartReferencesToAppend = collectionToAppendFrom.PartReferences;
            if (collectionPartReferencesToAppend != null)
            {
                foreach (string partReferenceToAdd in collectionPartReferencesToAppend)
                {
                    if (string.IsNullOrEmpty(partReferenceToAdd) || currentlyExistingPartReferences.Contains(partReferenceToAdd))
                    {
                        continue;
                    }

                    m_customPartReferencesProperty.InsertArrayElementAtIndex(m_customPartReferencesProperty.arraySize);
                    m_customPartReferencesProperty.GetArrayElementAtIndex(m_customPartReferencesProperty.arraySize - 1).stringValue = partReferenceToAdd;
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}