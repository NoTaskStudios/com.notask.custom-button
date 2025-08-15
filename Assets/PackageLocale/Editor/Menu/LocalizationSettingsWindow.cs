using SimpleLocalization.Runtime;
using UnityEditor;
using UnityEngine;

namespace SimpleLocalization.Scripts.Editor.Menu
{
    public class LocalizationSettingsWindow : EditorWindow
    {
        [MenuItem("Tools/SimpleLocalization/Settings")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationSettingsWindow>("Localization Settings");
        }

        private void OnGUI()
        {
            GUILayout.Label("Configuração de Linguagem", EditorStyles.boldLabel);

            if (LocalizationManager.Dictionary.Count == 0)
                LocalizationManager.Read();

            var langs = new System.Collections.Generic.List<string>(LocalizationManager.Dictionary.Keys);
            var index = langs.IndexOf(LocalizationManager.Language);
            var newIndex = EditorGUILayout.Popup("Idioma atual", index, langs.ToArray());

            if (newIndex != index)
                LocalizationManager.Language = langs[newIndex];
        }
    }
}