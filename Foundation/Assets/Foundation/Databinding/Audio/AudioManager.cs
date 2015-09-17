// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using System.Collections.Generic;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Audio layers used by the Audio Helper
    /// </summary>
    public enum AudioLayer
    {
        /// <summary>
        ///     Game Sound
        /// </summary>
        Sfx,

        /// <summary>
        ///     Game Music
        /// </summary>
        Music,

        /// <summary>
        ///     UI Sounds
        /// </summary>
        UISfx
    }


    /// <summary>
    ///     AudioManager is a service extending game audio with control layers.
    /// </summary>
    public class AudioManager
    {
        /// <summary>
        ///     Cache
        /// </summary>
        protected static Dictionary<AudioLayer, float> Volumes = new Dictionary<AudioLayer, float>();

        /// <summary>
        ///     Event raised when audio layer changes
        /// </summary>
        public static event Action<AudioLayer, float> OnVolumeChanged;

        /// <summary>
        ///     Returns the volume for the audio layer
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static float GetVolume(AudioLayer layer)
        {
            if (!Volumes.ContainsKey(layer))
                return 1;
            return Volumes[layer];
        }

        /// <summary>
        ///     Sets the volume for the audio layer
        /// </summary>
        /// <param name="layer">layer in question</param>
        /// <param name="volume">volume</param>
        /// <returns></returns>
        public static void SetVolume(AudioLayer layer, float volume)
        {
            if (!Volumes.ContainsKey(layer))
                Volumes.Add(layer, volume);
            else
                Volumes[layer] = volume;

            if (OnVolumeChanged != null)
                OnVolumeChanged(layer, volume);
        }
    }
}