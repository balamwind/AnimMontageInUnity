using System;
using System.Collections.Generic;
using UnityEngine;


//This component is animator utility component
//Use this, you can easily add events to your animations

//※ This component runs before other normal components
//※ Sub-StateMachine is not supported now.
//※ BlendTree is not supported. because transitions between animations are more likely to produce unwanted results.
//※ Use only one layer for user simplicity
[RequireComponent(typeof(Animator))]
public class AnimMontage : MonoBehaviour
{
    [Serializable]
    public struct AnimEvent
    {
        public TextAsset notifyScript;
        [SerializeReference] public AnimNotify notify;
        [HideInInspector] public bool run;  //use Notify, NotifyState
        [HideInInspector] public bool end;  //use NotifyState
    }

    [Serializable]
    public class RegisterAnim
    {
        [Serializable]
        public struct ConnectState
        {
            public int nameHash;
            public string trigger;
        }


        public const string exitTransition = "RegisterAnim_Exit";

        public Action<RegisterAnimInfo> onChange;
        public Action<RegisterAnimInfo> onComplete;


        [Tooltip("After inspection by this name, animation inspection is performed")]
        public string stateName;
        public AnimationClip clip;
        public AnimEvent[] animEvents;

        public int stateNameHash;
        public int runCount;       //Animation repeat count

        //When transitioning from this state to another state, if there is a trigger, registered.
        public List<ConnectState> connectStateList = new List<ConnectState>();
        //trigger name, when transition this state from default state
        public string defaultTrigger;
    }



#pragma warning disable CS0660, CS0661
    //Used to disclose only little information of RegisterAnimation
    public struct RegisterAnimInfo : IEquatable<RegisterAnimInfo>
    {
        public AnimationClip clip;
        public int animationNumber;


        public bool Equals(RegisterAnimInfo other) => this == other;
        public static bool operator !=(RegisterAnimInfo me, RegisterAnimInfo other) =>
            !(me == other);
        public static bool operator ==(RegisterAnimInfo me, RegisterAnimInfo other) =>
            me.clip == other.clip && me.animationNumber == other.animationNumber;
    }
#pragma warning restore CS0660, CS0661


    [SerializeField] RegisterAnim[] _registerAnims;
    //이렇게 해야 DefaultStateHash가 데이터로써 저장이 가능하기 때문
    [HideInInspector] public int defaultStateHash;

    public event Action<RegisterAnimInfo> onStart;
    public event Action<RegisterAnimInfo> onChange;
    public event Action<RegisterAnimInfo> onComplete;

    public int layerNumber { get; private set; } = -1;


    Animator _animator;
    bool _changeEnd;
    RegisterAnim _nowAnim;
    RegisterAnim _preAnim;


    void Awake()
    {
        _animator = GetComponent<Animator>();

        if (_registerAnims.Length != 0)
        {
            layerNumber = _animator.GetLayerIndex("AnimMontage");
            if (layerNumber < 0)
                Debug.LogError("'AnimMontage' named layer not found");
        }
    }

    void Start()
    {
        foreach (var anim in _registerAnims)
            if (anim.stateNameHash == 0)
                Debug.LogError(string.IsNullOrEmpty(anim.stateName) ? anim.clip.name : anim.stateName +
                    " is not linked. Please press the Register AnimMontage button.");

    }

    void LateUpdate()
    {
        var stateInfo = _animator.GetCurrentAnimatorStateInfo(layerNumber);

        //_preAnim이 존재한다면 defaultState는 Current나 Next에 존재할 수 없음
        //_preAnim은 반드시 Play로 기존 실행시키던 애님을 밀어내야 존재하기 때문
        if (_preAnim != null)
        {
            //_preAnim -> _nowAnim
            if (_preAnim.stateNameHash == stateInfo.shortNameHash)
            {
                RunNotify(_preAnim, stateInfo);
                stateInfo = _animator.GetNextAnimatorStateInfo(layerNumber);
            }
            else
                //Current가 부합하지 않을경우 = 끝났다는 뜻
                _preAnim = null;
        }

        //_nowAnim이 Next를 검사하는 타이밍
        //1. DefaultState -> _nowAnim으로 Transition 중일 때
        //2. _preAnim이 존재하고 _preAnim -> _nowAnim으로 Transition 중일 때
        //나머지는 Current를 검사함
        if (_nowAnim != null)
        {
            //DefaultState -> _nowAnim 이라면 Next를 검사하게
            //_nowAnim -> DefaultState 면 Current를 검사해야하므로 딱히 추가하지 않음
            //IsInTransition()이 한프레임 후에 true가 되서 여기에 추가로 결정 (사실 실행 순서를 바꾸면 될거같긴 해)
            if (defaultStateHash == stateInfo.shortNameHash)
                stateInfo = _animator.GetNextAnimatorStateInfo(layerNumber);

            if (_nowAnim.stateNameHash == stateInfo.shortNameHash)
                RunNotify(_nowAnim, stateInfo);
            else
            {
                //_nowAnim은 있는데, StateInfo가 달라진거면 Complete된거임
                CallChange(_nowAnim);
                CallComplete(_nowAnim);
                _nowAnim = null;
            }
        }
    }


