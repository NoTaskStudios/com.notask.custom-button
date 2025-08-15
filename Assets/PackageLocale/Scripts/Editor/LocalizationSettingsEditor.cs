using SimpleLocalization.Runtime;
using UnityEditor;

namespace SimpleLocalization.Editor
{
    [CustomEditor(typeof(LocalizationSettings))]
    public class LocalizationSettingsEditor : UnityEditor.Editor 
    {
        public override void OnInspectorGUI()
        {
            var settings = (LocalizationSettings) target;

            settings.DisplayHelp();
            DrawDefaultInspector();
            settings.DisplayButtons();
            settings.DisplayWarnings();
        }
    }
}