using System;
using UnityEngine;

namespace Notify
{
    public class Print : AnimNotify
    {
        [SerializeField] string str;

        public override void Notify(GameObject obj, AnimationClip clip)
        {
            Debug.Log(str);
        }
    }
}
