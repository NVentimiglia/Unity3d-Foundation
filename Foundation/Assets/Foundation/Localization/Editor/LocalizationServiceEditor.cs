using System;
using System.Linq;
using Assets.Foundation.Localization.Editor;
using UnityEditor;
using UnityEngine;

namespace Foundation.Localization.Editor
{
    [CustomEditor(typeof(LocalizationService), true)]
    public class LocalizationServiceEditor : UnityEditor.Editor
    {
        protected LocalizationService Target;

        
        [MenuItem("Tools/Foundation/Localization Service")]
        public static void ShowWindow()
        {
            LocalizationInitializer.Startup();
            Selection.activeObject = LocalizationService.Instance;
            EditorGUIUtility.PingObject(LocalizationService.Instance);

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LocalizationInitializer.Startup();

            Target = target as LocalizationService;

            if (Application.isPlaying)
                return;
            
            EditorGUILayout.LabelField("Default Language");
            var di = Array.IndexOf(Target.Languages, Target.DefaultLanguage);
            var di2 = EditorGUILayout.Popup(di, Target.Languages.Select(o => o.Name).ToArray());

            if (di != di2)
            {
                Target.DefaultLanguage = Target.Languages[di2];
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.LabelField("Cached Language");
            EditorGUILayout.LabelField(Target.Language.Name, EditorStyles.boldLabel);

            GUILayout.Space(16);

            if (GUILayout.Button("Reset Cached Language"))
            {
                Target.Language = Target.DefaultLanguage;
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Reset Language List"))
            {
                Target.Languages = LanguageInfo.All;
                EditorUtility.SetDirty(target);
            }
        }
    }
}
