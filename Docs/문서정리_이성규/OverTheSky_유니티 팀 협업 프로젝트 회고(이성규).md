# OverTheSky_유니티 팀 협업 프로젝트 회고

## 프로젝트 개요

- **프로젝트명**: OverTheSky
- **장르**: 온리업 (Only Up) / 등산 게임
- **팀원 수**: 5인
- **개발 기간**: 7일
- **개발 환경**: Unity 2022.3.62f2 / C#
- **프로젝트 목표**
  - 온리 업 장르의 게임을 개발한다.
  - 프로토타이핑을 통해 최대한 빨리 개발하는 것을 목표로 함.
  - 진행 기간 전 프로젝트 팀장으로써 빠른 개발을 위한 프레임워크 개발을 진행 전주 주말에 진행.
  - 개별 작업 노트를 작성을 통해 추후 문서화 및 소통의 효율성을 높인다.
  - 에셋의 선정에 신경을 쓰기보단 구현을 최우선 사항으로 삼음.

## 구현한 게임 설명

- **게임 장르**: 3인칭 3D 플랫포머(온리업 (Only Up) / 등산 게임)
- **핵심 규칙**
  - WASD키로 이동, LeftShift로 달리기
  - Space로 점프, R키로 체크포인트 위치로 리셋
  - 별도의 게임 데이터 저장 기능 없이 체크포인트 시스템 만으로 정상까지 도달해야함.
- **플레이 흐름**
  1. 타이틀 씬: GameManager 초기화 및 게임 시작/종료 UI 제공.
  2. 게임 씬: 플레이어는 다양한 물리 기믹(움직이는 발판, 회전 장애물, 점프 패드 등)을 돌파하며 등반.
  3. 클리어: 정상의 마지막 체크포인트 도달 시 3초 후 타이틀 씬으로 복귀.

## 설계 구조

### 주요 클래스

| 분류 | 클래스명 | 역할 |
|------|----------|------|
| **Core Framework** | `Singleton<T>` | 제네릭 싱글톤 베이스. DontDestroyOnLoad, 중복 방지, 종료 시 재생성 방지 |
| | `GameManager` | 게임 생명주기 관리, 커서 제어, 하위 매니저 자동 생성 및 계층 구조화 |
| | `InputManager` | 입력 처리 및 버퍼링. Update-FixedUpdate 간 입력 씹힘 방지 |
| | `Logger` | 인게임 디버그 로그 출력. Queue 기반 선입선출, 색상별 로그 타입 구분 |
| | `Define` | 전역 상수(씬 이름, 태그, 레이어, 애니메이터 파라미터) 관리 |
| **Player System** | `PlayerBase` | 플레이어 상태 감지(바닥/천장/벽), SphereCast 기반 Ground Check, 코요테 타임 |
| | `PlayerController` | 이동/점프 로직, 카메라 기준 이동, 경사면 처리, 애니메이션 연동 |
| | `ForceReceiver` | 외부 충격(넉백, 바람 등) 처리. ForceMode별 계산, 시간 기반 감쇠 |
| **Camera** | `CameraController` | 3인칭 카메라. 타겟 추적, 마우스 회전, 줌, SphereCast 벽 충돌 처리 |
| **Gimmick Base** | `PlatformBase` | 이동 발판 추상 클래스. MovePosition 기반 물리 이동, 속도 자동 계산 |
| **Interface** | `IMovingPlatform` | 이동 발판 속도 제공 인터페이스. 플레이어-발판 간 미끄러짐 방지 |

### 클래스 간 관계
```cs
GameManager (Singleton)
 ├── InputManager (Singleton, 자식으로 등록)
 ├── SceneController (Singleton, 자식으로 등록)
 └── Logger (Singleton, 프리팹)

PlayerController : PlayerBase
 ├── Rigidbody (물리 제어)
 ├── CapsuleCollider (충돌)
 ├── Animator (애니메이션)
 └── ForceReceiver (외부 충격 처리)

CameraController
 └── Target: Player Transform

PlatformBase (Abstract) : IMovingPlatform
 ├── Rigidbody (Kinematic, MovePosition 기반)
 ├── CalculateNextPosition() (자식이 구현)
 ├── GetVelocityAtPoint() (플레이어가 호출)
 └── IMovingPlatform (발판 속도 연동)
```

