using System;
using UnityEngine;

namespace Notify
{
    public class CreateParticle : AnimNotify
    {
        [SerializeField] ParticleSystem particlePrefab;
        [SerializeField] Vector3 localPosition;
        [SerializeField] Vector3 localRotation;
        [SerializeField] Vector3 localScale;

        
        public override void Notify(GameObject obj, AnimationClip clip)
        {
            var particle = GameObject.Instantiate<ParticleSystem>(particlePrefab);
            particle.transform.localPosition = localPosition;
            particle.transform.localEulerAngles = localRotation;
            particle.transform.localScale = localScale;

            particle.Play(true);
        }
    }
}
