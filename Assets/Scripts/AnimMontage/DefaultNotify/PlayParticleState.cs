using System;
using UnityEngine;

namespace Notify
{
    public class PlayParticleState : AnimNotifyState
    {
        [SerializeField] ParticleSystem particle;


        public override void BeginNotify(GameObject obj, AnimationClip clip)
        {
            particle.Play(true);
        }

        public override void EndNotify(GameObject obj, AnimationClip clip)
        {
            particle.Stop(true);
        }

        public override void Notify(GameObject obj, AnimationClip clip)
        {
        }
    }
}
