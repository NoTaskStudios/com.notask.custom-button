#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace CustomButton
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CustomButtonClass))]
    public class CustomButtonEditor : Editor
    {
        private Texture2D customIcon;
        private SerializedProperty interactableProperty;
        // General
        private SerializedProperty opacityOnChildrenProperty;
        private SerializedProperty offsetOnChildrenProperty;
        // Color Tint
        private SerializedProperty invertTextColorProperty;
        private SerializedProperty targetGraphicProperty;
        private SerializedProperty blockColorsProperty;
        private Color previousNormalColor;
        // Sprite Swap
        private SerializedProperty spriteSwapProperty;
        private SerializedProperty offsetVectorChildrenProperty;
        // Animation Preset
        private SerializedProperty presetEvtDownProperty;
        private SerializedProperty presetEvtUpProperty;
        private SerializedProperty presetEvtEnterProperty;
        private SerializedProperty presetEvtExitProperty;
        //Tab Toogles
        private bool showGeneralProperties = true;
        private bool showColorProperties;
        private bool showSpriteProperties;
        private bool showAnimationProperties;
        private SerializedProperty activeColorTintProperty;
        private SerializedProperty activeSpriteSwapProperty;
        private SerializedProperty activeAnimationProperty;
        private SerializedProperty applyBlinkHighlightedProperty;
        private SerializedProperty onClickProperty;

        private bool currentInteractable;
        
        private const string SelectedTabPreferenceKeyPrefix = "SelectedTabPreferenceKey_";
        private int selectedTab;
        private void OnEnable()
        {
            LoadIconResource();

            interactableProperty = serializedObject.FindProperty("_interactable");
            onClickProperty = serializedObject.FindProperty("onClick");
            // General
            opacityOnChildrenProperty = serializedObject.FindProperty(nameof(CustomButtonBase.childrenColorOpacityOnly));
            offsetOnChildrenProperty = serializedObject.FindProperty("applyOffsetOnChildren");
            // Color Tint
            targetGraphicProperty = serializedObject.FindProperty("targetGraphic");
            invertTextColorProperty = serializedObject.FindProperty("applyInvertColorOnTexts");
            blockColorsProperty = serializedObject.FindProperty("blockColors");
            previousNormalColor = ((CustomButtonClass)target).blockColors.normalColor;
            // Sprite Swap
            spriteSwapProperty = serializedObject.FindProperty("spriteState");
            offsetVectorChildrenProperty = serializedObject.FindProperty("offsetVectorChildren");
            applyBlinkHighlightedProperty = serializedObject.FindProperty("applyBlinkHighlighted");
            // Animation Preset
            presetEvtDownProperty = serializedObject.FindProperty("animationEventDown");
            presetEvtUpProperty = serializedObject.FindProperty("animationEventUp");
            presetEvtEnterProperty = serializedObject.FindProperty("animationEventEnter");
            presetEvtExitProperty = serializedObject.FindProperty("animationEventExit");
            //Tab System
            activeColorTintProperty = serializedObject.FindProperty("activeColorTint");
            activeSpriteSwapProperty = serializedObject.FindProperty(nameof(CustomButtonBase.activeSpriteSwap));
            activeAnimationProperty = serializedObject.FindProperty("activeAnimation");
            selectedTab = EditorPrefs.GetInt(GetGameObjectKey(target), 0);
            SelectTab(selectedTab);
        }
        private void LoadIconResource()
        {
            const string assetPath = "Icons/icon_customButton";
            var sprite = Resources.Load<Texture2D>(assetPath);
            customIcon = sprite;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var customButtonBase = (CustomButtonBase)target;
            EditorGUIUtility.SetIconForObject(target, customIcon);
            EditorGUI.BeginChangeCheck();
            var interactableValue = EditorGUILayout.Toggle("Interactable", interactableProperty.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                ((CustomButtonClass)target).Interactable = interactableValue;
                interactableProperty.boolValue = interactableValue;
                interactableProperty.serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
                EditorUtility.SetDirty(customButtonBase);
            }
            string[] tabs = { "General", "Color Tint", "Sprite Swap", "Animation" };
            selectedTab = GUILayout.Toolbar(selectedTab, tabs);
            if (selectedTab != EditorPrefs.GetInt(GetGameObjectKey(target), 0))
            {
                EditorPrefs.SetInt(GetGameObjectKey(target), selectedTab);
                SelectTab(selectedTab);
            }
            EditorGUILayout.Space();
            if (showGeneralProperties)
            {
                EditorGUILayout.PropertyField(targetGraphicProperty);
                const int size = 16;
                var colorTintLabel = new GUIContent("Status Color Tint:");
                var spriteSwapLabel = new GUIContent("Status Sprite Swap:");
                var animationLabel = new GUIContent("Status Animation Preset:");
                var labelStyle = GUI.skin.label;
                var calcColortint = labelStyle.CalcSize(colorTintLabel).x;
                var calcSpriteSwap = labelStyle.CalcSize(spriteSwapLabel).x;
                var calcAnimation = labelStyle.CalcSize(animationLabel).x;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(colorTintLabel , GUILayout.Width(calcColortint)); // Define a largura baseada no tamanho do rótulo
                GUILayout.Label(CreateRoundTexture(size, SetColorStatus(customButtonBase.activeColorTint)), GUILayout.Width(size), GUILayout.Height(size));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(spriteSwapLabel , GUILayout.Width(calcSpriteSwap)); // Define a largura baseada no tamanho do rótulo
                GUILayout.Label(CreateRoundTexture(size, SetColorStatus(customButtonBase.activeSpriteSwap)), GUILayout.Width(size), GUILayout.Height(size));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(animationLabel , GUILayout.Width(calcAnimation)); // Define a largura baseada no tamanho do rótulo
                GUILayout.Label(CreateRoundTexture(size, SetColorStatus(customButtonBase.activeAnimation)), GUILayout.Width(size), GUILayout.Height(size));
                EditorGUILayout.EndHorizontal();
            }
            if (showColorProperties)
            {
                var activeColor = EditorGUILayout.Toggle("Active Color Tint", activeColorTintProperty.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    activeColorTintProperty.boolValue = activeColor;
                    activeColorTintProperty.serializedObject.ApplyModifiedProperties();
                }
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                var invertColorText = EditorGUILayout.Toggle("Invert Color Text", invertTextColorProperty.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    invertTextColorProperty.boolValue = invertColorText;
                    invertTextColorProperty.serializedObject.ApplyModifiedProperties();
                }
                EditorGUI.BeginChangeCheck();
                var opacityChildrenText = EditorGUILayout.Toggle("Apply Opacity Children", opacityOnChildrenProperty.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    opacityOnChildrenProperty.boolValue = opacityChildrenText;
                    opacityOnChildrenProperty.serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.PropertyField(blockColorsProperty);
                EditorGUI.indentLevel--;
                // Só aplicar esta fução caso mexa no colorpicker no inpector
                //customButtonBase.UpdateColor(customButtonBase.blockColors.normalColor);
                
                EditorUtility.SetDirty(customButtonBase);
            }
            
            if (showSpriteProperties)
            {
                var activeSprite = EditorGUILayout.Toggle("Active Sprite Swap", activeSpriteSwapProperty.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    activeSpriteSwapProperty.boolValue = activeSprite;
                    activeSpriteSwapProperty.serializedObject.ApplyModifiedProperties();
                }
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                var offsetChildren = EditorGUILayout.Toggle("Apply Offset Children", offsetOnChildrenProperty.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    offsetOnChildrenProperty.boolValue = offsetChildren;
                    offsetOnChildrenProperty.serializedObject.ApplyModifiedProperties();
                }
                EditorGUI.BeginChangeCheck();
                var activeBlink = EditorGUILayout.Toggle("Apply Blink Highlighted", applyBlinkHighlightedProperty.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    applyBlinkHighlightedProperty.boolValue = activeBlink;
                    applyBlinkHighlightedProperty.serializedObject.ApplyModifiedProperties();
                }
                if (offsetChildren)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(offsetVectorChildrenProperty);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(spriteSwapProperty, true);
                EditorGUI.indentLevel--;
            }
            if (showAnimationProperties)
            {
                var activeAnimation = EditorGUILayout.Toggle("Active Animation Preset", activeAnimationProperty.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    activeAnimationProperty.boolValue = activeAnimation;
                    activeAnimationProperty.serializedObject.ApplyModifiedProperties();
                }
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(presetEvtDownProperty, true);
                EditorGUILayout.PropertyField(presetEvtUpProperty, true);
                EditorGUILayout.PropertyField(presetEvtEnterProperty, true);
                EditorGUILayout.PropertyField(presetEvtExitProperty, true);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(onClickProperty);
            serializedObject.ApplyModifiedProperties();
        }

        private static Color SetColorStatus(bool isActive) => isActive ? Color.green : Color.red;
        private static string GetGameObjectKey(Object targetObject) => SelectedTabPreferenceKeyPrefix + targetObject.name + "_" + targetObject.GetType().Name;
        private void SelectTab(int tab)
        {
            ResetTabs();
            switch (tab)
            {
                case 0: // General
                    showGeneralProperties = true;
                    break;
                case 1: // Color Tint
                    showColorProperties = true;
                    break;
                case 2: // Sprite Swap
                    showSpriteProperties = true;
                    break;
                case 3: // Animation
                    showAnimationProperties = true;
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }
        private static Texture2D CreateRoundTexture(int size, Color color)
        {
            var texture = new Texture2D(size, size);
            var pixels = new Color[size * size];

            var radius = size / 2f;
            var sqrRadius = radius * radius;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - radius;
                    var dy = y - radius;
                    var distance = dx * dx + dy * dy;

                    if (distance <= sqrRadius)
                    {
                        pixels[y * size + x] = color;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }
        private void ResetTabs()
        {
            showGeneralProperties = false;
            showColorProperties = false;
            showSpriteProperties = false;
            showAnimationProperties = false;
        }
    }
}
#endif