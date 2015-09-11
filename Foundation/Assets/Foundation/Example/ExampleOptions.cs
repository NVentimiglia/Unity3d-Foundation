// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using Foundation.Databinding;
using Foundation.Databinding.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Foundation.Example
{
    /// <summary>
    ///     Example Options Menu
    /// </summary>
    [AddComponentMenu("Foundation/Examples/ExampleOptions")]
    public class ExampleOptions : ObservableBehaviour
    {
        #region UE

        protected override void Awake()
        {
            base.Awake();
            Load();
        }

        #endregion

        #region view logic

        private bool _isVisible;

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value)
                    return;
                _isVisible = value;
                NotifyProperty("IsVisible", value);
            }
        }

        public void SaveAndClose()
        {
            Save();
            IsVisible = false;
        }

        #endregion

        #region option event

        public Text Output;

        /// <summary>
        ///     Raised when an option value changes
        /// </summary>
        public event Action<string, object> OnOptionChanged;

        public override void NotifyProperty(string memberName, object paramater)
        {
            base.NotifyProperty(memberName, paramater);

            if (OnOptionChanged != null)
                OnOptionChanged(memberName, paramater);

            Output.text = string.Format("{0} = {1}", memberName, paramater);
        }

        #endregion

        #region model

        [SerializeField] private float _soundVolume = 1;

        public float SoundVolume
        {
            get { return _soundVolume; }
            set
            {
                if (_soundVolume == value)
                    return;
                _soundVolume = value;
                NotifyProperty("SoundVolume", value);

                // update service
                AudioManager.SetVolume(AudioLayer.Sfx, value);
                AudioManager.SetVolume(AudioLayer.UISfx, value);
            }
        }

        [SerializeField] private float _musicVolume = 1;

        public float MusicVolume
        {
            get { return _musicVolume; }
            set
            {
                if (_musicVolume == value)
                    return;
                _musicVolume = value;
                NotifyProperty("MusicVolume", value);

                // update service
                AudioManager.SetVolume(AudioLayer.Music, value);
            }
        }

        [SerializeField] private bool _useCensor = true;

        public bool UseCensor
        {
            get { return _useCensor; }
            set { Set(ref _useCensor, value); }
        }

        [SerializeField] private string _userName = "Player 1";

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName == value)
                    return;
                _userName = value;
                NotifyProperty("UserName", value);

                // todo update service
            }
        }

        #endregion

        #region Save / Load

        public void Save()
        {
            PlayerPrefs.SetFloat("ExampleOptions.MusicVolume", MusicVolume);
            PlayerPrefs.SetFloat("ExampleOptions.SoundVolume", SoundVolume);
            PlayerPrefs.SetInt("ExampleOptions.UseCensor", UseCensor ? 1 : 0);
            PlayerPrefs.SetString("ExampleOptions.UserName", UserName);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            MusicVolume = PlayerPrefs.GetFloat("ExampleOptions.MusicVolume", MusicVolume);
            SoundVolume = PlayerPrefs.GetFloat("ExampleOptions.SoundVolume", SoundVolume);
            UseCensor = PlayerPrefs.GetInt("ExampleOptions.UseCensor", UseCensor ? 1 : 0) == 1;
            UserName = PlayerPrefs.GetString("ExampleOptions.UserName", UserName);
        }

        #endregion
    }
}