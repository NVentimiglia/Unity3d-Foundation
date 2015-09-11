// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System.IO;
using Foundation.Localization;
using UnityEditor;
using UnityEngine;

namespace Assets.Foundation.Localization.Editor
{
    [InitializeOnLoad]
    public class LocalizationInitializer
    {
        [MenuItem("Tools/Foundation/Initialize LocalizationService")]
        public static void Startup()
        {
            {
                var inst = Resources.Load<LocalizationService>("LocalizationService");
                if (inst == null)
                {
                    var path = Application.dataPath + "/Resources";

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    var asset = ScriptableObject.CreateInstance(typeof (LocalizationService));
                    Debug.Log(asset);
                    AssetDatabase.CreateAsset(asset, "Assets/Resources/LocalizationService.asset");
                    AssetDatabase.SaveAssets();

                    Debug.Log("LocalizationService Created");
                }

            }

        }
    }
}
