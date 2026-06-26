using System;
using UnityEngine;

namespace Notify
{
    public class Trail : AnimNotifyState
    {
        [SerializeField] TrailRenderer renderer;

        
        public override void BeginNotify(GameObject obj, AnimationClip clip)
        {
            renderer.emitting = true;
        }

        public override void EndNotify(GameObject obj, AnimationClip clip)
        {
            renderer.emitting = false;
        }

        public override void Notify(GameObject obj, AnimationClip clip)
        {
        }
    }
}
