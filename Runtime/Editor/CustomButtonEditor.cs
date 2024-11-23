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
        private SerializedProperty subTransitionsProperty;

        private SerializedProperty onClickProperty;

        //Cache
        private CustomButtonBase customButton;
        private UnityEngine.UI.Graphic currentGraphic;

        private List<GraphicTransition> cachedSubTransitions;

        private void OnEnable()
        {
            LoadIconResource();
            interactableProperty = serializedObject.FindProperty("_interactable");
            subTransitionsProperty = serializedObject.FindProperty(nameof(CustomButtonBase.subTransitions));
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

            SerializedProperty transition = serializedObject.FindProperty(nameof(CustomButtonBase.transition));
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

            PropertyField subTransitions = new PropertyField(subTransitionsProperty);
            root.Add(subTransitions);

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

                //odd i know
                UpdateCacheSubTransitions();
            });
        }

        private void UpdateCacheSubTransitions()
        {
            if (cachedSubTransitions == null) cachedSubTransitions = new();
            List<GraphicTransition> current = new(customButton.subTransitions.Count);

            for (int i = 0; i < customButton.subTransitions.Count; i++)
            {
                GraphicTransition subTransition = customButton.subTransitions[i];
                current.Add(subTransition);
            }

            //reset old ones
            for (int i = 0;i < cachedSubTransitions.Count; i++)
            {
                if (!current.Contains(cachedSubTransitions[i]))
                {
                    cachedSubTransitions[i].ResetTransitions();
                }
            }

            cachedSubTransitions = current;
            customButton.UpdateButtonState();
            if (!Application.isPlaying)
                EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}
#endif