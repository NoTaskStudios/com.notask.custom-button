#if UNITY_EDITOR
using SimpleLocalization.Runtime;
using UnityEditor;
using UnityEngine;

namespace OPAGames.SimpleLocalization.Editor
{
    [InitializeOnLoad]
    public static class LocalizationEditorHelper
    {
        static LocalizationEditorHelper()
        {
            EditorApplication.playModeStateChanged += ApplyLanguageBeforePlay;
        }

        private static void ApplyLanguageBeforePlay(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            var forceLanguage = EditorPrefs.GetBool("FineLocalization_UseEditorLanguage", false);
            if (!forceLanguage) return;
            var lang = EditorPrefs.GetString("FineLocalization_SelectedLanguage", "en-us");
            LocalizationManager.Language = lang;
            Debug.Log($"[Fine Localization] Force Language To: {lang} in Editor.");
        }
    }
}
#endif