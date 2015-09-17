// --------------------------------------
//  Unity3d Mvvm Toolkit and Lobby
//  BindingContextEditor.cs
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
    [CustomEditor(typeof (BindingContext))]
    public class BindingContextEditor : UnityEditor.Editor
    {
        public bool SupressUnityEngine;
        protected BindingContext Target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Target = target as BindingContext;

            if (Target == null)
                return;

            ModeSelection();

            switch (Target.ContextMode)
            {
                case BindingContext.BindingContextMode.MonoBinding:
                    MonoSelection();
                    Target.FindModel();
                    break;

                case BindingContext.BindingContextMode.MockBinding:
                    DrawNamespaceDrop();
                    DrawTypeDrop();
                    Target.FindModel();
                    break;

                case BindingContext.BindingContextMode.PropBinding:
                    PropSelection();
                    Target.FindModel();
                    break;

                default:
                    EditorGUILayout.LabelField("Please select a binding mode.");
                    break;
            }
        }

        private void ModeSelection()
        {
            var modes =
                Enum.GetValues(typeof (BindingContext.BindingContextMode))
                    .Cast<BindingContext.BindingContextMode>()
                    .Select(o => o.ToString())
                    .ToArray();
            var index = Array.IndexOf(modes, Target.ContextMode.ToString());
            var i = EditorGUILayout.Popup("Binding Mode", index, modes);

            if (i != index)
            {
                Target.ContextMode =
                    (BindingContext.BindingContextMode) Enum.Parse(typeof (BindingContext.BindingContextMode), modes[i]);
                EditorUtility.SetDirty(target);
            }
        }

        private void MonoSelection()
        {
            Target.ViewModel =
                (MonoBehaviour)
                    EditorGUILayout.ObjectField("ViewModel Componenet", Target.ViewModel, typeof (MonoBehaviour), true);
            if (Target.ViewModel != null)
                EditorGUILayout.LabelField(Target.ViewModel.GetType().ToString());
        }

        private void DrawNamespaceDrop()
        {
            var index = Array.IndexOf(BindingContext.NameSpaces, Target.ModelNamespace);
            var i = EditorGUILayout.Popup("Namespace", index > 0 ? index : 0, BindingContext.NameSpaces.ToArray());

            if (i != index)
            {
                Target.ModelNamespace = i > 0 ? BindingContext.NameSpaces[i] : null;
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawTypeDrop()
        {
            var list =
                BindingContext.ModelTypes.Where(
                    o => o.Namespace == Target.ModelNamespace && !o.HasAttribute<HideInInspector>())
                    .OrderBy(o => o.Name)
                    .ToArray();
            var type = BindingContext.ModelTypes.SingleOrDefault(o => Target.ModelFullName == o.FullName);
            var index = Array.IndexOf(list, type);
            var choices = list.Select(o => o.Name).ToArray();
            var i = EditorGUILayout.Popup("Model Class", index, choices);

            if (i != index)
            {
                Target.DataType = list[i];
                EditorUtility.SetDirty(target);
            }
        }

        private void PropSelection()
        {
            if (Target.Context == null)
            {
                EditorGUILayout.LabelField("Parent BindingContext not found.");

                var p = EditorGUILayout.TextField("Property", Target.PropertyName);

                if (p != Target.PropertyName)
                {
                    Target.PropertyName = p;
                    EditorUtility.SetDirty(target);
                }
            }
            else if (Target.Context.DataType == null)
            {
                EditorGUILayout.LabelField("Parent BindingContext.DataType not found.");

                var p = EditorGUILayout.TextField("Property", Target.PropertyName);

                if (p != Target.PropertyName)
                {
                    Target.PropertyName = p;
                    EditorUtility.SetDirty(target);
                }
            }
            else
            {
                var type = Target.Context.DataType;

                var members = EditorMembersHelper.GetProperties(type);

                if (members.Length == 0)
                {
                    EditorGUILayout.LabelField("This type has no fields or properties.");
                    return;
                }

                var choices =
                    members.Where(o => !o.Module.Assembly.FullName.Contains("UnityEngine")).OrderBy(o => o.Name);

                var labels = choices.Select(o =>
                    string.Format("{0} : {1}",
                        o.Name,
                        o.GetMemberType().Name
                        )).ToArray();

                var names = choices.Select(o => o.Name).ToArray();

                var index = Array.IndexOf(names, Target.PropertyName);

                var i = EditorGUILayout.Popup("Property / Field", index, labels.ToArray());

                if (i != index)
                {
                    Target.PropertyName = names[i];
                    EditorUtility.SetDirty(target);
                }
            }
        }
    }
}