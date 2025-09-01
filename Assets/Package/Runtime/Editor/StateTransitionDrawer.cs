#if UNITY_EDITOR
using CustomButton.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomButton
{
    [CustomPropertyDrawer(typeof(StateTransition<>))]
    public class StateTransitionDrawer: PropertyDrawer
    {
        private static int selectedTab;
        // Color Tint
        private SerializedProperty activeColorTintProperty;
        private SerializedProperty activeSpriteSwapProperty;
        private SerializedProperty activeAnimationProperty;

        private Action<int> onTabChanged;

        //Cache
        private UnityEngine.UI.Graphic currentGraphic;
        VisualElement colorTintTab;
        VisualElement spriteSwapTab;
        VisualElement animationTab;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            #region Properties
            SerializedProperty targetGraphicProperty = property.FindPropertyRelative(nameof(StateTransition<EditorEnum>.targetGraphic));
            //Tab System
            activeColorTintProperty = property.FindPropertyRelative(nameof(StateTransition<EditorEnum>.colorTintTransition));
            activeSpriteSwapProperty = property.FindPropertyRelative(nameof(StateTransition<EditorEnum>.spriteSwapTransition));
            activeAnimationProperty = property.FindPropertyRelative(nameof(StateTransition<EditorEnum>.animationTransition));
            #endregion

            VisualElement root = new();
            root.Add(new Label(property.name));
            VisualElement container = new();
            container.style.paddingLeft = 15;
            root.Add(container);

            PropertyField targetGraphic = new PropertyField(targetGraphicProperty);
            currentGraphic = targetGraphicProperty.objectReferenceValue as UnityEngine.UI.Graphic;
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
            container.Add(targetGraphic);

            VisualElement tabWindow = new VisualElement();
            tabWindow.style.flexDirection = FlexDirection.Row;
            tabWindow.SetBorder(3, borderColor: ColorExtensions.GrayShade(.1f));
            container.Add(tabWindow);

            #region ColorTab
            //Button ColorTab
            colorTintTab = new VisualElement();
            container.Add(colorTintTab);
            tabWindow.Add(TabButton("Color Tint", 0, activeColorTintProperty, colorTintTab, tabWindow));
            colorTintTab.style.display = DisplayStyle.None;
            #endregion

            #region SpriteTab
            //Button SpriteTab
            spriteSwapTab = new VisualElement();
            container.Add(spriteSwapTab);
            tabWindow.Add(TabButton("Sprite Swap", 1, activeSpriteSwapProperty, spriteSwapTab, tabWindow));
            #endregion

            #region AnimationTab
            //Button SpriteTab
            animationTab = new VisualElement();
            container.Add(animationTab);
            tabWindow.Add(TabButton("Animation", 2, activeAnimationProperty, animationTab, tabWindow));
            animationTab.style.display = DisplayStyle.None;
            #endregion

            //States
            var stateNamesProperty = property.FindPropertyRelative(nameof(StateTransition<EditorEnum>.stateNames));
            List<string> stateNames = EditorUtilities.GetArrayElements(stateNamesProperty, p => p.stringValue);
            var statesProperty = property.FindPropertyRelative(nameof(StateTransition<EditorEnum>.states));
            List<SerializedProperty> statesList = EditorUtilities.GetArrayProperties(statesProperty);

            for (int i = 0; i < stateNames.Count; i++)
            {
                string stateName = stateNames[i];
                var p = statesList[i];

                //color
                colorTintTab.Add(new PropertyField(statesList[i].FindPropertyRelative(nameof(GraphicState.color)), stateName));
                spriteSwapTab.Add(new PropertyField(statesList[i].FindPropertyRelative(nameof(GraphicState.sprite)), stateName));
                animationTab.Add(new PropertyField(statesList[i].FindPropertyRelative(nameof(GraphicState.animation)), stateName));
            }

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
            else ChangeTab(selectedTab);
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
                //graphicTransition.ResetTransitions();
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

    public enum EditorEnum
    {
        None,
    }
}
#endif