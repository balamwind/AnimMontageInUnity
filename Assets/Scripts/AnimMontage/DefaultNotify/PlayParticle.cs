using System;
using UnityEngine;

namespace Notify
{
    public class PlayParticle : AnimNotify
    {
        [SerializeField] ParticleSystem particle;


        public override void Notify(GameObject obj, AnimationClip clip)
        {
            particle.Play(true);
        }
    }
}
