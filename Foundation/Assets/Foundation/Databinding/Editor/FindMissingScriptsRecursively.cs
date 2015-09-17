// --------------------------------------
//  Unity3d Mvvm Toolkit and Lobby
//  FindMissingScriptsRecursively.cs
//  copyright (c) 2014 Nicholas Ventimiglia, http://avariceonline.com
//  All rights reserved.
//  -------------------------------------
// 

using UnityEditor;
using UnityEngine;

namespace Foundation.Editor
{
    public class FindMissingScriptsRecursively : EditorWindow
    {
        private static int go_count, components_count, missing_count;

        [MenuItem("Window/FindMissingScriptsRecursively")]
        public static void ShowWindow()
        {
            GetWindow(typeof (FindMissingScriptsRecursively));
        }

        public void OnGUI()
        {
            if (GUILayout.Button("Find Missing Scripts in selected GameObjects"))
            {
                FindInSelected();
            }
        }

        private static void FindInSelected()
        {
            var go = Selection.gameObjects;
            go_count = 0;
            components_count = 0;
            missing_count = 0;
            foreach (var g in go)
            {
                FindInGO(g);
            }
            Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count,
                components_count, missing_count));
        }

        private static void FindInGO(GameObject g)
        {
            go_count++;
            var components = g.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                components_count++;
                if (components[i] == null)
                {
                    missing_count++;
                    var s = g.name;
                    var t = g.transform;
                    while (t.parent != null)
                    {
                        s = t.parent.name + "/" + s;
                        t = t.parent;
                    }
                    Debug.Log(s + " has an empty script attached in position: " + i, g);
                }
            }
            // Now recurse through each child GO (if there are any):
            foreach (Transform childT in g.transform)
            {
                //Debug.Log("Searching " + childT.name  + " " );
                FindInGO(childT.gameObject);
            }
        }
    }
}