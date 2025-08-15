using System;
using System.Collections.Generic;
using System.Linq;
using SimpleLocalization.Runtime;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace OPAGames.SimpleLocalization.Editor.Menu
{
    [Overlay(typeof(SceneView), "Fine Localization", true), Icon("Assets/SimpleLocalization/Editor/Icons/translate.png")]
    public class LocalizationOverlay : Overlay
    {
        private Texture icon { get; set; }
        private Texture2D _icon;

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();

            if (LocalizationManager.Dictionary.Count == 0)
                LocalizationManager.Read();

            var langs = new List<string>(LocalizationManager.Dictionary.Keys
                .Where(k => !string.Equals(k, "KEY", StringComparison.OrdinalIgnoreCase)));
            var popupField = new PopupField<string>("Idioma", langs, LocalizationManager.Language);

            popupField.style.minWidth = 160;
            root.Add(popupField);

            var useEditorLanguage = EditorPrefs.GetBool("FineLocalization_UseEditorLanguage", false);
            popupField.SetEnabled(useEditorLanguage);

            var toggle = new Toggle("Forçar linguagem do Editor") { value = useEditorLanguage };
            toggle.RegisterValueChangedCallback(evt =>
            {
                EditorPrefs.SetBool("FineLocalization_UseEditorLanguage", evt.newValue);
                popupField.SetEnabled(evt.newValue);
            });

            root.Insert(0, toggle);

            popupField.RegisterValueChangedCallback(evt =>
            {
                LocalizationManager.Language = evt.newValue;
                EditorPrefs.SetString("FineLocalization_SelectedLanguage", evt.newValue);
            });

            return root;
        }

        public override void OnCreated()
        {
            _icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SimpleLocalization/Editor/Icons/translate.png");

            if (_icon != null)
            {
                icon = _icon;
            }
            else
            {
                Debug.LogWarning("[Fine Localization] Ícone não encontrado.");
            }
        }
    }
}