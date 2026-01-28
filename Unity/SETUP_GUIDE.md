# 3D 모바일 자동 전투 RPG - 설정 가이드

## Unity 프로젝트 설정

### 1. 데이터 생성
1. Unity Editor를 엽니다.
2. 상단 메뉴에서 `Tools > Game Data > Generate All Sample Data`를 클릭합니다.
3. `Assets/GameData` 폴더에 다음 데이터가 생성됩니다:
   - Skills (9개)
   - CharacterSpecs (15개)
   - Characters (3개)
   - Enemies (5개)

### 2. 씬 설정
1. 새 씬을 생성하거나 기존 씬을 엽니다.
2. 빈 GameObject를 생성하고 이름을 `GameManager`로 변경합니다.
3. `GameManager` GameObject에 `GameManager.cs` 컴포넌트를 추가합니다.
4. Auto Setup이 체크되어 있는지 확인합니다.

### 3. CheatManager 설정
1. Hierarchy에서 `CheatManager` GameObject를 찾습니다.
2. Inspector에서 다음을 설정합니다:
   - Troop Manager: `TroopManager` GameObject 할당
   - Player Spawn Point: 플레이어 생성 위치 (빈 GameObject 생성 후 할당)
   - Enemy Spawn Area: 적 생성 영역 (빈 GameObject 생성 후 할당)
   - Character Data Path: `Assets/GameData/Characters`
   - Enemy Data Path: `Assets/GameData/Enemies`

### 4. 태그 설정
1. `Edit > Project Settings > Tags and Layers`로 이동합니다.
2. Tags에 다음을 추가합니다:
   - `Player`
   - `Enemy`

### 5. 입력 축 확인
1. `Edit > Project Settings > Input Manager`로 이동합니다.
2. `Horizontal`과 `Vertical` 축이 설정되어 있는지 확인합니다.

## 치트 명령어

### 키보드 단축키
- **F1**: SpawnTroop (무작위 10명 부대 생성)
- **F2**: SpawnEnemy (적 5마리 소환)
- **1**: 스킬 1 강제 사용
- **2**: 스킬 2 강제 사용
- **3**: 스킬 3 강제 사용

### WASD 이동
- **W/A/S/D**: 부대 전체 이동 명령

## 폴더 구조
```
Assets/
├── GameData/           # ScriptableObject 데이터
│   ├── Characters/
│   ├── CharacterSpecs/
│   ├── Skills/
│   └── Enemies/
├── Scripts/
│   ├── Data/          # ScriptableObject 클래스
│   ├── Core/          # 핵심 시스템 (TroopManager, UnitBase 등)
│   ├── AI/            # 전투 AI (PlayerCharacter, EnemyCharacter, Projectile)
│   ├── Utility/       # 유틸리티 (GameManager, CheatManager)
│   └── Editor/        # 에디터 스크립트
```

## 실행 방법
1. Play 버튼 클릭
2. F1 키를 눌러 부대 생성
3. F2 키를 눌러 적 소환
4. WASD로 부대 이동 제어
5. 1/2/3 키로 스킬 강제 사용 테스트
