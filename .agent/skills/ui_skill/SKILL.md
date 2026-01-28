---
name: ui_skill
description: Core Identity & Philosophy
Role: Unity UI Architect & Implementation Specialist

Methodology: Atomic Design (Atoms -> Molecules -> Organisms -> Templates)

Target Platform: Android/iOS Mobile (Portrait/Landscape adaptive)
---

This skill creates a new UI Panel script and sets up the basic structure for UGUI + DOTween interactions.

## Workflow Rules (Instructions)
1. **Hierarchy Decomposition**: 이미지의 계층 구조를 최상단 부모(Canvas/Panel)부터 말단 자식(Button/Text)까지 트리 구조로 분해합니다.
2. **Zone Definition**: 화면 영역을 Header(Top), Footer(Bottom), Body(Content), Overlay(Popup)로 명확히 구분하여 모듈화합니다
3. **Naming Convention**: ID Priority: 사용자가 제공한 ID(예: AA1, a1)를 최우선 식별자로 사용합니다.
4. **Descriptive Naming**: Unity Hierarchy 가독성을 위해 기능적 영문 명칭을 병기합니다. (예: Footer_AB1, Btn_Action_a1)
5. **Layout Logic**: Component Based: 모든 배치는 Unity의 RectTransform (Anchor/Pivot 설정) 및 Layout Group (Vertical/Horizontal/Grid) 컴포넌트 설정을 기준으로 설명합니다
6. **Safe Area**: iPhone Notch, Android Camera Hole 대응이 필요한 상단/하단 패널은 반드시 Safe Area 처리 방침을 명시합니다.
7. **Interaction Definition**: 단순 클릭 이외의 기술적 요구사항을 파악하여 명시합니다: Toggle Group (라디오 버튼 탭), Scroll Rect (기본 스크롤), Object Pooling (무한 스크롤/대량 데이터)

## Output Structure Template
1. 모든 응답에는 이 스킬을 얼마나 참고했는지 %를 명시해줄 것(예: 참고율: 90%)
2. Summary: 구현 목표 요약 (Trigger 조건, Close 조건, Modal 여부 포함)
3. Layout Structure: 모듈별 상세 스펙 (Atomic Level 분해, 계층 순서대로 기술)
4. Technical Specs: 주요 컬러값(Hex), 앵커링(Anchoring) 전략, 인터랙션 로직
5. Hierarchy Reference: Unity 인스펙터 구조 트리 (Text Map)

## Quality Enhancement Toolkit (추천 기술 스택)
**Essential Plugins (필수 추천 플러그인)**
1. DOTween Pro: 고품질 UI 애니메이션버튼 활용에 사용, 클릭 시 OnPress 스케일링(0.9x), 팝업 등장 시 FadeIn + Bounce(Ease.OutBack) 효과 적용 명시.
2. TextMeshPro(TMP): Text 대신 TMP_Text 사용. 폰트 에셋 관리 및 SDF 렌더링으로 모든 해상도에서 선명함 유지.
3. EnhancedScroller(또는 LoopScrollRect): (필수)세로 20줄 이상의 리스트(예: AD101) 등 아이템이 많은 경우, Instantiate를 최소화하고 셀을 재사용하는 Object Pooling 구현 제안.
4. Odin Inspector: 복잡한 UI 매니저 스크립트 작성 시, 인스펙터에서 리스트나 딕셔너리를 시각적으로 관리하기 용이하도록 제안.

## Unity Core Components Guide
**Canvas Scaler 설정**
1. UI Scale Mode: Scale With Screen Size
2. Reference Resolution: 1080 x 1920 (Portrait 기준)
3. Match: Width(0) 혹은 Height(1) (세로형 게임은 보통 Width 0.5 ~ 1 권장)
**Atlas & 9-Slice (최적화)**
1. 모든 UI 이미지(버튼, 패널 배경)는 Sprite Atlas로 패킹하여 Draw Call 최소화.
2. 배경 박스는 해상도 대응 시 모서리 왜곡 방지를 위해 9-Slice (Sliced Sprite) 설정 필수.
**Raycast Blocking 제어**
1. 모달 팝업 활성화 시, 뒤쪽(HUD) 터치 방지를 위해 반투명 배경(Image with Alpha)에 Raycast Target: On 설정.
2. 터치가 불필요한 단순 텍스트, 아이콘, 데코레이션 이미지는 Raycast Target: Off를 명시하여 성능 확보.
**Auto Layout & Content Size Fitter**
1. 내용물에 따라 크기가 가변적인 요소(예: 라디오 버튼 그룹)는 Content Size Fitter와 Horizontal Layout Group의 조합 설정을 구체적으로 명시.


