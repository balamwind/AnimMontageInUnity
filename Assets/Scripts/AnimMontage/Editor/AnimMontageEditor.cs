using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(AnimMontage))]
[InitializeOnLoad]
public class AnimMontageEditor : Editor
{
    AnimMontage _montage => (AnimMontage)target;


    static AnimMontageEditor()
    {
        EditorSceneManager.sceneSaved += scene =>
        {
            AnimMontage[] animMontages = FindObjectsOfType<AnimMontage>();
            foreach (var montage in animMontages)
                montage.Set_Editor(anim =>
                {
                    AnimatorController controller = GetController(montage);
                    int layerNumber = Array.FindIndex(controller.layers, inLayer => inLayer.name == "AnimMontage");

                    LinkAnimMontages(anim, layerNumber, controller);
                    SetConnectStateDic(anim, layerNumber, controller);
                });
        };
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Register AnimMontage"))
            RegisterAnimMontage();
    }


    static void LinkAnimMontages(AnimMontage.RegisterAnim anim, int layerNumber, AnimatorController controller)
    {
        AnimatorControllerLayer layer = controller.layers[layerNumber];
        foreach (var childState in layer.stateMachine.states)
            if (anim.stateName == childState.state.name ||
                string.IsNullOrEmpty(anim.stateName) && anim.clip == childState.state.motion)
            {
                anim.stateNameHash = childState.state.nameHash;
                return;
            }

        //Check stateNameHash setting
        anim.stateNameHash = 0;
        Debug.LogWarning(string.IsNullOrEmpty(anim.stateName) ? anim.clip.name : anim.stateName + " is not set");
    }

    static void SetConnectStateDic(AnimMontage.RegisterAnim anim, int layerNumber, AnimatorController controller)
    {
        if (anim.stateNameHash == 0)
            return;

        anim.defaultTrigger = null;
        anim.connectStateList.Clear();

        foreach (var inState in controller.layers[layerNumber].stateMachine.states)
            if (inState.state.nameHash == anim.stateNameHash)
            {
                //Set defaultTrigger
                //defaultTrigger is defaultState to otherState
                foreach (var trans in controller.layers[layerNumber].stateMachine.defaultState.transitions)
                    if (trans.destinationState.nameHash == anim.stateNameHash)
                    {
                        foreach (var condition in trans.conditions)
                            if (condition.mode == AnimatorConditionMode.If &&
                               //Set if parameter is trigger
                               Array.Find(controller.parameters, param => param.name == condition.parameter).type
                               == AnimatorControllerParameterType.Trigger)
                                anim.defaultTrigger = condition.parameter;

                        break;
                    }

                //Add ConnectState with transitions from this state to another state
                foreach (var trans in inState.state.transitions)
                    //Add if hasExit is true
                    if (trans.hasExitTime && trans.conditions.Length == 0)
                    {
                        anim.connectStateList.Add(new AnimMontage.RegisterAnim.ConnectState
                        {
                            nameHash = trans.destinationState.nameHash,
                            trigger = AnimMontage.RegisterAnim.exitTransition
                        });
                    }
                    else
                    {
                        foreach (var condition in trans.conditions)
                            if (condition.mode == AnimatorConditionMode.If &&
                               //Add if parameter is trigger
                               Array.Find(controller.parameters, param => param.name == condition.parameter).type
                               == AnimatorControllerParameterType.Trigger)
                            {
                                anim.connectStateList.Add(new AnimMontage.RegisterAnim.ConnectState
                                {
                                    nameHash = trans.destinationState.nameHash,
                                    trigger = condition.parameter
                                });
                            }
                    }

                break;
            }
    }

    static AnimatorController GetController(AnimMontage montage)
    {
        return AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(montage.GetComponent<Animator>().runtimeAnimatorController));
    }

    //Set Montages
    void RegisterAnimMontage()
    {
        if (_montage.GetComponent<Animator>().runtimeAnimatorController == null)
        {
            Debug.LogError("Animator's runtimeAnimatorController is null");
            return;
        }

        AnimatorController controller = GetController(_montage);

        //'AnimMontage' 레이어를 가져옴
        AnimatorControllerLayer layer = null;
        AnimatorState empty = null;
        foreach (var inLayer in controller.layers)
            if (inLayer.name == "AnimMontage")
            {
                layer = inLayer;
                empty = layer.stateMachine.defaultState;
            }

        //Create layer if layer is null
        if (layer == null)
        {
            Debug.LogWarning("No layers were found with the name 'AnimMontage'. Create a new layer");

            layer = new AnimatorControllerLayer();
            layer.name = "AnimMontage";
            layer.defaultWeight = 1;
            layer.stateMachine = new AnimatorStateMachine();
            layer.stateMachine.name = layer.name;
            layer.stateMachine.hideFlags = HideFlags.HideInHierarchy;

            //Save
            if (AssetDatabase.GetAssetPath(controller) != "")
                AssetDatabase.AddObjectToAsset(layer.stateMachine, AssetDatabase.GetAssetPath(controller));

            controller.AddLayer(layer);

            empty = layer.stateMachine.AddState("Empty");
            layer.stateMachine.defaultState = empty;

            EditorUtility.SetDirty(empty);
        }

        _montage.defaultStateHash = empty.nameHash;

        var registerAnims = serializedObject.FindProperty("_registerAnims");
        int size = registerAnims.arraySize;

        for (int j = 0; j < size; j++)
        {
            AnimatorStateMachine sm = layer.stateMachine;
            AnimatorState state = null;

            var animProperty = registerAnims.GetArrayElementAtIndex(j);
            string stateName = animProperty.FindPropertyRelative("stateName").stringValue;
            AnimationClip clip = (AnimationClip)animProperty.FindPropertyRelative("clip").objectReferenceValue;

            for (int i = 0; i < sm.states.Length; i++)
                if (stateName == sm.states[i].state.name ||
                    sm.states[i].state.motion == clip && string.IsNullOrEmpty(stateName))
                {
                    state = sm.states[i].state;
                    //change only clip
                    if (state.motion != clip)
                        state.motion = clip;

                    break;
                }

            if (state) continue;

            //State 생성
            state = sm.AddState(string.IsNullOrEmpty(stateName) ? clip.name : stateName);
            state.motion = clip;
            state.AddTransition(empty, true);

            EditorUtility.SetDirty(state);
        }

        EditorUtility.SetDirty(layer.stateMachine);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        _montage.Set_Editor(anim =>
        {
            int layerNumber = Array.FindIndex(controller.layers, inLayer => inLayer.name == "AnimMontage");

            LinkAnimMontages(anim, layerNumber, controller);
            SetConnectStateDic(anim, layerNumber, controller);
        });
    }
}
