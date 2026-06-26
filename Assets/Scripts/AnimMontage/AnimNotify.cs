//※ Inherited child must be in Notify namespace
//ex) Notify.Print

//※※ If you change an inherited class to the extent that it is deleted or similar to deleted, 
//     Unity throw error. and all Notify in RegisterAnims will be broken, 
//     So if you want to change the class, you must modify it without that Notify

using System;
using UnityEngine;

[Serializable]
public abstract class AnimNotify
{
    //I wanted to get some performance
    [field: SerializeField, Disable] public bool isAnimNotify { get; protected set; } = true;

    public int startFrame;


    public abstract void Notify(GameObject obj, AnimationClip clip);
}
