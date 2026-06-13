/// <summary>
/// Project : Mind Code Interactive
/// Class : ReorderableList.cs
/// Namespace : MindCodeInteractive.ReorderableList.Code.Editor
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.ReorderableList.Code.Editor
{
    public class ReorderableList
    {
        public UnityEditorInternal.ReorderableList Native { get; private set; }

        protected bool UseFoldout { get; set; } = true;
        protected SerializedProperty SourceProperty { get; set; }

        public string DisplayName => SourceProperty.displayName;

        protected bool IsFoldedOut
        {
            get => SourceProperty.isExpanded;
            set => SourceProperty.isExpanded = value;
        }

        protected SerializedProperty ElementPropertyAt(int elementIndex) => SourceProperty.GetArrayElementAtIndex(elementIndex);

        public ReorderableList(SerializedProperty sourceSerializedProperty, bool shouldUseFoldout = true)
        {
            SourceProperty = sourceSerializedProperty;
            UseFoldout = shouldUseFoldout;

            InitializeNative(sourceSerializedProperty);
            InitializeReadyMadeDrawers();
        }

        public ReorderableList(SerializedProperty sourceSerializedProperty, NativeFunctionOptions nativeFunctionOptions, ReadyMadeDrawerOptions readyMadeDrawerOptions, bool shouldUseFoldout = true)
        {
            SourceProperty = sourceSerializedProperty ?? throw new ArgumentNullException(nameof(sourceSerializedProperty));
            UseFoldout = shouldUseFoldout;

            InitializeNative(sourceSerializedProperty, nativeFunctionOptions);
            InitializeReadyMadeDrawers(readyMadeDrawerOptions);
        }

        public ReorderableList(SerializedProperty sourceSerializedProperty, NativeFunctionOptions nativeFunctionOptions, bool shouldUseFoldout = true)
        {
            SourceProperty = sourceSerializedProperty ?? throw new ArgumentNullException(nameof(sourceSerializedProperty));
            UseFoldout = shouldUseFoldout;

            InitializeNative(sourceSerializedProperty, nativeFunctionOptions);
            InitializeReadyMadeDrawers();
        }

        public ReorderableList(SerializedProperty sourceSerializedProperty, ReadyMadeDrawerOptions readyMadeDrawerOptions, bool shouldUseFoldout = true)
        {
            SourceProperty = sourceSerializedProperty ?? throw new ArgumentNullException(nameof(sourceSerializedProperty));
            UseFoldout = shouldUseFoldout;

            InitializeNative(sourceSerializedProperty);
            InitializeReadyMadeDrawers(readyMadeDrawerOptions);
        }

        protected virtual void InitializeNative(SerializedProperty listProperty) => InitializeNative(listProperty, NativeFunctionOptions.Default);

        protected virtual void InitializeNative(SerializedProperty listProperty, NativeFunctionOptions nativeOptions)
        {
            if (listProperty == null)
            {
                return;
            }

            Native = new UnityEditorInternal.ReorderableList(
                listProperty.serializedObject,
                listProperty,
                nativeOptions.Draggable,
                nativeOptions.DisplayHeader,
                nativeOptions.DisplayAddButton,
                nativeOptions.DisplayRemoveButton
            );
        }

        public virtual void InitializeReadyMadeDrawers() => InitializeReadyMadeDrawers(ReadyMadeDrawerOptions.Default);

        public virtual void InitializeReadyMadeDrawers(ReadyMadeDrawerOptions drawerOptions)
        {
            if (drawerOptions.UseReadyMadeHeader)
            {
                AddDrawHeaderCallback();
            }

            if (drawerOptions.UseReadyMadeElement)
            {
                AddDrawElementPropertyCallback();
            }

            if (drawerOptions.UseReadyMadeBackground)
            {
                AddDrawElementBackgroundCallback();
            }
        }

        public virtual void Layout()
        {
            if (Native == null)
            {
                return;
            }

            if (!UseFoldout)
            {
                Native.DoLayoutList();
                return;
            }

            LayoutWithFoldout();
        }

        protected virtual void LayoutWithFoldout()
        {
            IsFoldedOut = EditorGUILayout.Foldout(IsFoldedOut, DisplayName, true);

            if (!IsFoldedOut)
            {
                return;
            }

            Native.DoLayoutList();
        }

        public virtual void AddDrawHeaderCallback()
        {
            if (Native == null)
            {
                return;
            }

            Native.drawHeaderCallback += DrawHeader;
        }

        public virtual void AddDrawHeaderCallback(string headerLabel)
        {
            if (Native == null)
            {
                return;
            }

            Native.drawHeaderCallback += drawRect => DrawHeader(drawRect, headerLabel);
        }

        protected virtual void DrawHeader(Rect headerDrawRect) => EditorGUI.LabelField(headerDrawRect, DisplayName);

        protected virtual void DrawHeader(Rect headerDrawRect, string headerLabel) => EditorGUI.LabelField(headerDrawRect, headerLabel);

        public void AddDrawElementPropertyCallback()
        {
            if (Native == null)
            {
                return;
            }

            Native.drawElementCallback += DrawProperty;
            Native.elementHeightCallback += ElementHeight;
        }

        protected virtual void DrawProperty(Rect elementDrawRect, int elementIndex, bool isElementActive, bool isElementFocused)
            => DrawProperty(elementDrawRect, ElementPropertyAt(elementIndex));

        protected virtual void DrawProperty(Rect propertyDrawRect, SerializedProperty propertyToDraw)
        {
            if (propertyToDraw == null)
            {
                return;
            }

            EditorGUI.PropertyField(
                LayoutUtility.AdjustedRect(propertyDrawRect, propertyToDraw),
                propertyToDraw,
                true
            );
        }

        protected virtual float ElementHeight(int elementIndex) => LayoutUtility.ElementHeight(ElementPropertyAt(elementIndex));

        public void AddDrawElementBackgroundCallback()
        {
            if (Native == null)
            {
                return;
            }

            Native.drawElementBackgroundCallback += (Rect backgroundDrawRect, int backgroundElementIndex, bool isBackgroundActive, bool isBackgroundFocused) =>
                DrawElementBackgroundAlternatively(backgroundDrawRect, backgroundElementIndex, isBackgroundActive, isBackgroundFocused);
        }

        protected virtual void DrawElementBackgroundAlternatively(Rect backgroundDrawRect, int backgroundElementIndex, bool isBackgroundActive, bool isBackgroundFocused)
        {
            if (isBackgroundFocused)
            {
                DrawActiveColor(backgroundDrawRect);
                return;
            }

            if (backgroundElementIndex % 2 != 0)
            {
                return;
            }

            DrawDifferentBackgroundColor(backgroundDrawRect);
        }

        protected virtual void DrawActiveColor(Rect activeColorDrawRect) => BackgroundUtility.DrawElementBackgroundColorActive(activeColorDrawRect);

        protected virtual void DrawDifferentBackgroundColor(Rect differentBackgroundDrawRect) => BackgroundUtility.DrawElementBackgroundColorDifferent(differentBackgroundDrawRect);

        public virtual void AddDrawDropDownCallback(string[] dropDownItemNames, Action<string> onDropDownItemSelected)
        {
            if (Native == null)
            {
                return;
            }

            Native.onAddDropdownCallback += (dropDownRect, reorderableList) =>
                DrawDropDown(dropDownRect, dropDownItemNames, onDropDownItemSelected);
        }

        protected virtual void DrawDropDown(Rect dropDownRect, string[] candidateItemNames, Action<string> onItemSelected)
        {
            GenericMenu dropDownMenu = new GenericMenu();

            for (int i = 0; i < candidateItemNames.Length; i++)
            {
                string itemName = candidateItemNames[i];
                dropDownMenu.AddItem(new GUIContent(itemName), false, () => onItemSelected?.Invoke(itemName));
            }

            dropDownMenu.DropDown(dropDownRect);
        }
    }
}