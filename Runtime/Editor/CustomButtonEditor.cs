#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
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
        private SerializedProperty activeColorTintProperty;
        private SerializedProperty invertTextColorProperty;
        private SerializedProperty childColorProperty;
        private SerializedProperty childAlphaProperty;
        private SerializedProperty blockColorsProperty;
        private Color previousNormalColor;
        // Sprite Swap
        private SerializedProperty activeSpriteSwapProperty;
        private SerializedProperty spriteSwapProperty;
        // Animation Preset
        private SerializedProperty activeAnimationProperty;
        private SerializedProperty normalAnimProperty;
        private SerializedProperty highlightedAnimProperty;
        private SerializedProperty pressedAnimProperty;
        private SerializedProperty selectedAnimProperty;
        private SerializedProperty disabledAnimProperty;

        private SerializedProperty onClickProperty;

        private Action<int> onTabChanged;

        //private SerializedProperty offsetVectorChildrenProperty;
        //private SerializedProperty applyBlinkHighlightedProperty;

        //Cache
        private CustomButtonBase customButton;
        private UnityEngine.UI.Graphic currentGraphic;
        VisualElement colorTintTab;
        VisualElement spriteSwapTab;
        VisualElement animationTab;

        private void OnEnable()
        {
            LoadIconResource();

            interactableProperty = serializedObject.FindProperty("_interactable");
            onClickProperty = serializedObject.FindProperty("onClick");
            // General
            opacityOnChildrenProperty = serializedObject.FindProperty(nameof(CustomButtonBase.changeChildrenAlpha));
            offsetOnChildrenProperty = serializedObject.FindProperty("applyOffsetOnChildren");
            // Color Tint
            targetGraphicProperty = serializedObject.FindProperty("targetGraphic");
            invertTextColorProperty = serializedObject.FindProperty(nameof(CustomButtonBase.invertColorOnTexts));
            childColorProperty = serializedObject.FindProperty(nameof(CustomButtonBase.changeChildrenColor));
            childAlphaProperty = serializedObject.FindProperty(nameof(CustomButtonBase.changeChildrenAlpha));
            blockColorsProperty = serializedObject.FindProperty("blockColors");
            previousNormalColor = ((CustomButtonClass)target).blockColors.normalColor;
            // Sprite Swap
            spriteSwapProperty = serializedObject.FindProperty("spriteState");
            //offsetVectorChildrenProperty = serializedObject.FindProperty("offsetVectorChildren");
            //applyBlinkHighlightedProperty = serializedObject.FindProperty("applyBlinkHighlighted");
            // Animation Preset
            normalAnimProperty = serializedObject.FindProperty(nameof(CustomButtonBase.normalAnimation));
            highlightedAnimProperty = serializedObject.FindProperty(nameof(CustomButtonBase.highlightedAnimation));
            pressedAnimProperty = serializedObject.FindProperty(nameof(CustomButtonBase.pressedAnimation));
            selectedAnimProperty = serializedObject.FindProperty(nameof(CustomButtonBase.selectedAnimation));
            disabledAnimProperty = serializedObject.FindProperty(nameof(CustomButtonBase.disabledAnimation));
            //Tab System
            activeColorTintProperty = serializedObject.FindProperty(nameof(CustomButtonBase.colorTintTransition));
            activeSpriteSwapProperty = serializedObject.FindProperty(nameof(CustomButtonBase.spriteSwapTransition));
            activeAnimationProperty = serializedObject.FindProperty(nameof(CustomButtonBase.animationTransition));
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
            tabWindow.style.flexDirection = FlexDirection.Row;
            tabWindow.SetBorder(3, borderColor: ColorExtensions.GrayShade(.1f));
            root.Add(tabWindow);

            #region ColorTab
            //Button ColorTab
            colorTintTab = new VisualElement();
            root.Add(colorTintTab);
            tabWindow.Add(TabButton("Color Tint", 0, activeColorTintProperty, colorTintTab, tabWindow));
            //colorTintTab.Add(new Label("color tab"));
            colorTintTab.style.display = DisplayStyle.None;
            PropertyField invertTextColorField = new(invertTextColorProperty);
            invertTextColorField.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            colorTintTab.Add(invertTextColorField);
            PropertyField childColorField = new(childColorProperty);
            childColorField.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            colorTintTab.Add(childColorField);
            PropertyField childAlphaField = new(childAlphaProperty);
            childAlphaField.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            colorTintTab.Add(childAlphaField);
            PropertyField colorBlockField = new(blockColorsProperty);
            colorBlockField.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            colorTintTab.Add(colorBlockField);
            #endregion

            #region SpriteTab
            //Button SpriteTab
            spriteSwapTab = new VisualElement();
            root.Add(spriteSwapTab);
            tabWindow.Add(TabButton("Sprite Swap", 1, activeSpriteSwapProperty, spriteSwapTab, tabWindow));
            //spriteSwapTab.Add(new Label("sprite tab"));
            spriteSwapTab.style.display = DisplayStyle.None;
            PropertyField spriteSwap = new(spriteSwapProperty);
            spriteSwap.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            spriteSwapTab.Add(spriteSwap);
            #endregion

            #region AnimationTab
            //Button SpriteTab
            animationTab = new VisualElement();
            root.Add(animationTab);
            tabWindow.Add(TabButton("Animation", 2, activeAnimationProperty, animationTab, tabWindow));
            //animationTab.Add(new Label("animation tab"));
            animationTab.style.display = DisplayStyle.None;
            PropertyField normalAnimation = new(normalAnimProperty);
            normalAnimation.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            animationTab.Add(normalAnimation);
            PropertyField highlightedAnimation = new(highlightedAnimProperty);
            highlightedAnimation.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            animationTab.Add(highlightedAnimation);
            PropertyField pressedAnimation = new(pressedAnimProperty);
            pressedAnimation.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            animationTab.Add(pressedAnimation);
            PropertyField selectedAnimation = new(selectedAnimProperty);
            selectedAnimation.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            animationTab.Add(selectedAnimation);
            PropertyField disabledAnimation = new(disabledAnimProperty);
            disabledAnimation.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            animationTab.Add(disabledAnimation);
            #endregion

            VisualElement padding = new();
            padding.style.height = 20;
            root.Add(padding);

            VisualElement onChangeEvent = OnChangeEvent();
            root.Add(onChangeEvent);

            VerifyTabs();

            return root;
        }

        private void ChangeTab(int tabID)
        {
            selectedTab = tabID;
            onTabChanged?.Invoke(tabID);
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

        private void VerifyTabs()
        {
            int activeTabs = 0;
            if (activeColorTintProperty.boolValue) activeTabs ++;
            if (activeSpriteSwapProperty.boolValue) activeTabs ++;
            if (activeAnimationProperty.boolValue) activeTabs ++;

            if (activeTabs == 0)
            {
                colorTintTab.style.display = DisplayStyle.None;
                spriteSwapTab.style.display = DisplayStyle.None;
                animationTab.style.display = DisplayStyle.None;
            }
            else if( activeTabs == 1)
            {
                if (activeColorTintProperty.boolValue) ChangeTab(0);
                else if (activeSpriteSwapProperty.boolValue) ChangeTab(1);
                else if (activeAnimationProperty.boolValue) ChangeTab(2);
            }
        }

        private VisualElement TabButton(string label, int tabID, SerializedProperty enableProperty, VisualElement tabProperties, VisualElement tabwindow)
        {
            Button button = new Button();
            button.text = label;
            button.style.flexGrow = 1;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            button.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                button.style.unityTextAlign = button.resolvedStyle.width switch
                {
                    < 118 and > 95 => TextAnchor.MiddleRight,
                    >= 118 => TextAnchor.MiddleCenter,
                    _ => button.style.unityTextAlign
                };
                tabwindow.style.flexDirection = tabwindow.resolvedStyle.width <= 326 ?  FlexDirection.Column: FlexDirection.Row;
            });
            
            Toggle enableToggle = new Toggle();
            enableToggle.value = enableProperty.boolValue;
            enableToggle.style.width = 14;
            enableToggle.RegisterValueChangedCallback((evt) =>
            {
                
                enableProperty.boolValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
                customButton.UpdateButtonState();
                VerifyTabs();
            });
            button.Add(enableToggle);
            button.clicked += () =>
            {
                ChangeTab(tabID);
            };

            onTabChanged += (id) =>
            {
                bool onTab = id == tabID;
                button.pickingMode = onTab ? PickingMode.Ignore : PickingMode.Position;
                tabProperties.style.display = onTab ? DisplayStyle.Flex : DisplayStyle.None;
            };

            return button;
        }
    }
}
#endif