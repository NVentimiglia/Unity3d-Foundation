using UnityEditor;
using UnityEngine;

namespace Foundation.Editor
{
    public class DeletePlayerPrefsEditor
    {
        [MenuItem("Tools/Foundation/Delete All Player Prefs")]
        public static void CreateAppServices()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("PlayerPrefs.DeleteAll();");
        }
    }
}