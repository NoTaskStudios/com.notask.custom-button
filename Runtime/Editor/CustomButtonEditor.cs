#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
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
            subTransitions.RegisterValueChangeCallback(UpdateSubTransitions);
            root.Add(subTransitions);

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

        private void UpdateSubTransitions(SerializedPropertyChangeEvent evt)
        {
            Debug.Log(".");
            if (!evt.changedProperty.isArray) return;

            if (cachedSubTransitions != null)
            {
                for (int i = 0; i < cachedSubTransitions.Count; i++)
                    cachedSubTransitions[i].ResetTransitions();
            }
            
            CacheSubTransitionsList(evt.changedProperty);
            
            for (int i = 0; i < cachedSubTransitions.Count; i++)
                cachedSubTransitions[i].UpdateState(customButton.Interactable ? 
                    SelectionState.Normal : SelectionState.Disabled);
        }

        private void CacheSubTransitionsList(SerializedProperty sp)
        {
            if (cachedSubTransitions == null) cachedSubTransitions = new();
            cachedSubTransitions.Clear();
            if (!sp.isArray) return;
            
            SerializedProperty property = sp.Copy();
            int arrayLength = 0;

            property.Next(true); // skip generic field
            property.Next(true); // advance to array size field

            // Get the array size
            arrayLength = property.intValue;
            int lastIndex = arrayLength - 1;

            property.Next(true); // advance to first array index

            for (int i = 0; i < arrayLength; i++)
            {
                //Debug.Log(property.boxedValue as GraphicTransition);
                cachedSubTransitions.Add(property.boxedValue as GraphicTransition);
                if(i < lastIndex) sp.Next(false); // advance without drilling into children
            }
        }
    }
}
#endif