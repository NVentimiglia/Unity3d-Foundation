// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System.Collections;
using Foundation.Databinding;
using UnityEngine;

namespace Assets.Foundation.Example
{
    /// <summary>
    ///     Example main menu
    /// </summary>
    [AddComponentMenu("Foundation/Examples/ExampleMainMenu")]
    public class ExampleMainMenu : ObservableBehaviour
    {
        [SerializeField] private bool _isVisible = true;

        /// <summary>
        ///     Game Icon, onetime binding
        /// </summary>
        public Texture2D GameIcon;

        /// <summary>
        ///     Game Name, onetime binding
        /// </summary>
        public string GameName;

        /// <summary>
        ///     Reference to the options menu
        /// </summary>
        /// <remarks>
        ///     Protip : Replace with Injector [Import]
        /// </remarks>
        public ExampleOptions Options;

        /// <summary>
        ///     Reference to the scores menu
        /// </summary>
        /// <remarks>
        ///     Protip : Replace with Injector [Import]
        /// </remarks>
        public ExampleList Scores;

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                //Prevent stack overflows
                if (_isVisible == value)
                    return;
                //set the value
                _isVisible = value;
                //notify bound listeners that this has changed
                NotifyProperty("IsVisible", value);
                Debug.Log("ExampleMainMenu.IsVisible " + value);
            }
        }

        /// <summary>
        ///     Open and Wait for close
        /// </summary>
        /// <returns></returns>
        public IEnumerator OpenOptions()
        {
            //switch visibility
            IsVisible = false;
            Options.IsVisible = true;

            //wait for the options menu to close
            while (Options.IsVisible)
            {
                yield return 1;
            }

            //switch visibility on
            IsVisible = true;
        }

        /// <summary>
        ///     Open and Wait for close
        /// </summary>
        /// <returns></returns>
        public IEnumerator OpenScores()
        {
            //switch visibility
            IsVisible = false;
            Scores.IsVisible = true;

            //wait for the options menu to close
            while (Scores.IsVisible)
            {
                yield return 1;
            }

            //switch visibility on
            IsVisible = true;
        }

        protected override void Awake()
        {
            base.Awake();
            Debug.Log("ExampleMainMenu.Awake");
        }
    }
}