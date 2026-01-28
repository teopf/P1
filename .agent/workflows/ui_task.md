---
description: UI 구성 요소 설계 및 구현을 위한 표준 절차 (UI 스킬 준수 필수)
---

1. **[CRITICAL] UI 스킬 파일 정독**: UI 작업 시작 전, 반드시 UI Skill 가이드라인을 로드하여 규칙을 확인합니다.
   - `view_file .agent/skills/ui_skill/SKILL.md`

2. **요구사항 분석 및 설계**:
   - UI 스킬의 'Layout Structure' 템플릿에 맞춰 요구사항을 분해합니다.
   - Atomic Design, Naming Convention(ID_Function), Quality Check(DOTween, TMP) 적용 여부를 확인합니다.

3. **구현 계획(Implementation Plan) 수립**:
   - `implementation_plan.md`에 UI 스킬 준수 여부를 명시하는 섹션을 포함합니다.
   - 계층 구조(Hierarchy)를 미리 텍스트로 정의합니다.

4. **코드 생성 및 적용**:
   - 에디터 스크립트(Generator) 및 런타임 컨트롤러(Controller)를 구현합니다.
