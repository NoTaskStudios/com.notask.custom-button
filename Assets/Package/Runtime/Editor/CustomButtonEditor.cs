#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace CustomButton
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CustomButtonClass))]
    public class CustomButtonEditor : Editor
    {
        private Texture2D customIcon;
        private Texture2D circleIcon;
        private SerializedProperty interactableProperty;

        private SerializedProperty onClickProperty;

        //Cache
        private CustomButtonBase customButton;
        private UnityEngine.UI.Graphic currentGraphic;

        private void OnEnable()
        {
            LoadIconResource();
            interactableProperty = serializedObject.FindProperty("_interactable");
            onClickProperty = serializedObject.FindProperty("onClick");
        }
        private void LoadIconResource()
        {
            const string assetPath = "Icons/icon_customButton";
            var sprite = Resources.Load<Texture2D>(assetPath);
            customIcon = sprite;
        }

        public override VisualElement CreateInspectorGUI()
        {
            #region Setup
            serializedObject.Update();
            customButton = target as CustomButtonBase;
            EditorGUIUtility.SetIconForObject(target, customIcon);
            #endregion

            VisualElement root = new();

            PropertyField interactable = new PropertyField(interactableProperty);
            interactable.RegisterValueChangeCallback(_ => customButton.UpdateButtonState());
            root.Add(interactable);

            SerializedProperty transition = serializedObject.FindProperty(nameof(CustomButtonBase.Transition));
            PropertyField transitionField = new PropertyField(transition);
            transitionField.RegisterValueChangeCallback(_ => customButton.UpdateButtonState());
            root.Add(transitionField);

            VisualElement padding = new();
            padding.style.height = 20;
            root.Add(padding);

            VisualElement onChangeEvent = OnChangeEvent();
            root.Add(onChangeEvent);
            
            padding = new();
            padding.style.height = 8;
            root.Add(padding);

            customButton.UpdateButtonState();

            return root;
        }

        private VisualElement OnChangeEvent()
        {
            return new IMGUIContainer(() =>
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(onClickProperty);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
            });
        }
    }
}
#endif