// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Utility for playing audio clips from the AudioListener (great for UI sounds or music)
    /// </summary>
    /// <remarks>
    ///     Attach onto all AudioListeners
    /// </remarks>
    [AddComponentMenu("Foundation/Audio/Audio2dListener")]
    [RequireComponent(typeof (AudioListener))]
    public class Audio2DListener : MonoBehaviour
    {
        #region static

        /// <summary>
        ///     Static Instance
        /// </summary>
        protected static Audio2DListener Instance { get; set; }

        /// <summary>
        ///     Returns an instance of the Audio2dListener
        /// </summary>
        /// <returns></returns>
        public static Audio2DListener GetInstance()
        {
            if (Instance == null)
            {
                // create a new one
                var found = FindObjectOfType<AudioListener>();

                if (found == null)
                {
                    Debug.LogError("No AudioListener found !");
                    return null;
                }

                Instance = found.gameObject.AddComponent<Audio2DListener>();
            }

            return Instance;
        }

        #endregion

        #region protected members

        /// <summary>
        ///     Listing of all child audio sources
        /// </summary>
        protected List<AudioSource> Children = new List<AudioSource>();

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }

        #endregion

        #region public members

        /// <summary>
        ///     Returns the next available Audio Source
        /// </summary>
        /// <returns></returns>
        public static AudioSource GetNext()
        {
            if (GetInstance() == null)
                return null;

            for (var i = 0; i < Instance.Children.Count; i++)
            {
                if (Instance.Children[i] == null)
                {
                    // this happens on level changing.
                    return null;
                }

                if (Instance.Children[i].isPlaying)
                    continue;

                return Instance.Children[i];
            }

            var go = new GameObject("_Audio2d");
            go.transform.parent = Instance.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            var s = go.AddComponent<AudioSource>();

            Instance.Children.Add(s);

            return s;
        }

        /// <summary>
        ///     Plays the Audio in the UISFX Layer
        /// </summary>
        /// <param name="c">clip</param>
        /// <param name="v">volume</param>
        /// <returns>A playing audio source</returns>
        public static AudioSource PlayUI(AudioClip c, float v)
        {
            return Play(c, v, 1, AudioLayer.UISfx);
        }

        /// <summary>
        ///     Plays the Audio in the UISFX Layer
        /// </summary>
        /// <param name="c">clip</param>
        /// <param name="v">volume</param>
        /// <param name="p">pitch</param>
        /// <returns>A playing audio source</returns>
        public static AudioSource PlayUI(AudioClip c, float v, float p)
        {
            return Play(c, v, p, AudioLayer.UISfx);
        }


        /// <summary>
        ///     Plays the Audio
        /// </summary>
        /// <param name="c">clip</param>
        /// <param name="v">volume</param>
        /// <param name="p">pitch</param>
        /// <param name="l">Audio ayer</param>
        /// <returns>A playing audio source</returns>
        public static AudioSource Play(AudioClip c, float v, float p, AudioLayer l)
        {
            var s = GetNext();

            // return null
            if (s == null)
                return null;

            s.clip = c;
            s.pitch = p;
            s.loop = false;
            s.volume = v*AudioManager.GetVolume(l);
            s.Play();
            return s;
        }

        #endregion
    }
}