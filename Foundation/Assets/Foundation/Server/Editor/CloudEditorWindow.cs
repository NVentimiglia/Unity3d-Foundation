// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System.IO;
using Assets.Foundation.Server;
using UnityEditor;
using UnityEngine;

namespace Foundation.Editor
{
    public class CloudEditorWindow : EditorWindow
    {

        [MenuItem("Tools/Foundation/Cloud Settings")]
        public static void ShowWindow()
        {
            GetWindowWithRect<CloudEditorWindow>(new Rect(0, 0, 640, 200), false, "Cloud Settings");
        }

        static void CreateSettings()
        {
            var instance = Resources.Load<CloudConfig>("CloudConfig");
            if (instance == null)
            {
                Debug.Log("Cloud Config Created at Resources/CloudConfig.asset");

                var inst = CreateInstance<CloudConfig>();

                if (!Directory.Exists(Application.dataPath + "/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");

                AssetDatabase.CreateAsset(inst, "Assets/Resources/CloudConfig.asset");

                AssetDatabase.SaveAssets();
            }
        }

        static CloudConfig Target
        {
            get
            {
                if (CloudConfig.Instance == null)
                    CreateSettings();
                return CloudConfig.Instance;
            }
        }
        
        void Documentation()
        {
            Application.OpenURL("http://unity3dfoundation.com/Wiki");
        }
        
        void OnGUI()
        {
            GUILayout.BeginHorizontal(GUILayout.MinHeight(64));

            GUILayout.Label("Unity3d Foundation", new GUIStyle
            {
                fontSize = 32,
                padding = new RectOffset(16, 0, 16, 0),
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }


            });

            GUILayout.EndHorizontal();

            //
            GUILayout.Label("Application Key");
            Target.Key = EditorGUILayout.TextField(Target.Key);
            EditorStyles.label.wordWrap = true;
            GUILayout.Space(16);
            
            GUILayout.Label("Service URL");
            Target.Path = EditorGUILayout.TextField(Target.Path);
            GUILayout.Space(16);
            //
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset to Default"))
            {

                Target.Key = "";
                Target.Path = "http://unity3dfoundation.com";
            }
            if (GUILayout.Button("Documentation"))
            {
                Documentation();
            }
            EditorGUILayout.EndHorizontal();
            //
            EditorUtility.SetDirty(Target);
        }
    }
}