// --------------------------------------
//  Unity3d Mvvm Toolkit and Lobby
//  PropertyBinderEditor.cs
//  copyright (c) 2014 Nicholas Ventimiglia, http://avariceonline.com
//  All rights reserved.
//  -------------------------------------
// 

using System;
using System.Linq;
using System.Reflection;
using Foundation.Databinding;
using UnityEditor;
using UnityEngine;

namespace Foundation.Editor
{
    /// <summary>
    ///     Handles the finding of the Context
    /// </summary>
    [CustomEditor(typeof (BindingBase), true)]
    public class BinderEditor : UnityEditor.Editor
    {
        protected BindingBase Target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Target = target as BindingBase;

            if (!Application.isPlaying)
                Target.FindContext();

            if (Target.Context == null)
            {
                EditorGUILayout.LabelField("BindingContext not found.");
                PropertyTextInputs();
            }
            else if (Target.Context.DataType == null)
            {
                EditorGUILayout.LabelField("BindingContext.DataType not found.");
                PropertyTextInputs();
            }
            else
            {
                Target.Init();


                foreach (var binding in Target.GetBindingInfos())
                {
                    if (binding.ShouldShow != null && !binding.ShouldShow())
                        continue;

                    PropertyDropDown(binding);
                }
            }
        }

        private void PropertyDropDown(BindingBase.BindingInfo info)
        {
            var type = Target.Context.DataType;

            var members = new MemberInfo[0];

            // filter
            switch (info.Filters)
            {
                case BindingBase.BindingFilter.Commands:
                    members = EditorMembersHelper.GetMethods(type);
                    break;
                case BindingBase.BindingFilter.Properties:
                    members = EditorMembersHelper.GetProperties(type);
                    break;
            }

            //filter
            if (info.FilterTypes != null)
            {
                members = members.Where(o => info.FilterTypes.Any(t => ValidType(t, o.GetParamaterType()))).ToArray();
            }

            if (members.Length == 0)
            {
                EditorGUILayout.LabelField(string.Format("{0}->{1} has no valid members.", info.BindingName, type.Name));
                return;
            }

            var labels = members.Select(o => string.Format("{0} : {1}", o.Name, o.GetParamaterType())).ToList();

            var names = members.Select(o => o.Name).ToList();

            labels.Insert(0, "Null");
            names.Insert(0, "");

            var index = Array.IndexOf(names.ToArray(), info.MemberName);

            var i = EditorGUILayout.Popup(info.BindingName, index, labels.ToArray());

            if (i != index)
            {
                info.MemberName = names[i];
                EditorUtility.SetDirty(target);
            }
        }

        private void PropertyTextInputs()
        {
            Target.Init();

            foreach (var binding in Target.GetBindingInfos())
            {
                var p = EditorGUILayout.TextField(binding.BindingName, binding.MemberName);

                if (p != binding.MemberName)
                {
                    binding.MemberName = p;
                    EditorUtility.SetDirty(target);
                }
            }
        }

        private bool ValidType(Type filteredType, Type memberType)
        {
            return filteredType.IsAssignableFrom(memberType);
        }
    }
}