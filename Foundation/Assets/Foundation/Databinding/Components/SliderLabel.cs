// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Binding a UI.Text to a Slider's value
    /// </summary>
    [RequireComponent(typeof (Text))]
    [AddComponentMenu("Foundation/Databinding/SliderLabel")]
    public class SliderLabel : MonoBehaviour
    {
        public string FormatString = string.Empty;
        protected bool IsInit;
        protected Text Label;
        public bool MakeInt;
        protected Slider MySlider;

        protected void Awake()
        {
            Label = GetComponent<Text>();
            MySlider = GetComponentInParent<Slider>();
            MySlider.onValueChanged.AddListener(SetPercent);
            SetPercent(MySlider.value);
        }

        public void SetPercent(float value)
        {
            if (Label)
            {
                if (MakeInt)
                {
                    var v = Mathf.RoundToInt(value*100);
                    if (string.IsNullOrEmpty(FormatString))
                    {
                        Label.text = v.ToString();
                    }
                    else
                    {
                        Label.text = string.Format(FormatString, v);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(FormatString))
                    {
                        Label.text = value.ToString();
                    }
                    else
                    {
                        Label.text = string.Format(FormatString, value);
                    }
                }
            }
        }
    }
}