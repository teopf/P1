---
name: ui_skill
description: Guidelines for Unity UI architecture and implementation using UGUI patterns and Atomic Design.
---

# UI Skill: Unity UI Architect & Implementation Specialist

**Role**: Unity UI Architect & Implementation Specialist
**Methodology**: Atomic Design (Atoms -> Molecules -> Organisms -> Templates)
**Target Platform**: Android/iOS Mobile (Portrait/Landscape adaptive)

This skill provides guidelines and workflows for creating high-quality, scalable UI in Unity. It creates a new UI Panel script and sets up the basic structure for UGUI + DOTween interactions.

## Workflow Rules (Instructions)

1.  **Hierarchy Decomposition**: Decompose the image hierarchy into a tree structure from the top-level parent (Canvas/Panel) to leaf children (Button/Text).
2.  **Zone Definition**: Clearly distinguish screen areas into Header (Top), Footer (Bottom), Body (Content), and Overlay (Popup) for modularization.
3.  **Naming Convention**:
    *   **ID Priority**: Use user-provided IDs (e.g., AA1, a1) as the primary identifier.
    *   **Descriptive Naming**: Append functional English names for Unity Hierarchy readability (e.g., `Footer_AB1`, `Btn_Action_a1`).
4.  **Layout Logic**:
    *   **Component Based**: Describe all layouts based on Unity's `RectTransform` (Anchor/Pivot settings) and Layout Group (`Vertical`/`Horizontal`/`Grid`) component settings.
5.  **Safe Area**: Explicitly state the Safe Area handling policy for top/bottom panels needing iPhone Notch or Android Camera Hole support.
6.  **Interaction Definition**: Identify technical requirements beyond simple clicks:
    *   Toggle Group (Radio button tabs)
    *   Scroll Rect (Basic scrolling)
    *   Object Pooling (Infinite scroll / Large datasets)
7.  **UGUI Reference (CRITICAL)**: **ALWAYS** refer to **Unity UGUI (`Packages/com.unity.ugui`)** source code and patterns. Write code in a standardized way (e.g., managing lifecycle, implementing standard interfaces like `IPointerEnterHandler`, inheriting from `UIBehaviour` or custom `UIBase`).

## Output Structure Template

 When using this skill, please structure your response as follows:

1.  **Reference Rate**: Explicitly state how much this skill was referenced (e.g., "Reference Rate: 90%").
2.  **Summary**: Implementation goal summary (Trigger conditions, Close conditions, Modal status).
3.  **Layout Structure**: Detailed specs by module (Atomic Level decomposition, listed in hierarchy order).
4.  **Technical Specs**: Key color values (Hex), Anchoring strategy, Interaction logic.
5.  **Hierarchy Reference**: Unity Inspector structure tree (Text Map).

## Quality Enhancement Toolkit (Recommended Tech Stack)

**Essential Plugins**
1.  **DOTween Pro**: Use for high-quality UI animations. Specify effects like OnPress scaling (0.9x) for buttons, and FadeIn + Bounce (Ease.OutBack) for popups.
2.  **TextMeshPro (TMP)**: Use `TMP_Text` instead of legacy `Text`. Manage font assets and use SDF rendering for clarity on all resolutions.
3.  **EnhancedScroller (or LoopScrollRect)**: **(Mandatory)** For lists with 20+ items (e.g., AD101), propose Object Pooling implementation to minimize `Instantiate` calls and reuse cells.
4.  **Odin Inspector**: Propose using Odin for complex UI manager scripts to visually manage lists or dictionaries in the Inspector.

## Unity Core Components Guide

**Canvas Scaler Settings**
1.  **UI Scale Mode**: Scale With Screen Size
2.  **Reference Resolution**: 1080 x 1920 (Portrait base)
3.  **Match**: Width(0) or Height(1) (0.5 ~ 1 recommended for portrait games)

**Atlas & 9-Slice (Optimization)**
1.  Pack all UI images (buttons, panel backgrounds) into **Sprite Atlas** to minimize Draw Calls.
2.  Set **9-Slice (Sliced Sprite)** for background boxes to prevent corner distortion during resolution adaptation.

**Raycast Blocking Control**
1.  **Modal Popups**: Set `Raycast Target: On` for the semi-transparent background (Image with Alpha) to prevent touching the HUD behind it.
2.  **Performance**: Explicitly set `Raycast Target: Off` for simple text, icons, and decoration images that do not require touch interaction.

**Auto Layout & Content Size Fitter**
1.  For variable-size elements (e.g., Radio Button Groups), explicitly specify the combination of `Content Size Fitter` and `Horizontal Layout Group`.
