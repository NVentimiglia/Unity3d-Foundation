// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Highlights a default element
    /// </summary>
    [AddComponentMenu("Foundation/Databinding/SelectOnEnable")]
    public class SelectOnEnable : MonoBehaviour
    {
        public bool DisableOnMobile;

        private void OnEnable()
        {
            if (DisableOnMobile && Application.isMobilePlatform)
                return;

            StartCoroutine(OnEnableAsync());
        }

        private IEnumerator OnEnableAsync()
        {
            yield return 1;
            var es = EventSystem.current;


            //if it's an input field, also set the text caret
            var inputfield = gameObject.GetComponent<InputField>();
            if (inputfield != null)
                inputfield.OnPointerClick(new PointerEventData(es));

            es.SetSelectedGameObject(gameObject);
        }
    }
}