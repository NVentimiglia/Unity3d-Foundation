// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Navigation via the Keyboard Tab key
    /// </summary>
    [AddComponentMenu("Foundation/uGUI/TabNavigation")]
    public class TabNavigation : MonoBehaviour
    {
        public enum DirectionEnum
        {
            UpDown,
            LeftRight
        }

        public Selectable Default;
        public DirectionEnum Direction = DirectionEnum.UpDown;

        private void Awake()
        {
            enabled = enabled && !Application.isMobilePlatform;
        }

        private void LateUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                var system = EventSystem.current;

                var next = Next(system.currentSelectedGameObject.GetComponent<Selectable>());

                if (next == null)
                    next = Default;

                if (next != null)
                {
                    var inputfield = next.GetComponent<InputField>();

                    if (inputfield != null)
                        inputfield.OnPointerClick(new PointerEventData(system));
                            //if it's an input field, also set the text caret

                    system.SetSelectedGameObject(next.gameObject);
                }
            }
        }

        private Selectable Next(Selectable current)
        {
            switch (Direction)
            {
                case DirectionEnum.UpDown:
                    return current.FindSelectableOnDown();
                case DirectionEnum.LeftRight:
                    return current.FindSelectableOnRight();
            }
            return current.FindSelectableOnDown();
        }
    }
}