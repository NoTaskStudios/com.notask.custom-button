#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomButton
{
    [CustomPropertyDrawer(typeof(GraphicTransition))]
    public class GraphicTransitionDrawer : PropertyDrawer
    {
        private int selectedTab;
        // Color Tint
        private SerializedProperty activeColorTintProperty;
        private SerializedProperty activeSpriteSwapProperty;
        private SerializedProperty activeAnimationProperty;

        private Action<int> onTabChanged;

        //Cache
        private GraphicTransition graphicTransition = null;
        private UnityEngine.UI.Graphic currentGraphic;
        VisualElement colorTintTab;
        VisualElement spriteSwapTab;
        VisualElement animationTab;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            graphicTransition = property.boxedValue as GraphicTransition;
            //SerializationCheck
            if (graphicTransition.badlySerialized)//no default values, fix it
            {
                graphicTransition = new GraphicTransition(null);
                property.boxedValue = graphicTransition;
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }

            #region Properties
            SerializedProperty targetGraphicProperty = property.FindPropertyRelative(nameof(GraphicTransition.targetGraphic));
            // Color Tint
            SerializedProperty blockColorsProperty = property.FindPropertyRelative(nameof(GraphicTransition.blockColors));
            SerializedProperty invertTextColorProperty = property.FindPropertyRelative(nameof(GraphicTransition.invertColorOnTexts));
            SerializedProperty childColorProperty = property.FindPropertyRelative(nameof(GraphicTransition.changeChildrenColor));
            SerializedProperty childAlphaProperty = property.FindPropertyRelative(nameof(GraphicTransition.changeChildrenAlpha));
            // Sprite Swap
            SerializedProperty spriteSwapProperty = property.FindPropertyRelative(nameof(GraphicTransition.spriteState));
            // Animation Preset
            SerializedProperty normalAnimProperty = property.FindPropertyRelative(nameof(GraphicTransition.normalAnimation));
            SerializedProperty highlightedAnimProperty = property.FindPropertyRelative(nameof(GraphicTransition.highlightedAnimation));
            SerializedProperty pressedAnimProperty = property.FindPropertyRelative(nameof(GraphicTransition.pressedAnimation));
            SerializedProperty selectedAnimProperty = property.FindPropertyRelative(nameof(GraphicTransition.selectedAnimation));
            SerializedProperty disabledAnimProperty = property.FindPropertyRelative(nameof(GraphicTransition.disabledAnimation));
            //Tab System
            activeColorTintProperty = property.FindPropertyRelative(nameof(GraphicTransition.colorTintTransition));
            activeSpriteSwapProperty = property.FindPropertyRelative(nameof(GraphicTransition.spriteSwapTransition));
            activeAnimationProperty = property.FindPropertyRelative(nameof(GraphicTransition.animationTransition));
            #endregion

            VisualElement root = new();

            PropertyField targetGraphic = new PropertyField(targetGraphicProperty);
            currentGraphic = graphicTransition.targetGraphic;
            targetGraphic.RegisterValueChangeCallback((evt) =>
            {
                if (currentGraphic)
                {
                    currentGraphic.CrossFadeColor(Color.white, 0, true, true);
                    if (currentGraphic as UnityEngine.UI.Image)
                    {
                        UnityEngine.UI.Image img = (UnityEngine.UI.Image)currentGraphic;
                        img.overrideSprite = null;
                    }
                }
                currentGraphic = (UnityEngine.UI.Graphic)evt.changedProperty.objectReferenceValue;
                evt.changedProperty.serializedObject.ApplyModifiedProperties();
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
            colorTintTab.style.display = DisplayStyle.None;

            PropertyField colorBlockField = new(blockColorsProperty);
            colorTintTab.Add(colorBlockField);

            PropertyField invertTextColorField = new(invertTextColorProperty);
            colorTintTab.Add(invertTextColorField);

            PropertyField childColorField = new(childColorProperty);
            colorTintTab.Add(childColorField);

            PropertyField childAlphaField = new(childAlphaProperty);
            colorTintTab.Add(childAlphaField);
            #endregion

            #region SpriteTab
            //Button SpriteTab
            spriteSwapTab = new VisualElement();
            root.Add(spriteSwapTab);
            tabWindow.Add(TabButton("Sprite Swap", 1, activeSpriteSwapProperty, spriteSwapTab, tabWindow));
            //spriteSwapTab.Add(new Label("sprite tab"));
            spriteSwapTab.style.display = DisplayStyle.None;
            PropertyField spriteSwap = new(spriteSwapProperty);
            //spriteSwap.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
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
            //normalAnimation.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            animationTab.Add(normalAnimation);
            PropertyField highlightedAnimation = new(highlightedAnimProperty);
            //highlightedAnimation.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            animationTab.Add(highlightedAnimation);
            PropertyField pressedAnimation = new(pressedAnimProperty);
            //pressedAnimation.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            animationTab.Add(pressedAnimation);
            PropertyField selectedAnimation = new(selectedAnimProperty);
            //selectedAnimation.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            animationTab.Add(selectedAnimation);
            PropertyField disabledAnimation = new(disabledAnimProperty);
            //disabledAnimation.RegisterValueChangeCallback((evt) => customButton.UpdateButtonState());
            animationTab.Add(disabledAnimation);
            #endregion

            VerifyTabs();

            if (!activeColorTintProperty.boolValue)
            {
                if (activeSpriteSwapProperty.boolValue) ChangeTab(1);
                else if (activeAnimationProperty.boolValue) ChangeTab(2);
            }
            else ChangeTab(0);

            return root;
        }

        private void ChangeTab(int tabID)
        {
            selectedTab = tabID;
            onTabChanged?.Invoke(tabID);
        }

        private void VerifyTabs()
        {
            int activeTabs = 0;
            if (activeColorTintProperty.boolValue) activeTabs++;
            if (activeSpriteSwapProperty.boolValue) activeTabs++;
            if (activeAnimationProperty.boolValue) activeTabs++;

            if (activeTabs == 0)
            {
                colorTintTab.style.display = DisplayStyle.None;
                spriteSwapTab.style.display = DisplayStyle.None;
                animationTab.style.display = DisplayStyle.None;
            }
            else if (activeTabs == 1)
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
                tabwindow.style.flexDirection = tabwindow.resolvedStyle.width <= 326 ? FlexDirection.Column : FlexDirection.Row;
            });

            Toggle enableToggle = new Toggle();
            enableToggle.value = enableProperty.boolValue;
            enableToggle.style.width = 14;
            enableToggle.RegisterValueChangedCallback((evt) =>
            {

                enableProperty.boolValue = evt.newValue;
                enableProperty.serializedObject.ApplyModifiedProperties();
                enableProperty.serializedObject.Update();
                graphicTransition.ResetTransitions();
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