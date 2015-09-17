// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Usefull for dynamic text. Sets the LayoutElement height based on the text's height.
    /// </summary>
    [AddComponentMenu("Foundation/Databinding/TextHeightLayout")]
    public class TextHeightLayout : MonoBehaviour
    {
        public int BaseHeight;
        public bool DebugMode;
        public LayoutElement Element;
        public Text Text;

        private void Awake()
        {
            Text.RegisterDirtyLayoutCallback(Recalculate);
        }

        private IEnumerator Start()
        {
            yield return 1;
            Recalculate();
        }

        [ContextMenu("Recalculate")]
        private void Recalculate()
        {
            if (DebugMode)
            {
                Element.preferredHeight = BaseHeight + Text.preferredHeight;
                Debug.Log(BaseHeight + " " + Text.preferredHeight);
            }
            else
            {
                Element.preferredHeight = BaseHeight + Text.preferredHeight;
            }
        }
    }
}