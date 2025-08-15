using SimpleLocalization.Runtime;
using UnityEditor;
using UnityEngine;

namespace SimpleLocalization.Editor
{
    /// <summary>
    /// Adds "Sync" button to LocalizationSync script.
    /// </summary>
    [CustomEditor(typeof(LocalizedText))]
    public class LocalizedTextEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Localization Editor"))
            {
                LocalizationEditorWindow.Open();
            }
        }
    }
}