**핵심 설계 의도**:
- `PlayerBase`와 `PlayerController` 분리: 상태 감지(Ground/Ceiling/Wall)와 이동 로직을 분리하여 단일 책임 원칙 준수
- `ForceReceiver` 컴포넌트화: 외부 충격 처리를 분리하여 기믹 개발자가 `AddImpact()`만 호출하면 되도록 단순화
- `PlatformBase` 추상화: 팀원은 `CalculateNextPosition()`만 구현하면 물리 이동/속도 계산은 부모가 처리
- `IMovingPlatform` 인터페이스: 팀원들의 다양한 발판 구현을 플레이어 물리 시스템과 호환시키기 위한 어댑터
  - 플레이어는 이 인터페이스를 통해 속도를 물어본다.
```cs
public interface IMovingPlatform
{
    Vector3 GetVelocityAtPoint(Vector3 worldPoint);
}

[RequireComponent(typeof(Rigidbody))]
public abstract class PlatformBase : MonoBehaviour, IMovingPlatform

// [인터페이스 구현] 플레이어가 발을 디딘 지점(worldPoint)의 정확한 속도를 반환
public Vector3 GetVelocityAtPoint(Vector3 worldPoint)
{
    // 리지드바디가 MovePosition/Rotation으로 이동하면,
    // 유니티가 내부적으로 선속도+각속도를 계산해서 GetPointVelocity로 준다.
    return rb.GetPointVelocity(worldPoint);
}

protected abstract Vector3 CalculateNextPosition();
protected virtual Quaternion CalculateNextRotation() => rb.rotation;
```

## 사용한 C# 개념

### 1. 제네릭 싱글톤 패턴 (Generic Singleton)
```cs
public class Singleton<T> : MonoBehaviour where T : Component
```
- **where T : Component** 제약 조건으로 MonoBehaviour 파생 타입만 허용
- 씬에 없으면 자동 생성, DontDestroyOnLoad로 씬 전환 시 유지
- `_isQuitting` 플래그로 종료 시 재생성 방지

### 2. 프로퍼티를 활용한 상태 변화 감지
```cs
private bool _isGrounded;
public bool IsGrounded 
{ 
    get => _isGrounded;
    private set
    {
        if (_isGrounded != value)
        {
            bool wasGrounded = _isGrounded;
            _isGrounded = value;
            if (!wasGrounded && IsGrounded) OnLand();
            if (wasGrounded && !IsGrounded) OnFall();
        }
    }
}
```

- setter에서 값 변화를 감지하여 이벤트성 메서드 호출
- 매 프레임 체크하는 방식 대비 불필요한 호출 최소화

- ### 3. 입력 버퍼링 (Input Buffering)
```cs
// InputManager
private bool _jumpBuffered = false;
public bool ConsumeJump()
{
    if (_jumpBuffered)
    {
        _jumpBuffered = false;
        return true;
    }
    return false;
}

// InputManager의 Update안에
// 점프: GetKeyDown이 true면 버퍼에 저장 (소비될 때까지 유지)
if (Input.GetKeyDown(KeyCode.Space))
{
    _jumpBuffered = true;
}
```
- Update(입력 감지)와 FixedUpdate(물리 처리) 간 타이밍 불일치 해결
- 점프 입력이 씹히는 현상 방지

## 잘했다고 생각하는 점

### 1. 사전 준비로 개발 속도 확보
프로젝트 시작 전 주말을 활용해 Core 프레임워크(Singleton, Logger, Define)와 PlayerBase를 미리 구현해둠. 덕분에 본 개발 기간에는 기능 구현에만 집중할 수 있었고, 팀원들도 매니저 클래스나 상수를 바로 사용할 수 있었음.

### 2. 문서 기반 소통
R&D 문서, 역할 분담 문서, 프로젝트 가이드 문서를 사전에 작성하여 팀원들에게 공유. "왜 Character Controller를 안 쓰는지", "네이밍 컨벤션은 어떻게 하는지" 등의 질문에 문서로 답변할 수 있어 소통 비용이 줄었음.

