using System;
using UnityEngine;

namespace Notify
{
    public class Move : AnimNotify
    {
        [SerializeField] Vector3 moveVec;
        [SerializeField] float time;


        public override void Notify(GameObject obj, AnimationClip clip)
        {
            var rigidbody = obj.GetComponent<Rigidbody>();
            Vector3 moveDir = obj.transform.rotation * moveVec;

            if (rigidbody)
                rigidbody.AddForce(moveDir, ForceMode.Impulse);
        }
    }
}
