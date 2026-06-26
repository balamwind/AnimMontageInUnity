# AnimMontage in Unity

버전 : 2020.3.42f1

Unreal Engine의 AnimMontage / AnimNotify 시스템을 Unity에서 구현한 확장 라이브러리입니다.

Unity 기본 AnimationEvent는 AnimationClip에 직접 이벤트를 박아야 하고, 인스펙터에서 타입을 선택하거나 구조적으로 관리하기 어렵습니다.  
이 라이브러리는 `[SerializeReference]`를 활용한 다형성 직렬화로 **인스펙터에서 Notify 타입을 선택·추가**할 수 있고, 애니메이션 재생 제어 API와 콜백 시스템까지 함께 제공합니다.

중요) 테스트용으로 제작되었습니다

---

## 구조

```
AnimMontage (MonoBehaviour)
├── RegisterAnim[]          // 등록된 애니메이션 목록
│   ├── AnimationClip
│   ├── AnimEvent[]         // 이 애니메이션에 등록된 Notify 목록
│   │   └── [SerializeReference] AnimNotify
│   └── ConnectState[]      // 상태 간 트리거 연결 정보
│
├── AnimNotify (abstract)           // 특정 프레임에 1회 실행
│   └── startFrame
│   └── Notify(GameObject, AnimationClip)
│
└── AnimNotifyState (abstract)      // startFrame ~ endFrame 구간 동안 실행
    ├── endFrame
    ├── normalizedTime (0~1)
    ├── BeginNotify(...)
    ├── Notify(...)           // 구간 중 매 프레임
    └── EndNotify(...)
```

---

## 핵심 설계

### `[SerializeReference]` 기반 다형성 직렬화

`AnimNotify`를 상속한 클래스를 만들고 `Notify` 네임스페이스에 넣으면, 인스펙터에서 드롭다운으로 선택할 수 있습니다.

```csharp
// 예시: 특정 프레임에 사운드를 재생하는 Notify
namespace Notify
{
    [Serializable]
    public class PlaySound : AnimNotify
    {
        public AudioClip clip;

        public override void Notify(GameObject obj, AnimationClip animClip)
        {
            obj.GetComponent<AudioSource>()?.PlayOneShot(clip);
        }
    }
}
```

```csharp
// 예시: 구간 동안 히트 판정을 활성화하는 NotifyState
namespace Notify
{
    [Serializable]
    public class EnableHitBox : AnimNotifyState
    {
        public override void BeginNotify(GameObject obj, AnimationClip clip)
            => obj.GetComponentInChildren<Collider>().enabled = true;

        public override void Notify(GameObject obj, AnimationClip clip) { }

        public override void EndNotify(GameObject obj, AnimationClip clip)
            => obj.GetComponentInChildren<Collider>().enabled = false;
    }
}
```

### `_preAnim` / `_nowAnim` 전환 처리

애니메이션 전환(Transition) 중에도 이전 애니메이션(`_preAnim`)과 현재 애니메이션(`_nowAnim`) 각각의 Notify를 독립적으로 처리합니다. 블렌딩 도중 강제 실행(`force = true`)도 지원합니다.

### `LateUpdate` 기반 프레임 체크

`Animator.GetCurrentAnimatorStateInfo()`의 `normalizedTime`을 `LateUpdate`에서 폴링하여 프레임 타이밍을 계산합니다. 루프 애니메이션은 `runCount`로 반복 횟수를 추적해 매 루프마다 Notify가 재실행됩니다.

---

## 재생 API

```csharp
// AnimationClip으로 재생
animMontage.Play(clip);

// 등록된 이름으로 재생
animMontage.Play("Attack");

// 인덱스로 재생 + 콜백
animMontage.Play(0,
    changeFunc:   info => Debug.Log("애니메이션 전환됨"),
    completeFunc: info => Debug.Log("애니메이션 완료됨")
);

// 강제 재생 (트랜지션 무시)
animMontage.Play("Attack", force: true);

// 이벤트 구독
animMontage.onStart    += info => { };
animMontage.onChange   += info => { };
animMontage.onComplete += info => { };

// 상태 확인
animMontage.IsPlay();
animMontage.IsInTransition();
animMontage.GetNowMontageInfo();
```

---

## 적용 사례

Unity Mirror 기반 멀티플레이 배틀로얄 프로젝트 [Project_FC](https://gitlab.com/b_mh/project_fc)에서 전투 히트 판정 타이밍 제어에 실제 적용했습니다.

---

## 제한사항

- Sub-StateMachine 미지원
- BlendTree 미지원
- 단일 레이어(`AnimMontage` 이름의 레이어)만 사용

---

## 알려진 문제점

- AnimMontage 스크립트에 추가된 Animation이나 Notify 등이 많아 질 경우 에디터에서 프레임이 떨어지는 문제를 확인
- force와 관계 없이, 다른 Animation Layer를 사용하기 때문에 기본 Layer에서 Blend되어 자연스럽게 애니메이션이 변경되지 않는 것을 확인

---

## 배운 점

- Unreal의 AnimMontage 구조를 분석하고 Unity로 재구현하며 **두 엔진의 애니메이션 파이프라인 차이**를 깊이 이해
- `[SerializeReference]`를 통한 다형성 직렬화로 **에디터 친화적인 확장 구조** 설계 경험
- 실제 프로젝트 적용 후 Notify 수가 늘어날수록 LateUpdate 폴링 비용이 커지는 한계를 확인. 적용 범위를 용도에 따라 선별해야 한다는 것을 체감
