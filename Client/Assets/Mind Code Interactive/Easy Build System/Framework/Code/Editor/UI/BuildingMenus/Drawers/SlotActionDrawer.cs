/// <summary>
/// Project : Easy Build System
/// Class : SlotActionDrawer.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.UI.BuildingMenus.Drawers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Actions;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.UI.BuildingMenus.Drawers
{
    [CustomPropertyDrawer(typeof(IBuildingMenuSlotAction), true)]
    public class SlotActionDrawer : PropertyDrawer
    {
        private static Type[] s_actionTypes;
        private static readonly Dictionary<string, Type> s_typeCacheMap = new Dictionary<string, Type>(256);
        private static readonly HashSet<string> s_expandedInit = new HashSet<string>();

        private const float FOLDOUT_WIDTH = 16f;

        private static Type[] ActionTypes => s_actionTypes ?? (s_actionTypes = FindActionTypes());

        public override float GetPropertyHeight(SerializedProperty propertyToMeasure, GUIContent labelForProperty)
        {
            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            float standardVerticalSpacing = EditorGUIUtility.standardVerticalSpacing;
            float calculatedTotalHeight = singleLineHeight + standardVerticalSpacing;

            if (string.IsNullOrEmpty(propertyToMeasure.managedReferenceFullTypename))
            {
                float helpBoxWidthAvailable = Mathf.Max(0f, EditorGUIUtility.currentViewWidth - 36f);
                calculatedTotalHeight += EditorStyles.helpBox.CalcHeight(
                    new GUIContent("No action selected. Click dropdown to choose."),
                    helpBoxWidthAvailable);
                return calculatedTotalHeight;
            }

            if (!propertyToMeasure.isExpanded)
            {
                return calculatedTotalHeight;
            }

            SerializedProperty iteratorPropertyToCopy = propertyToMeasure.Copy();
            SerializedProperty endIteratorPropertyToReach = iteratorPropertyToCopy.GetEndProperty();
            bool shouldProcessChildren = true;

            while (iteratorPropertyToCopy.NextVisible(shouldProcessChildren) && !SerializedProperty.EqualContents(iteratorPropertyToCopy, endIteratorPropertyToReach))
            {
                calculatedTotalHeight += EditorGUI.GetPropertyHeight(iteratorPropertyToCopy, true) + standardVerticalSpacing;
                shouldProcessChildren = false;
            }

            return calculatedTotalHeight;
        }

        public override void OnGUI(Rect positionToDrawAt, SerializedProperty propertyToDraw, GUIContent labelForProperty)
        {
            EnsureDefaultExpanded(propertyToDraw);
            EditorGUI.BeginProperty(positionToDrawAt, labelForProperty, propertyToDraw);

            float singleLineHeightValue = EditorGUIUtility.singleLineHeight;
            float standardVerticalSpacingValue = EditorGUIUtility.standardVerticalSpacing;

            Rect headerRectangleToUse = EditorGUI.IndentedRect(new Rect(positionToDrawAt.x, positionToDrawAt.y, positionToDrawAt.width, singleLineHeightValue));
            Rect foldoutRectangleToUse = new Rect(headerRectangleToUse.x, headerRectangleToUse.y, FOLDOUT_WIDTH, singleLineHeightValue);
            Rect dropdownRectangleToUse = new Rect(headerRectangleToUse.x + FOLDOUT_WIDTH, headerRectangleToUse.y, headerRectangleToUse.width - FOLDOUT_WIDTH, singleLineHeightValue);

            Type resolvedCurrentActionType = ResolveType(propertyToDraw.managedReferenceFullTypename);
            GUIContent actionTypeNameToDisplay = resolvedCurrentActionType != null
                ? new GUIContent(resolvedCurrentActionType.Name, "Selected action type")
                : new GUIContent("(None)", "No action selected");

            propertyToDraw.isExpanded = EditorGUI.Foldout(foldoutRectangleToUse, propertyToDraw.isExpanded, GUIContent.none, true);

            if (EditorGUI.DropdownButton(dropdownRectangleToUse, actionTypeNameToDisplay, FocusType.Keyboard))
            {
                ShowActionTypeMenu(propertyToDraw, resolvedCurrentActionType);
            }

            float currentVerticalDrawPosition = positionToDrawAt.y + singleLineHeightValue + standardVerticalSpacingValue;

            if (string.IsNullOrEmpty(propertyToDraw.managedReferenceFullTypename))
            {
                GUIContent helpMessageContent = new GUIContent("No action selected. Click dropdown to choose.");
                Rect helpBoxRectangleToUse = EditorGUI.IndentedRect(new Rect(positionToDrawAt.x, currentVerticalDrawPosition, positionToDrawAt.width,
                    EditorStyles.helpBox.CalcHeight(helpMessageContent, positionToDrawAt.width)));
                EditorGUI.HelpBox(helpBoxRectangleToUse, "No action selected. Click dropdown to choose.", MessageType.Info);
                EditorGUI.EndProperty();
                return;
            }

            if (!propertyToDraw.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            int originalIndentLevelBeforeDraw = EditorGUI.indentLevel;
            DrawActionProperties(propertyToDraw, positionToDrawAt, currentVerticalDrawPosition, standardVerticalSpacingValue);
            EditorGUI.indentLevel = originalIndentLevelBeforeDraw;
            EditorGUI.EndProperty();
        }

        private static Type[] FindActionTypes()
        {
            IEnumerable<Type> actionTypeEnumerationToFilter;

#if UNITY_2020_1_OR_NEWER
            actionTypeEnumerationToFilter = TypeCache.GetTypesDerivedFrom<IBuildingMenuSlotAction>();
#else
            actionTypeEnumerationToFilter = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assemblyItemToProcess =>
                {
                    try
                    {
                        return assemblyItemToProcess.GetTypes();
                    }
                    catch (ReflectionTypeLoadException reflectionExceptionToHandle)
                    {
                        return reflectionExceptionToHandle.Types.Where(typeItemInException => typeItemInException != null);
                    }
                })
                .Where(typeItemToFilter => typeof(IBuildingMenuSlotAction).IsAssignableFrom(typeItemToFilter));
#endif

            return actionTypeEnumerationToFilter
                .Where(typeItemToValidate => typeItemToValidate != null && typeItemToValidate.IsClass && !typeItemToValidate.IsAbstract && !typeItemToValidate.IsInterface && typeItemToValidate.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(typeItemToSort => typeItemToSort.Name)
                .ToArray();
        }

        private void ShowActionTypeMenu(SerializedProperty propertyToUpdate, Type currentActionTypeSelected)
        {
            GenericMenu actionTypeMenuToDisplay = new GenericMenu();
            Type[] allAvailableActionTypes = ActionTypes;

            if (allAvailableActionTypes.Length == 0)
            {
                actionTypeMenuToDisplay.AddDisabledItem(new GUIContent("No IBuildingMenuSlotAction implementations found"));
            }
            else
            {
                foreach (Type actionTypeInMenu in allAvailableActionTypes)
                {
                    bool isActionTypeCurrentlySelected = currentActionTypeSelected == actionTypeInMenu;
                    actionTypeMenuToDisplay.AddItem(new GUIContent(actionTypeInMenu.Name), isActionTypeCurrentlySelected, () =>
                    {
                        propertyToUpdate.serializedObject.Update();
                        propertyToUpdate.managedReferenceValue = Activator.CreateInstance(actionTypeInMenu);
                        propertyToUpdate.isExpanded = true;
                        propertyToUpdate.serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            actionTypeMenuToDisplay.ShowAsContext();
        }

        private void DrawActionProperties(SerializedProperty propertyContainingAction, Rect positionRectangle, float startingYPosition, float verticalSpacingBetweenProperties)
        {
            SerializedProperty childPropertyToCopy = propertyContainingAction.Copy();
            SerializedProperty childEndPropertyToReach = childPropertyToCopy.GetEndProperty();
            bool shouldEnterChildrenWhenIterating = true;
            float currentVerticalPositionForDrawing = startingYPosition;

            while (childPropertyToCopy.NextVisible(shouldEnterChildrenWhenIterating) && !SerializedProperty.EqualContents(childPropertyToCopy, childEndPropertyToReach))
            {
                float childPropertyHeightToUse = EditorGUI.GetPropertyHeight(childPropertyToCopy, true);
                Rect childPropertyRectangleToUse = new Rect(positionRectangle.x, currentVerticalPositionForDrawing, positionRectangle.width, childPropertyHeightToUse);
                EditorGUI.PropertyField(childPropertyRectangleToUse, childPropertyToCopy, true);
                currentVerticalPositionForDrawing += childPropertyHeightToUse + verticalSpacingBetweenProperties;
                shouldEnterChildrenWhenIterating = false;
            }
        }

        private static void EnsureDefaultExpanded(SerializedProperty propertyToExpand)
        {
            UnityEngine.Object targetObjectOfProperty = propertyToExpand.serializedObject?.targetObject;
            if (targetObjectOfProperty == null)
            {
                return;
            }

#pragma warning disable CS0618
            string expansionKeyForTracking = targetObjectOfProperty.GetInstanceID().ToString() + ":" + propertyToExpand.propertyPath;
#pragma warning restore CS0618

            if (s_expandedInit.Contains(expansionKeyForTracking))
            {
                return;
            }

            propertyToExpand.isExpanded = true;
            s_expandedInit.Add(expansionKeyForTracking);
        }

        private static Type ResolveType(string fullTypenameToResolve)
        {
            if (string.IsNullOrEmpty(fullTypenameToResolve))
            {
                return null;
            }

            if (s_typeCacheMap.TryGetValue(fullTypenameToResolve, out Type cachedTypeFound))
            {
                return cachedTypeFound;
            }

            string typeAssemblyNameToParse;
            string typeFullNameToParse;

            int spaceIndexInTypename = fullTypenameToResolve.IndexOf(' ');
            if (spaceIndexInTypename >= 0)
            {
                typeAssemblyNameToParse = fullTypenameToResolve.Substring(0, spaceIndexInTypename);
                typeFullNameToParse = fullTypenameToResolve.Substring(spaceIndexInTypename + 1);
            }
            else
            {
                string[] typeNamePartsToParse = fullTypenameToResolve.Split(':');
                if (typeNamePartsToParse.Length != 2)
                {
                    return null;
                }

                typeAssemblyNameToParse = typeNamePartsToParse[0];
                typeFullNameToParse = typeNamePartsToParse[1];
            }

            string qualifiedTypeNameToLoad = typeFullNameToParse + ", " + typeAssemblyNameToParse;
            Type resolvedTypeFromCache = Type.GetType(qualifiedTypeNameToLoad);

            if (resolvedTypeFromCache == null)
            {
                foreach (Assembly domainAssemblyToSearch in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (domainAssemblyToSearch.GetName().Name == typeAssemblyNameToParse)
                    {
                        resolvedTypeFromCache = domainAssemblyToSearch.GetType(typeFullNameToParse);
                        if (resolvedTypeFromCache != null)
                        {
                            break;
                        }
                    }
                }
            }

            if (resolvedTypeFromCache != null)
            {
                s_typeCacheMap[fullTypenameToResolve] = resolvedTypeFromCache;
            }

            return resolvedTypeFromCache;
        }
    }
}