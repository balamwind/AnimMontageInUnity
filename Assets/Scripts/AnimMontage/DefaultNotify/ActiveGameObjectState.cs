using System;
using UnityEngine;

namespace Notify
{
    //This class inherits the AnimNotifyState, 
    //so EndNotify runs unconditionally for whatever reason it is terminated
    public class ActiveGameObjectState : AnimNotifyState
    {
        [SerializeField] GameObject gameObject;

        
        public override void BeginNotify(GameObject obj, AnimationClip clip)
        {
            gameObject.SetActive(true);
        }

        public override void EndNotify(GameObject obj, AnimationClip clip)
        {
            gameObject.SetActive(false);
        }

        public override void Notify(GameObject obj, AnimationClip clip)
        {
        }
    }
}
