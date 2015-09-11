// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using Foundation.Localization;
using UnityEditor;

namespace Assets.Foundation.Localization.Editor
{
    /// <summary>
    /// Watches for file changes and propagates the changes to the service
    /// </summary>
    public class LocalizationPostprocessor : AssetPostprocessor
    {
        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            LocalizationInitializer.Startup();

            // Changed assets
            foreach (string asset in importedAssets)
            {
                // if in localization
                if (asset.Contains("Localization"))
                {
                    //reload
                    var service = LocalizationService.Instance;
                    if (service != null)
                        service.LoadTextAssets();

                    //only once
                    return;
                }
            }

        }
    }
}