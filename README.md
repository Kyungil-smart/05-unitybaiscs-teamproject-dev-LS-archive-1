# Over The Sky

<img src="https://img.shields.io/badge/Unity-2022.3.62f2-black?style=for-the-badge&logo=unity" alt="Unity">
<img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C#">
<img src="https://img.shields.io/badge/Platform-Windows-0078D6?style=for-the-badge&logo=windows" alt="Platform">


3인칭 3D 플랫포머 게임. 달리기와 점프로 발판을 밟고 장애물을 피하며 가장 높은 곳에 있는 목표지점에 도달하면 된다.


## 프로젝트 개요

| 항목 | 내용 |
|------|------|
| **장르** | 온리업 (Only Up) / 등산 플랫포머 |
| **플레이타임** | 약 1시간 |
| **개발 환경** | Unity 2022.3.62f2 / C# |
| **프로젝트 기간** | 2026.01.28 ~ 2026.02.04 (7일) |
| **팀 구성** | 5인 |

## 👥 팀 구성 및 역할

| 이름 | 역할 | 담당 |
|------|------|------|
| **이성규** | 팀장 | 플레이어 시스템, 카메라, 기믹 베이스, 병합 관리 |
| **김동현** | 팀원 | 스폰 시스템, 맵 디자인, UI, 타이틀 씬 |
| **이인** | 팀원 | 파괴 기믹, 스카이박스 연출, 상호작용 |
| **조규민** | 팀원 | 환경 기믹 (안개/비/카메라 쉐이킹) |
| **허범** | 팀원 | 이동 기믹 (왕복/원형/경로 발판) |

## 게임 특징

- **목표**: 발판을 밟으며 떨어지지 않고 정상까지 올라가기
- **체크포인트**: 도달 시 리스폰 위치로 저장
- **다양한 기믹**: 움직이는 발판, 회전 장애물, 바람 구역, 부서지는 발판 등
- **긴장감**: 떨어지면 체크포인트부터 다시 시작

## 플레이 방법

| 키 | 동작 |
|----|------|
| `W/A/S/D` | 이동 |
| `LeftShift` | 달리기 |
| `SpaceBar` | 점프 |
| `R` | 체크포인트로 리스폰 |
| `Esc` | 커서 잠금 토글 |
| `Alt + F4` | 종료 |

## 기술적 특징

### Rigidbody 기반 커스텀 물리 시스템
학습 목적으로 **Character Controller 없이** 플레이어 물리를 직접 구현했습니다.

- **SphereCast 기반 Ground Check**: 모서리 미끄러짐 방지
- **ProjectOnPlane 경사면 처리**: 자연스러운 경사 이동
- **입력 버퍼링**: Update-FixedUpdate 간 점프 씹힘 방지
- **코요테 타임**: 낙하 직전 점프 허용
- **벽 반동**: 급경사 무한 등반 꼼수 방지

# 실행 환경
- **Unity**: 2022.3.62f2
- **플랫폼**: Windows

# 협업 관리
- 노션, Github

## 라이선스

이 프로젝트는 교육 목적으로 제작되었습니다.

사용된 캐릭터 에셋:
- [Starter Assets - ThirdPerson](https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526) (Unity 공식)

## 문서 바로가기

### 팀 문서
#### OverTheSky 프로젝트 가이드 문서
[![OverTheSky 프로젝트 가이드 문서](https://img.shields.io/badge/Team_Guide-Click_Here-blue?style=for-the-badge&logo=readme)](./Docs/OverTheSky%20프로젝트%20가이드%20문서.md)

#### OverTheSky 프로젝트 역할분담 문서
[![OverTheSky 프로젝트 역할분담 문서](https://img.shields.io/badge/Team_Role-Click_Here-orange?style=for-the-badge&logo=readme)](./Docs/OverTheSky%20프로젝트%20역할%20분담%20문서.md)

### 작업노트

**이성규**<br>
[![작업노트-이성규](https://img.shields.io/badge/작업_노트-Click_Here-blue?style=for-the-badge&logo=readme)](./Docs/문서정리_이성규/8팀%20이성규%20작업%20노트(OverTheSky).md)
[![작업회고-이성규](https://img.shields.io/badge/회고_문서-Click_Here-blue?style=for-the-badge&logo=readme)](./Docs/문서정리_이성규/OverTheSky_유니티%20팀%20협업%20프로젝트%20회고(이성규).md)
[![R&D 문서-이성규](https://img.shields.io/badge/R&D_문서-Click_Here-blue?style=for-the-badge&logo=readme)](./Docs/문서정리_이성규/OverTheSky_R&D%20문서_이성규.md)

**김동현**<br>
[![작업노트-김동현](https://img.shields.io/badge/Work_Note-Click_Here-blue?style=for-the-badge&logo=readme)](./Docs/문서정리_김동현/8팀_김동현_작업노트.md)

**이인**<br>
[![작업노트-이인](https://img.shields.io/badge/Work_Note-Click_Here-blue?style=for-the-badge&logo=readme)](./Docs/문서정리_이인)

**조규민**<br>
[![작업노트-조규민](https://img.shields.io/badge/Work_Note-Click_Here-blue?style=for-the-badge&logo=readme)](./Docs/문서정리_조규민/8팀_조규민_작업노트.md)

**허범**<br>
[![작업노트-허범](https://img.shields.io/badge/Work_Note-Click_Here-blue?style=for-the-badge&logo=readme)](./Docs/문서정리_허범/8팀%20허범%20작업노트.md)