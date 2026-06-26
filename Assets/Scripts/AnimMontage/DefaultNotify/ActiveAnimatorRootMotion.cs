using System;
using UnityEngine;

namespace Notify
{
    public class ActiveAnimatorRootMotion : AnimNotifyState
    {
        public override void BeginNotify(GameObject obj, AnimationClip clip)
        {
            Animator animator = obj.GetComponent<Animator>();
            if (animator)
                animator.applyRootMotion = true;
        }

        public override void EndNotify(GameObject obj, AnimationClip clip)
        {
            Animator animator = obj.GetComponent<Animator>();
            if (animator)
                animator.applyRootMotion = false;
        }

        public override void Notify(GameObject obj, AnimationClip clip)
        {
        }
    }
}