    #region public
    //Run with animation clip
    public void Play(AnimationClip clip, bool force = false, Action<RegisterAnimInfo> changeFunc = null, Action<RegisterAnimInfo> completeFunc = null)
    {
        Play(Array.FindIndex(_registerAnims, inAnim => inAnim.clip == clip), force, changeFunc, completeFunc);
    }

    //Run with state name
    public void Play(string name, bool force = false, Action<RegisterAnimInfo> changeFunc = null, Action<RegisterAnimInfo> completeFunc = null)
    {
        Play(Array.FindIndex(_registerAnims, inAnim => inAnim.stateName == name), force, changeFunc, completeFunc);
    }

    /// <summary>
    /// play registered animation
    /// 자동으로 CmdPlayAnimMontage를 호출함.
    /// endFunc와 completeFunc는 오직 본인 클라에서만 실행 됨
    /// </summary>
    /// <param name="number">array number that want to play register animation</param>
    /// <param name="force">Whether to ignore transition and force it to run</param>
    /// <param name="changeFunc">call when animation is end</param>
    /// <param name="completeFunc">call when animation is completely end</param>
    public void Play(int number, bool force = false, Action<RegisterAnimInfo> changeFunc = null, Action<RegisterAnimInfo> completeFunc = null)
    {
        if (number < 0)
        {
            Debug.LogError("Animation is not registered");
            return;
        }

        if (number >= _registerAnims.Length)
        {
            Debug.LogError("Number greater than size of register animation arrays");
            return;
        }

        RegisterAnim anim = _registerAnims[number];
        string triggerName = GetTriggerName(anim);

        //블렌드 도중에 Trigger는 정상 실행하지 않음
        //고로 블렌드 도중에 애니메이션이 들어오면 강제 실행으로 변경
        if (_animator.IsInTransition(layerNumber) && triggerName != null)
            force = true;

        ChangeNowAnim(anim);

        //본인 클라에서 Play한건 강제성이 없다면 트리거로
        //남의 클라에서 실행한건 트리거로 플레이 했다는게 들어왔다면
        //트리거를 뽑아와보고 실패하면 강제플레이
        if (force || string.IsNullOrEmpty(triggerName))
            _animator.Play(anim.stateNameHash, layerNumber, 0f);
        else
            _animator.SetTrigger(triggerName);

        anim.onChange = changeFunc;
        anim.onComplete = completeFunc;
    }

    public bool IsPlay()
    {
        return _preAnim != null || _nowAnim != null;
    }

    public bool IsInTransition()
    {
        return _animator.IsInTransition(layerNumber);
    }

    /// <summary>
    /// Get information of the currently running montage. 
    /// Returns the default value if it is null
    /// </summary>
    public RegisterAnimInfo GetNowMontageInfo()
    {
        if (_nowAnim == null)
            return default;

        RegisterAnimInfo info = new RegisterAnimInfo()
        {
            clip = _nowAnim.clip,
            animationNumber = Array.FindIndex(_registerAnims, inAnim => inAnim == _nowAnim)
        };

        return info;
    }

    /// <summary>
    /// ※ This function must be call when transition
    /// Get information of the previously running montage.
    /// Returns the default value if it is null
    /// </summary>
    public RegisterAnimInfo GetPreMontageInfo()
    {
        if (_preAnim == null)
            return default;

        RegisterAnimInfo info = new RegisterAnimInfo()
        {
            clip = _preAnim.clip,
            animationNumber = Array.FindIndex(_registerAnims, inAnim => inAnim == _preAnim)
        };

        return info;
    }

    //this function is run in editor
    public void Set_Editor(Action<RegisterAnim> setEditorFunc)
    {
        foreach (RegisterAnim anim in _registerAnims)
            setEditorFunc(anim);
    }
    #endregion


    #region private
    void ChangeNowAnim(RegisterAnim anim)
    {
        if (_nowAnim == null)
            _nowAnim = anim;
        else
        {
            //변경 이벤트 호출
            CallChange(_nowAnim);

            //기존 _nowAnim에서 anim으로 연결된 블렌딩이 없을 시, 바로 _nowAnim 변경
            var conn = _nowAnim.connectStateList.Find(connect => connect.nameHash == anim.stateNameHash);
            if (conn.nameHash == default && conn.trigger == default)
                _nowAnim = anim;
            else
            {
                _preAnim = _nowAnim;
                _nowAnim = anim;
            }
        }

        _nowAnim.runCount = 0;
        for (int j = 0; j < _nowAnim.animEvents.Length; j++)
        {
            _nowAnim.animEvents[j].run = false;
            _nowAnim.animEvents[j].end = false;
        }

        RegisterAnimInfo regiInfo = new RegisterAnimInfo
        {
            clip = _nowAnim.clip,
            animationNumber = Array.FindIndex(_registerAnims, inAnim => inAnim == _nowAnim)
        };

        onStart?.Invoke(regiInfo);
    }

