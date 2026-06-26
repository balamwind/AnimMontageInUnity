using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public abstract class AnimNotifyState : AnimNotify
{
    public int length => endFrame - startFrame;

    public int endFrame;
    //Progress so far. value is 0 ~ 1.
    [HideInInspector] public float normalizedTime;  


    public AnimNotifyState()
    {
        isAnimNotify = false;
    }

    public abstract void BeginNotify(GameObject obj, AnimationClip clip);
    public abstract void EndNotify(GameObject obj, AnimationClip clip);
}