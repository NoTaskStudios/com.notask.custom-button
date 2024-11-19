#if UNITY_EDITOR
using CustomButton.Utils;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace CustomButton
{
    public abstract class CustomButtonComponent : EditorWindow
    {
        [MenuItem("GameObject/UI/Custom Button - TextMeshPro", false, 31)]
        private static void AddCustomButtonTMPro(MenuCommand menuCommand)
        {
            /* TODO: Verificação para substituir
            var shouldDestroy = EditorUtility.DisplayDialog("Aviso", "Já existe uma instância do CustomButton. Deseja substituí-la?", "Sim", "Não");
            if (shouldDestroy)
            {
                // Destruir a instância mais recente
                var existingButtons = FindObjectsOfType<CustomButtonBase>();
                if (existingButtons.Length > 0)
                {
                    DestroyImmediate(existingButtons[existingButtons.Length - 1].gameObject);
                }
            }
            */
            var obj = menuCommand.context as GameObject;
            var rectTransform = obj?.GetComponent<RectTransform>();
            var canvas = FindCanvasInHierarchy(menuCommand);

            if (rectTransform != null)
            {
                if (canvas != null && RectTransformUtility.RectangleContainsScreenPoint(canvas.GetComponent<RectTransform>(), rectTransform.position))
                    menuCommand.context = rectTransform.gameObject;
                else
                    menuCommand.context = canvas.gameObject;
            }

            var customButtonObject = new GameObject("Custom Button");
            var textObject = new GameObject("Text (TMP)");

            EditorGUIUtility.PingObject(customButtonObject);
            EditorApplication.delayCall += () => Selection.activeGameObject = customButtonObject;

            var buttonObjectRT = customButtonObject.AddComponent<RectTransform>();
            var textRT = textObject.AddComponent<RectTransform>();
            
            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            
            ApplySpriteButton(customButtonObject.TryGetComponent(out Image image) ? image : customButtonObject.AddComponent<Image>());
            buttonObjectRT.sizeDelta = new Vector2(160f, 30f);
            textRT.sizeDelta = Vector2.zero;

            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;

            textRT.sizeDelta = Vector2.zero;

            var parentObject = menuCommand.context as GameObject;
            if (parentObject != null)
            {
                customButtonObject.transform.SetParent(parentObject.transform, false);
                textObject.transform.SetParent(customButtonObject.transform, false);
            }

            text.fontSize = 17;
            text.alignment = TextAlignmentOptions.Center;
            text.color = ColorUtility.TryParseHtmlString("#323232", out var color) ? color : Color.black;
            text.SetText("Custom Button");
            
            var custombutton = customButtonObject.AddComponent<CustomButtonClass>();
            const string assetShake = "DefaultPresets/ShakePreset";
            var defaultPreset = Resources.Load<AnimationPreset>(assetShake);

            custombutton.transition = new(customButtonObject.GetComponent<Image>());
            Undo.RegisterCreatedObjectUndo(customButtonObject, "Create " + customButtonObject.name);
            custombutton.OnTransformChildrenChanged();
            
        }
        private static void ApplySpriteButton(Image image)
        {
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 7f;
            image.fillCenter = true;
            
            const string assetPath = "Textures/UISprite-CB-Base";
            var sprite = Resources.Load<Sprite>(assetPath);

            if (sprite != null)
                image.sprite = sprite;
        }
        private static Canvas FindCanvasInHierarchy(MenuCommand menuCommand)
        {
            Canvas foundCanvas = null;
            var canvases = FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                if (!canvas.isActiveAndEnabled) continue;
                menuCommand.context = canvas.gameObject;
                foundCanvas = canvas;
            }

            if (!foundCanvas)// if canvas not found, create one
            {
                foundCanvas = CreateCanvas(menuCommand);
            }
            
            //look for eventSystem
            if(!FindFirstObjectByType<EventSystem>()) CreateEventSystem();
            
            return foundCanvas;
        }
        private static Canvas CreateCanvas(MenuCommand menuCommand)
        {
            var canvasObject = new GameObject("Canvas");
            menuCommand.context = canvasObject;

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new("Event System");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
    
}
#endif