    void CallChange(RegisterAnim anim)
    {
        //If _nowAnim has an running AnimNotify, force EndNotify to be called
        foreach (var animEvent in anim.animEvents)
            if (animEvent.notify is AnimNotifyState)
            {
                AnimNotifyState nState = (AnimNotifyState) animEvent.notify;
                if (nState.normalizedTime > 0 && nState.normalizedTime < 1)
                {
                    nState.normalizedTime = 1;
                    nState.EndNotify(gameObject, anim.clip);
                }
            }

        RegisterAnimInfo regiAnimInfo = new RegisterAnimInfo()
        {
            clip = anim.clip,
            animationNumber = Array.FindIndex(_registerAnims, inAnim => inAnim == anim),
        };

        //call onEnd
        onChange?.Invoke(regiAnimInfo);
        anim.onChange?.Invoke(regiAnimInfo);
    }

    void CallComplete(RegisterAnim anim)
    {
        RegisterAnimInfo regiAnimInfo = new RegisterAnimInfo()
        {
            clip = anim.clip,
            animationNumber = Array.FindIndex(_registerAnims, inAnim => inAnim == anim),
        };

        //call onComplete
        onComplete?.Invoke(regiAnimInfo);
        anim.onComplete?.Invoke(regiAnimInfo);
    }

    //※ The normalized time of Unity's AnimatorStateInfo depends on the setting of 
    //   Transition rather than 1 at the end of the animation.
    void RunNotify(RegisterAnim anim, AnimatorStateInfo compareStateInfo)
    {
        if (anim.animEvents != null)
        {
            //Initialize when animation repeats
            bool changeRunCount = false;
            if (anim.clip.isLooping && anim.runCount != (int) compareStateInfo.normalizedTime)
            {
                changeRunCount = true;
                anim.runCount = (int) compareStateInfo.normalizedTime;
            }

            for (int i = 0; i < anim.animEvents.Length; i++)
            {
                AnimEvent animEvent = anim.animEvents[i];
                AnimNotify notify = animEvent.notify;

                if (notify == null)
                    continue;

                if (changeRunCount)
                {
                    animEvent.run = false;
                    animEvent.end = false;
                }

                //Run Notify
                float normalizedTime = compareStateInfo.normalizedTime - anim.runCount;
                float startNormalTime = notify.startFrame / (anim.clip.frameRate * anim.clip.length);

                if (notify is AnimNotifyState)
                {
                    AnimNotifyState nState = (AnimNotifyState) notify;
                    float endNormalTime = nState.endFrame / (anim.clip.frameRate * anim.clip.length);

                    if (animEvent.end == false && animEvent.run == false && normalizedTime >= startNormalTime)
                    {
                        animEvent.run = true;

                        nState.normalizedTime = 0;
                        nState.BeginNotify(gameObject, anim.clip);
                    }
                    else if (animEvent.end == false && animEvent.run && normalizedTime >= endNormalTime)
                    {
                        animEvent.run = false;
                        animEvent.end = true;

                        nState.normalizedTime = 1;
                        nState.EndNotify(gameObject, anim.clip);
                    }
                    else if (animEvent.end == false && normalizedTime >= startNormalTime && normalizedTime <= endNormalTime)
                    {
                        nState.normalizedTime = Mathf.InverseLerp(startNormalTime, endNormalTime, normalizedTime);
                        nState.Notify(gameObject, anim.clip);
                    }
                }
                else
                {
                    if (animEvent.run == false && normalizedTime >= startNormalTime)
                    {
                        animEvent.run = true;
                        notify.Notify(gameObject, anim.clip);
                    }
                }

                anim.animEvents[i] = animEvent;

            }
        }
    }

    //Check if _nowAnim and animare connected
    //Check the transition condition for a trigger, and return the trigger name if any
    //If false, return null
    string GetTriggerName(RegisterAnim anim)
    {
        string result = null;
        //transition from default state
        if (_nowAnim == null)
            result = string.IsNullOrEmpty(anim.defaultTrigger) ? null : anim.defaultTrigger;
        else
            foreach (var connectState in _nowAnim.connectStateList)
                if (connectState.nameHash == anim.stateNameHash)
                {
                    result = connectState.trigger;
                    break;
                }

        return result;
    }
    #endregion
}

//use [Disable]
//ex) [Disable] float a;
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class DisableAttribute : PropertyAttribute
{
}
