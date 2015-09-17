// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using UnityEngine;

namespace Foundation.Databinding
{
    /// <summary>
    /// Sets the "Format String" for a TextBinder
    /// </summary>
    /// <remarks>
    /// Format strings use the String.Format method. ie : Hello {0}.
    /// </remarks>
    [RequireComponent(typeof(TextBinder))]
    [AddComponentMenu("Foundation/Databinding/LocalizeFormatString")]
    public class LocalizeFormatString : MonoBehaviour
    {
        [HideInInspector]
        public string File;

        [HideInInspector]
        public string Key;

        protected string Fallback;

        [HideInInspector]
        public string Value;


        private void Awake()
        {
            OnLocalization(LocalizationService.Instance);
            LocalizationService.OnLanguageChanged += OnLocalization;
        }

        private void OnDestroy()
        {
            LocalizationService.OnLanguageChanged -= OnLocalization;
        }

        private void OnLocalization(LocalizationService localization)
        {
            if (localization == null)
                return;

            var label = GetComponent<TextBinder>();

            var data = localization.GetFromFile(File, Key);

            label.FormatString = data;
            label.OnBindingRefresh();
        }
    }
}