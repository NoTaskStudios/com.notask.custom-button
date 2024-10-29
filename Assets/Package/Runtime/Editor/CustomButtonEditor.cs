#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
namespace CustomButton
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CustomButtonClass))]
    public class CustomButtonEditor : Editor
    {
        private Texture2D customIcon;
        private Texture2D circleIcon;
        private SerializedProperty interactableProperty;
        private SerializedProperty targetGraphicProperty;
        private int selectedTab;
        // General
        private SerializedProperty opacityOnChildrenProperty;
        private SerializedProperty offsetOnChildrenProperty;
        // Color Tint
        private SerializedProperty invertTextColorProperty;
        private SerializedProperty blockColorsProperty;
        private Color previousNormalColor;
        // Sprite Swap
        private SerializedProperty spriteSwapProperty;
        private SerializedProperty offsetVectorChildrenProperty;
        // Animation Preset
        private SerializedProperty normalAnimProperty;
        private SerializedProperty highlightedAnimProperty;
        private SerializedProperty pressedAnimProperty;
        private SerializedProperty selectedAnimProperty;
        private SerializedProperty disabledAnimProperty;
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

        //Cache
        private CustomButtonBase customButton;
        private UnityEngine.UI.Graphic currentGraphic;

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
            normalAnimProperty = serializedObject.FindProperty(nameof(CustomButtonBase.normalAnimation));
            highlightedAnimProperty = serializedObject.FindProperty(nameof(CustomButtonBase.highlightedAnimation));
            pressedAnimProperty = serializedObject.FindProperty(nameof(CustomButtonBase.pressedAnimation));
            selectedAnimProperty = serializedObject.FindProperty(nameof(CustomButtonBase.selectedAnimation));
            disabledAnimProperty = serializedObject.FindProperty(nameof(CustomButtonBase.disabledAnimation));
            //Tab System
            activeColorTintProperty = serializedObject.FindProperty("activeColorTint");
            activeSpriteSwapProperty = serializedObject.FindProperty(nameof(CustomButtonBase.activeSpriteSwap));
            activeAnimationProperty = serializedObject.FindProperty("activeAnimation");
        }
        private void LoadIconResource()
        {
            const string assetPath = "Icons/icon_customButton";
            var sprite = Resources.Load<Texture2D>(assetPath);
            customIcon = sprite;
        }

        private static Color SetColorStatus(bool isActive) => isActive ? Color.green : Color.red;
        private Texture2D CreateRoundTexture(int size, Color color)
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
            circleIcon = texture;
            return texture;
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
            interactable.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            root.Add(interactable);

            PropertyField targetGraphic = new PropertyField(targetGraphicProperty);
            currentGraphic = (UnityEngine.UI.Graphic)targetGraphicProperty.objectReferenceValue;
            targetGraphic.RegisterValueChangeCallback((evt) =>
            {
                if (currentGraphic)
                {
                    currentGraphic.CrossFadeColor(Color.white, 0, true, true);
                    if(currentGraphic as UnityEngine.UI.Image)
                    {
                        UnityEngine.UI.Image img = (UnityEngine.UI.Image)currentGraphic;
                        img.overrideSprite = null;
                    }
                }
                currentGraphic = (UnityEngine.UI.Graphic)evt.changedProperty.objectReferenceValue;
                customButton.UpdateButtonState();
            });
            root.Add(targetGraphic);

            VisualElement tabWindow = new VisualElement();
            root.Add(tabWindow);

            //Button GeneralTab
            tabWindow.Add(TabButton("Color Tint", activeColorTintProperty));

            return root;
        }

        private Button TabButton(string label,SerializedProperty enableProperty)
        {
            Button button = new Button();
            button.text = label;
            Toggle enableToggle = new Toggle();
            enableToggle.value = enableProperty.boolValue;
            enableToggle.style.width = 14;
            enableToggle.RegisterValueChangedCallback((evt) =>
            {
                enableProperty.boolValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
                customButton.UpdateButtonState();
            });
            button.Add(enableToggle);

            return button;
        }
    }
}
#endif