// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;
using System.Linq;
using Foundation.Localization;
using UnityEditor;
using UnityEngine;

namespace Assets.Foundation.Localization.Editor
{
    /// <summary>
    /// Handles the finding of the Context
    /// </summary>
    [CustomEditor(typeof(LocalizedText), true)]
    public class LocalizedTextEditor : UnityEditor.Editor
    {
        protected LocalizedText Target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LocalizationInitializer.Startup();

            Target = target as LocalizedText;

            if (Application.isPlaying)
                return;

            var service = LocalizationService.Instance;

            if (service == null || service.Strings == null)
            {
                var p = EditorGUILayout.TextField("Key", Target.Key);

                if (p != Target.Key)
                {
                    Target.Key = p;
                    EditorUtility.SetDirty(target);
                }

                EditorGUILayout.LabelField("Error ", "LocalizationService Not Found");
            }
            else
            {
                var files = service.StringsByFile.Select(o => o.Key).ToArray();

                var findex = Array.IndexOf(files, Target.File);

                var fi = EditorGUILayout.Popup("File", findex, files);

                if (fi != findex)
                {
                    Target.File = files[fi];
                    EditorUtility.SetDirty(target);
                }

                //
                if (!string.IsNullOrEmpty(Target.File))
                {
                    //filter
                    Target.Filter = EditorGUILayout.TextField("Filter ", Target.Filter);

                    string[] words;

                    if (!string.IsNullOrEmpty(Target.Filter))
                    {
                        words = service.StringsByFile[Target.File].Select(o => o.Key).Where(o => o.Contains(Target.Filter)).ToArray();
                    }
                    else
                    {
                        words = service.StringsByFile[Target.File].Select(o => o.Key).ToArray();
                    }
                    var index = Array.IndexOf(words, Target.Key);

                    var i = EditorGUILayout.Popup("Keys", index, words);

                    if (i != index)
                    {
                        Target.Key = words[i];
                        Target.Value = service.Get(Target.Key, string.Empty);
                        EditorUtility.SetDirty(target);
                    }
                }

            }

            if (!string.IsNullOrEmpty(Target.Value))
            {
                EditorGUILayout.LabelField("Value ", Target.Value);

                Target.GetComponent<UnityEngine.UI.Text>().text = Target.Value;
            }
        }
    }
}


