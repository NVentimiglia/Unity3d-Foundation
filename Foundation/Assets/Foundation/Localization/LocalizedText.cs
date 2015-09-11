// --------------------------------------
//  Unity Foundation
//  LocalizedLabel.cs
//  copyright (c) 2014 Nicholas Ventimiglia, http://avariceonline.com
//  All rights reserved.
//  -------------------------------------
// 

using UnityEngine;
using UnityEngine.UI;

namespace Foundation.Localization
{
    /// <summary>
    /// Static Localized Text
    /// </summary>
    [RequireComponent(typeof (Text))]
    [AddComponentMenu("Foundation/Localization/LocalizedText")]
    public class LocalizedText : MonoBehaviour
    {
        [HideInInspector]
        public string File;

        [HideInInspector]
        public string Filter;

        [HideInInspector]
        public string Key;

        protected string Fallback;

        [HideInInspector] public string Value;

      
        private void Awake()
        {
            OnLocalization(LocalizationService.Instance);
            LocalizationService.OnLanguageChanged += OnLocalization;
        }

        private void OnDestroy()
        {
            LocalizationService.OnLanguageChanged -= OnLocalization;
        }

        public void OnLocalization(LocalizationService localization)
        {
            if (localization == null)
                return;

            var label = GetComponent<Text>();

            GetComponent<Text>().text = localization.GetFromFile(File, Key, label.text);
        }
    }
}