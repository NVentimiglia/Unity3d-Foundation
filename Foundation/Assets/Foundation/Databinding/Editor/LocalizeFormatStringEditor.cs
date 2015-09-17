// --------------------------------------
//  Unity3d Mvvm Toolkit and Lobby
//  LocalizedLabelBinderEditor.cs
//  copyright (c) 2014 Nicholas Ventimiglia, http://avariceonline.com
//  All rights reserved.
//  -------------------------------------
// 


using System;
using System.Linq;
using Foundation.Databinding;
using UnityEditor;
using UnityEngine;

namespace Foundation.Editor
{
    /// <summary>
    /// Handles the finding of the Context
    /// </summary>
    [CustomEditor(typeof(LocalizeFormatString), true)]
    public class LocalizeFormatStringEditor : UnityEditor.Editor
    {
        protected LocalizeFormatString Target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Target = target as LocalizeFormatString;

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

                EditorGUILayout.LabelField("Error ", "ILocalizationService Not Found");
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
                    var words = service.StringsByFile[Target.File].Select(o => o.Key).ToArray();
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
                EditorGUILayout.LabelField("Value ", Target.Value);
        }
    }
}