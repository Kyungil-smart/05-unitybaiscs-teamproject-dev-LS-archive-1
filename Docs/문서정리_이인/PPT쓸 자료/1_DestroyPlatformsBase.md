
# DestroyPlatformsBase  
DestroyPlatforms에서 사용하는 **발판 기믹 정의용 추상 ScriptableObject**이다.   
ScriptableObject : 씬(Scene)에 종속되지 않고 프로젝트 에셋으로 저장되는 데이터 컨테이너 클래스  

DestroyPlatforms는 발판 기믹이 **언제 실행될지(트리거, 생명주기)** 만 관리하며,    
실제 동작 로직은 DestroyPlatformsBase를 상속한 구현체에 위임한다.
 
DestroyPlatformsBase 내부에는 

| 구분       | 이름                               | 타입                     | 설명                                     |
| -------- | -------------------------------- | ---------------------- | -------------------------------------- |
| Field    | `m_Time`                         | `float`                | 발판이 트리거된 후 몇초 동안 지연될건지 정해주는 변수|
| Property | `Time`                           | `float` (read-only)    | `m_Time` 접근용 프로퍼티                      |
| Field    | `m_isTriggeredByPlayer`          | `bool`                 | 플레이어에 의해 발동되는 기믹인지, 주기적으로 동작하는 기믹인지 판별 |
| Property | `isTriggeredByPlayer`            | `bool` (read-only)     | 트리거 방식 확인용 프로퍼티                        |
| Field    | `m_respawnTime`                  | `float`                | 비활성화된 발판이 다시 활성화되기까지의 시간               |
| Property | `respawnTime`                    | `float` (read-only)    | 재생성 시간 접근용 프로퍼티                        |
| Method   | `OnGimmic()`                     | `abstract void`        | 발판이 실제로 사라지거나 동작하는 핵심 기믹 실행 메서드        |
| Method   | `Init(...)`                      | `abstract void`        | 발판 초기화용 메서드 (오너, 비활성 대상, 활성화 관리자 전달)   |
| Method   | `RunForSeconds(Renderer render)` | `abstract IEnumerator` | OnGimmic 실행되기 전에 실행되는 연출 및 대기 로직           |


---


DestroyPlatformsBase를 상속한 ScriptableObject에서  
발판별로 다른 파괴 방식, 연출, 재생성 로직을 구현한다.  

이 구조를 통해 발판 기믹을 코드 수정 없이 ScriptableObject 교체만으로 확장할 수 있다.