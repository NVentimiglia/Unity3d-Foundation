// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using System.Collections;
using UnityEngine;

namespace Foundation.Databinding
{
    /// <summary>
    ///     An audio source for playing UI sounds. Depends on the Audio2D Listener.
    /// </summary>
    [Serializable]
    [AddComponentMenu("Foundation/Audio/Audio2DSource")]
    public class Audio2DSource : MonoBehaviour
    {
        // settings
        public AudioClip Clip;
        public int Delay = 0;
        protected bool FirstTime = true;
        public AudioLayer Layer = AudioLayer.UISfx;
        public bool Loop = false;
        public float Pitch = 1;
        // play on enabled
        public bool PlayOnEnable = true;
        public float Volume = 1;
        // set internally
        public bool IsPlaying { get; set; }
        // temp
        public AudioSource Source { get; set; }

        protected void OnEnable()
        {
            if (PlayOnEnable)
                Play();
        }

        protected void OnDisable()
        {
            Stop();
        }

        /// <summary>
        ///     Stops playing the audio
        /// </summary>
        public void Stop()
        {
            if (Source)
            {
                Source.Stop();
            }
            Source = null;
            IsPlaying = false;

            StopAllCoroutines();
        }

        /// <summary>
        ///     play audio with delay
        /// </summary>
        public void Play()
        {
            StartCoroutine(PlayAsync(Delay));
        }

        /// <summary>
        ///     play audio
        /// </summary>
        /// <param name="delay"></param>
        public void Play(float delay)
        {
            StartCoroutine(PlayAsync(delay));
        }

        private IEnumerator PlayAsync(float delay)
        {
            if (Source)
            {
                Source.Stop();
                Source = null;
                IsPlaying = false;
            }

            IsPlaying = true;

            if (delay > 0)
                yield return new WaitForSeconds(delay);

            Source = Audio2DListener.GetNext();
            Source.loop = Loop;
            Source.pitch = Pitch;
            Source.volume = Volume*AudioManager.GetVolume(Layer);
            Source.clip = Clip;

            while (Source != null && Source.isPlaying)
            {
                yield return 1;
            }

            Source = null;
            IsPlaying = false;
        }
    }
}