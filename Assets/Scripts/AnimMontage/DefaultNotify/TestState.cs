using System;
using UnityEngine;

namespace Notify
{
    public class TestState : AnimNotifyState
    {
        [SerializeField] string str;

        public override void BeginNotify(GameObject obj, AnimationClip clip)
        {
            Debug.Log("begin");
        }

        public override void EndNotify(GameObject obj, AnimationClip clip)
        {
            Debug.Log("end");
        }

        public override void Notify(GameObject obj, AnimationClip clip)
        {
            Debug.Log(str);
        }
    }
}
