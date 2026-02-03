# FallingObject (낙하 오브젝트 기믹)

특정 시간 동안 공중에 대기한 뒤 낙하하는 오브젝트 기믹이다.  
이 기믹의 핵심은 플레이어와 충돌 시 아래가 아닌 **옆 방향으로 밀어내는 상호작용**에 있다.

- 플레이어가 닿았을 때 트리거되는 방식이 아니라
- **주기적으로 자동 실행되는 기믹**이다.

---

## FallingObject_Init

해당 기믹에서 사용할 데이터들을 참조하고 초기화하는 영역이다.

- Rigidbody, 타이머, 기믹 관련 데이터 초기화
- 낙하 전 상태 세팅 (중력 비활성화 등)

---

## FallingObject_RunForSeconds

RunForSeconds 단계에서는 다음과 같은 흐름으로 동작한다.

- `Rigidbody.useGravity = false` 상태로 오브젝트를 공중에 유지
- 지정한 시간만큼 대기
- 대기 시간이 끝나면 `Rigidbody.useGravity = true`로 전환

> 낙하 시작 타이밍을 제어하는 준비 단계이다.

---

## FallingObject_OnGimmic

`OnGimmic` 메서드는 실제 낙하가 시작되는 시점에 호출된다.

기본적으로는 `useGravity = true`만으로도 낙하가 가능하지만,  
연출적으로 자연스럽지 않다고 판단하여 **추가적인 힘을 직접 적용**했다.

```csharp
_rigid.AddForce(Vector3.down * _AddForcePower, ForceMode.VelocityChange);
