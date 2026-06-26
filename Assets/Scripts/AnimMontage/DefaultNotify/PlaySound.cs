using System;
using UnityEngine;

namespace Notify
{
    public class PlaySound : AnimNotify
    {
        [Tooltip("If null, use AudioSource of this gameobject.")]
        [SerializeField] AudioSource source;
        [SerializeField] AudioClip audio;
        [SerializeField] float volume;


        public override void Notify(GameObject obj, AnimationClip clip)
        {
            if (source)
                source.PlayOneShot(audio, volume);
            else
                obj.GetComponent<AudioSource>().PlayOneShot(audio, volume);
        }
    }
}