### 3. 컴포넌트 분리 설계
`ForceReceiver`를 별도 컴포넌트로 분리한 덕분에, 팀원들이 기믹에서 넉백을 구현할 때 `GetComponent<ForceReceiver>().AddImpact(force)`만 호출하면 됨. PlayerController 내부 구조를 몰라도 상호작용 가능.

### 4. 팀원 친화적 베이스 클래스 설계
`PlatformBase` 추상 클래스를 만들어 팀원들이 `CalculateNextPosition()`만 구현하면 되도록 설계. 물리 이동(MovePosition), 속도 계산, Rigidbody 세팅은 부모가 처리. 실제로 팀원이 이걸 안 쓰고 직접 구현했지만, 의도 자체는 좋았음.

### 5. 병합 시 팀원 코드 최소 수정
병합 작업이 동작하는 코드는 최대한 건드리지 않고 자신의 코드를 수정하고 동작하지 않아 수정이 필요시 최소한으로 팀원의 코드를 수정하는 방식을 선택.

## 아쉬웠던 점 / 어려웠던 점

### 1. Character Controller 없이 구현하는 난이도
- 학습 목적으로 Character Controller와 Physics Material 사용을 제외했는데, 예상보다 훨씬 많은 예외 처리가 필요했음.
- 경사면 처리, 벽 타기 방지, 점프 후 착지 안정화 등 R&D 문서에서 예상한 것 이상의 이슈가 발생.
- 초기엔 Drag만으로 마찰을 흉내 내려고 했으나, 경사면 정지나 벽 미끄러짐 구현에 한계를 느껴 런타임에 PhysicMaterial을 동적으로 생성 및 교체하는 방식으로 해결책을 선회함.

### 3. Git 충돌 관리
씬 파일 충돌이 자주 발생했고, 한 번은 팀원 작업물이 누락되어 과거 시점 프로젝트에서 패키지로 복구하는 작업이 필요했음. 머지시 팀원이 작업한 모든 파일을 세세히 확인해보지 않은 스스로의 부주의함도 있었다.

### 4. 완성도(폴리싱) 향상에 쓸 시간 부족
기능 구현에 시간을 많이 쓰다 보니 UI나 맵 전반의 완성도나 따로 에셋을 할애해 맵 비주얼을 개선할 시간이 부족했다.

## 다시 만든다면 개선하고 싶은 점

### 1. 이벤트 기반 아키텍처 도입
현재는 프로퍼티 setter에서 직접 메서드를 호출하는 방식인데, C# event나 UnityEvent를 활용한 옵저버 패턴으로 개선하면 확장성이 좋아질 것.
```csharp
public event Action OnGrounded;
public event Action OnAirborne;
```

### 2. 상태 머신(FSM) 적용
PlayerController의 상태 관리가 bool 플래그 조합으로 되어있는데, 명시적인 State 패턴을 적용하면 상태 전이 로직이 더 명확해질 것.

## 이번 프로젝트를 통해 배운 점

- **Rigidbody 물리의 깊은 이해**: velocity 직접 제어, drag/friction의 동작 원리, FixedUpdate 타이밍
- **SphereCast/CapsuleCast 활용**: 단순 Raycast로는 해결할 수 없는 볼륨 기반 충돌 감지
- **ProjectOnPlane**: 경사면에서의 이동 벡터 보정 수학적 원리

### 설계적 학습
- **컴포넌트 분리의 중요성**: ForceReceiver 분리로 팀원 코드와의 결합도를 낮춤

### 협업적 학습
- **팀장으로서의 트레이드오프**: 완벽한 코드보다 동작하는 결과물, 리팩토링보다 일정 준수
- **문서화의 효율성**: 반복되는 질문에 대한 비용을 문서로 줄일 수 있음
- **Git 워크플로우**: 브랜치 전략과 프리팹 기반 작업의 중요성을 체감

## 한 줄 총평

> "Character Controller 없이 캐릭터 물리를 직접 구현하면서 Unity Rigidybody의 사용법에 더욱 익숙해졌고, 팀장으로서 팀을 이끌며 여러가지 이슈에 대처하며 완성도와 일정 사이의 균형을 잡는 법을 배웠다."

---

**작성일**: 2026-02-03  
**작성자**: 이성규