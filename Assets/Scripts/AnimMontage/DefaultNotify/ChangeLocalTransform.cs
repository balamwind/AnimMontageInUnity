using System;
using UnityEngine;

namespace Notify
{
    public class ChangeLocalTransform : AnimNotifyState
    {
        [SerializeField] Transform _target;
        [SerializeField] Vector3 _localPos;
        [SerializeField] Vector3 _localEuler;
        [SerializeField] Vector3 _localScale;
        [Tooltip("If true, values are not substituted but added.")]
        [SerializeField] bool _plus = true;
        [Tooltip("If true, values return smoothly to original values.")]
        [SerializeField] bool _smooth;

        Vector3 _prePos;
        Vector3 _preEuler;
        Vector3 _preScale;


        public override void BeginNotify(GameObject obj, AnimationClip clip)
        {
            _prePos = _target.localPosition;

            Vector3 euler = _target.localEulerAngles;
            _preEuler.x = euler.x > 180f ? euler.x - 360f: euler.x;
            _preEuler.y = euler.y > 180f ? euler.y - 360f : euler.y;
            _preEuler.z = euler.z > 180f ? euler.z - 360f : euler.z;

            _preScale = _target.localScale;

            _target.localPosition = _plus ? _prePos + _localPos : _localPos;
            _target.localEulerAngles = _plus ? _preEuler + _localEuler : _localEuler;
            _target.localScale = _plus ? _preScale + _localScale : _localScale;
        }

        public override void EndNotify(GameObject obj, AnimationClip clip)
        {
            _target.localPosition = _prePos;
            _target.localEulerAngles = _preEuler;
            _target.localScale = _preScale;
        }

        public override void Notify(GameObject obj, AnimationClip clip)
        {
            if (_smooth)
            {
                Vector3 startLocalPos = _plus ? _prePos + _localPos : _localPos;
                Vector3 startLocalEuler = _plus ? _preEuler + _localEuler : _localEuler;
                Vector3 startLocalScale = _plus ? _preScale + _localScale : _localScale;

                _target.localPosition = Vector3.Lerp(startLocalPos, _prePos, normalizedTime);
                _target.localEulerAngles = Vector3.Lerp(startLocalEuler, _preEuler, normalizedTime);
                _target.localScale = Vector3.Lerp(startLocalScale, _preScale, normalizedTime);
            }

        }
    }
}
