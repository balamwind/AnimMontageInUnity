using System;
using UnityEngine;

namespace Notify
{
    public class ActiveGameObject : AnimNotify
    {
        [SerializeField] GameObject gameObject;
        [SerializeField] bool value;


        public override void Notify(GameObject obj, AnimationClip clip)
        {
            gameObject.SetActive(value);
        }
    }
}
