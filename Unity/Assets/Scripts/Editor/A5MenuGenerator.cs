using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// a5 퀘스트 메뉴 UI 레이아웃 자동 생성 에디터 도구
/// </summary>
public class A5MenuGenerator : EditorWindow
{
    // 기준 해상도 (Android Portrait)
    private const int REF_WIDTH = 1080;
    private const int REF_HEIGHT = 1920;

    // 색상 정의 (A2MenuGenerator 패턴 따름)
    private static readonly Color COLOR_PANEL_HEADER = new Color32(176, 196, 222, 255); // #B0C4DE
    private static readonly Color COLOR_PANEL_BG = new Color32(107, 140, 255, 255);     // Blue/Purple gradient start
    private static readonly Color COLOR_PANEL_BG_END = new Color32(155, 107, 255, 255); // Blue/Purple gradient end

    private static readonly Color COLOR_BTN_ORANGE = new Color32(255, 136, 0, 64);      // Default Orange (25% opacity)
    private static readonly Color COLOR_BTN_ORANGE_ACTIVE = new Color32(255, 110, 0, 128); // Active Orange

    private static readonly Color COLOR_ITEM_BG = new Color32(255, 0, 0, 64);           // Red/Salmon (25%)
    private static readonly Color COLOR_ITEM_GREEN = new Color32(168, 255, 178, 128);   // Light Green (50%)
    private static readonly Color COLOR_ITEM_BLUE = new Color32(0, 43, 255, 64);        // Blue (25%)

    [MenuItem("Tools/UI/Generate a5 Quest Menu")]
    public static void Generate()
    {
        string canvasName = "Canvas_a5_Quest";
        GameObject canvasObj = GameObject.Find(canvasName);
        if (canvasObj != null) DestroyImmediate(canvasObj);

        // Canvas 생성
        canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 35; // a3(30)과 a6(40) 사이

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_WIDTH, REF_HEIGHT);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // A5MenuController 추가
        canvasObj.AddComponent<A5MenuController>();

        // 초기 비활성화
        canvasObj.SetActive(false);

        Sprite roundedSprite = GetRoundedSprite();

        // 1. AA101 Top Header (재사용 패턴)
        CreateHeader(canvasObj.transform, roundedSprite);

        // 2. AD601 Scroll View (퀘스트 리스트) - 하이어라키 순서 맞춤
        CreateQuestScrollView(canvasObj.transform, roundedSprite);

        // 3. AD611-AD615 Milestone Rewards
        CreateMilestones(canvasObj.transform, roundedSprite);

        // 4. AD602 Overall Progress Bar
        CreateOverallProgress(canvasObj.transform, roundedSprite);

        // 5. AA503 Category Toggle
        CreateCategoryToggle(canvasObj.transform, roundedSprite);

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create a5 Quest Menu Layout");
        Selection.activeGameObject = canvasObj;
        Debug.Log("a5 Quest Menu Layout Generated Successfully!");
    }

    /// <summary>
    /// 상단 헤더 패널 생성 (AA101)
    /// </summary>
    private static void CreateHeader(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateImage("Panel_AA101_Header", parent, sprite, COLOR_PANEL_HEADER);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -63);
        rt.sizeDelta = new Vector2(0, 90);

        SetGradient(panel.GetComponent<Image>(), COLOR_PANEL_BG, COLOR_PANEL_BG_END);

        // Btn_Close_b101: 우측 정렬, 90x90
        CreateButton("Btn_Close_b101", panel.transform, sprite, COLOR_BTN_ORANGE,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-27 - 45, 0), new Vector2(90, 90));
    }

    /// <summary>
    /// 전체 퀘스트 진행률 바 생성 (AD602)
    /// </summary>
    private static void CreateOverallProgress(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateRect("Panel_AD602_OverallProgress", parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -153);
        rt.sizeDelta = new Vector2(0, 120);

        // 진행률 바 배경
        GameObject bgBar = CreateImage("ProgressBar_Background", panel.transform, sprite, COLOR_ITEM_BG);
        RectTransform bgRT = bgBar.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0.5f, 0);
        bgRT.anchorMax = new Vector2(0.5f, 0);
        bgRT.pivot = new Vector2(0.5f, 0.5f);
        bgRT.anchoredPosition = Vector2.zero;
        bgRT.sizeDelta = new Vector2(945, 25);

        // 진행률 바 Fill (Image fillAmount 사용)
        GameObject fillBar = CreateImage("ProgressBar_Fill", bgBar.transform, sprite, new Color32(255, 215, 0, 255));
        RectTransform fillRT = fillBar.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0, 0.5f);
        fillRT.anchorMax = new Vector2(0, 0.5f);
        fillRT.pivot = new Vector2(0, 0.5f);
        fillRT.anchoredPosition = Vector2.zero;
        fillRT.sizeDelta = new Vector2(945, 17);
        Image fillImg = fillBar.GetComponent<Image>();
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0.75f; // 75% 기본값

        // 진행률 텍스트
        CreateText("Text_Progress", panel.transform, "Daily: 15/20", 43);
    }

    /// <summary>
    /// 마일스톤 보상 슬롯 생성 (AD611-AD615)
    /// </summary>
    private static void CreateMilestones(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateRect("Panel_AD611_Milestones", parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -273);
        rt.sizeDelta = new Vector2(0, 100);

        // 5개 마일스톤 슬롯 (AD611-AD615)
        float[] xPositions = { 107, 319, 523, 731, 935 }; // 좌측 기준 위치
        string[] ids = { "AD611", "AD612", "AD613", "AD614", "AD615" };
        string[] labels = { "5/20", "10/20", "15/20", "18/20", "20/20" };

        for (int i = 0; i < 5; i++)
        {
            GameObject slot = CreateImage($"Milestone_{ids[i]}", panel.transform, sprite, COLOR_ITEM_BG);
            RectTransform slotRT = slot.GetComponent<RectTransform>();
            slotRT.anchorMin = new Vector2(0, 0.5f);
            slotRT.anchorMax = new Vector2(0, 0.5f);
            slotRT.pivot = new Vector2(0, 0.5f);
            slotRT.anchoredPosition = new Vector2(xPositions[i], 0);
            slotRT.sizeDelta = new Vector2(80, 80);

            CreateText($"Label_{ids[i]}", slot.transform, labels[i], 24);
        }
    }

    /// <summary>
    /// 퀘스트 스크롤 뷰 생성 (AD601)
    /// </summary>
    private static void CreateQuestScrollView(Transform parent, Sprite sprite)
    {
        // ScrollView 패널 (하단까지 stretch)
        GameObject panel = CreateImage("Panel_AD601_ScrollView", parent, sprite, COLOR_PANEL_HEADER);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -153);
        rt.sizeDelta = new Vector2(0, -413);

        SetGradient(panel.GetComponent<Image>(), COLOR_PANEL_BG, COLOR_PANEL_BG_END);

        // ScrollRect 설정
        ScrollRect sr = panel.AddComponent<ScrollRect>();
        sr.vertical = true;
        sr.horizontal = false;
        sr.movementType = ScrollRect.MovementType.Elastic;
        sr.scrollSensitivity = 20;

        // Viewport
        GameObject viewport = CreateRect("Viewport", panel.transform);
        RectTransform vpRT = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.pivot = new Vector2(0.5f, 0.5f);
        vpRT.anchoredPosition = new Vector2(0, -125);
        vpRT.sizeDelta = new Vector2(0, -250);
        viewport.AddComponent<Image>().color = new Color(1,1,1,0.01f); // Transparent raycast target
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        sr.viewport = vpRT;

        // Content
        GameObject content = CreateRect("Content", viewport.transform);
        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 0.5f);
        contentRT.anchorMax = new Vector2(1, 0.5f);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = new Vector2(0, 628.5f);
        contentRT.sizeDelta = new Vector2(-180, 896);
        sr.content = contentRT;

        // ContentSizeFitter로 동적 크기 조정
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // VerticalLayoutGroup으로 퀘스트 엔트리 수직 배치
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(0, 0, 20, 20); // 좌우 0px, 상하 20px
        vlg.spacing = 8; // 엔트리 간 간격
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = false;
        vlg.childControlWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = false;

        // QuestScrollView 컴포넌트 추가
        QuestScrollView scrollView = content.AddComponent<QuestScrollView>();
        scrollView.contentRoot = contentRT;

        // 8개 퀘스트 엔트리 생성 (Group_9 ~ Group_2)
        for (int i = 9; i >= 2; i--)
        {
            CreateQuestEntry($"Group_{i}", content.transform, sprite, i);
        }
    }

    /// <summary>
    /// 개별 퀘스트 엔트리 생성
    /// </summary>
    private static void CreateQuestEntry(string name, Transform parent, Sprite sprite, int index)
    {
        GameObject group = CreateRect(name, parent);
        RectTransform groupRT = group.GetComponent<RectTransform>();
        groupRT.anchorMin = new Vector2(0, 1);
        groupRT.anchorMax = new Vector2(0, 1);
        groupRT.pivot = new Vector2(0.5f, 0.5f);
        groupRT.sizeDelta = new Vector2(900, 100);  // anchor가 한 점에 고정되어 있으므로 sizeDelta가 실제 크기

        LayoutElement le = group.AddComponent<LayoutElement>();
        le.preferredWidth = 900;
        le.preferredHeight = 100;

        // 배경 카드
        GameObject bg = CreateImage("Background", group.transform, sprite, COLOR_ITEM_GREEN);
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;

        // 좌측 아이콘 (AD6xx)
        GameObject icon = CreateImage($"AD6{10 + index}_Icon", group.transform, sprite, COLOR_ITEM_BG);
        RectTransform iconRT = icon.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0, 0.5f);
        iconRT.anchorMax = new Vector2(0, 0.5f);
        iconRT.pivot = new Vector2(0, 0.5f);
        iconRT.anchoredPosition = new Vector2(20, 0);
        iconRT.sizeDelta = new Vector2(80, 80);

        // 중앙 퀘스트 정보 영역
        GameObject questInfo = CreateRect("QuestInfo", group.transform);
        RectTransform qiRT = questInfo.GetComponent<RectTransform>();
        qiRT.anchorMin = new Vector2(0, 0);
        qiRT.anchorMax = new Vector2(1, 1);
        qiRT.offsetMin = new Vector2(110, 10); // 좌측 110px, 하단 10px
        qiRT.offsetMax = new Vector2(-110, -10); // 우측 110px, 상단 10px

        // 퀘스트 제목 (QTxxx)
        GameObject title = CreateRect($"QT{100 + index}_Title", questInfo.transform);
        RectTransform titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, -5);
        titleRT.sizeDelta = new Vector2(0, 30);
        Text titleText = title.AddComponent<Text>();
        titleText.text = $"Quest {index}: Complete Task";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 24;
        titleText.alignment = TextAnchor.MiddleLeft;
        titleText.color = Color.black;

        // 퀘스트 설명
        GameObject desc = CreateRect($"QT{100 + index}_Desc", questInfo.transform);
        RectTransform descRT = desc.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0, 0.5f);
        descRT.anchorMax = new Vector2(1, 0.5f);
        descRT.pivot = new Vector2(0.5f, 0.5f);
        descRT.anchoredPosition = new Vector2(0, 0);
        descRT.sizeDelta = new Vector2(0, 20);
        Text descText = desc.AddComponent<Text>();
        descText.text = "Description of the quest task";
        descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        descText.fontSize = 16;
        descText.alignment = TextAnchor.MiddleLeft;
        descText.color = new Color(0.3f, 0.3f, 0.3f);

        // 진행률 바 (QTPxxx)
        GameObject progressBarContainer = CreateRect($"QTP{100 + index}_ProgressBar", questInfo.transform);
        RectTransform pbRT = progressBarContainer.GetComponent<RectTransform>();
        pbRT.anchorMin = new Vector2(0, 0);
        pbRT.anchorMax = new Vector2(1, 0);
        pbRT.pivot = new Vector2(0.5f, 0);
        pbRT.anchoredPosition = new Vector2(0, 5);
        pbRT.sizeDelta = new Vector2(0, 20);

        // 진행률 바 배경
        GameObject pbBg = CreateImage("ProgressBar_Background", progressBarContainer.transform, sprite, COLOR_ITEM_BG);
        RectTransform pbBgRT = pbBg.GetComponent<RectTransform>();
        pbBgRT.anchorMin = Vector2.zero;
        pbBgRT.anchorMax = Vector2.one;
        pbBgRT.sizeDelta = Vector2.zero;

        // 진행률 바 Fill
        GameObject pbFill = CreateImage("ProgressBar_Fill", pbBg.transform, sprite, new Color32(76, 175, 80, 255));
        RectTransform pbFillRT = pbFill.GetComponent<RectTransform>();
        pbFillRT.anchorMin = new Vector2(0, 0);
        pbFillRT.anchorMax = new Vector2(0, 1);
        pbFillRT.pivot = new Vector2(0, 0.5f);
        pbFillRT.anchoredPosition = Vector2.zero;
        pbFillRT.sizeDelta = new Vector2(100, 0); // 기본 100px 너비
        Image pbFillImg = pbFill.GetComponent<Image>();
        pbFillImg.type = Image.Type.Filled;
        pbFillImg.fillMethod = Image.FillMethod.Horizontal;
        pbFillImg.fillAmount = 0.7f; // 70% 기본값

        // 진행률 텍스트
        GameObject pbText = CreateRect("ProgressText", pbBg.transform);
        RectTransform pbTextRT = pbText.GetComponent<RectTransform>();
        pbTextRT.anchorMin = Vector2.zero;
        pbTextRT.anchorMax = Vector2.one;
        pbTextRT.sizeDelta = Vector2.zero;
        Text pbTextComp = pbText.AddComponent<Text>();
        pbTextComp.text = "7/10";
        pbTextComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pbTextComp.fontSize = 16;
        pbTextComp.alignment = TextAnchor.MiddleCenter;
        pbTextComp.color = Color.white;

        // 우측 Claim 버튼 (QOKxxx)
        GameObject claimBtn = CreateButton($"QOK{100 + index}_ClaimBtn", group.transform, sprite, COLOR_BTN_ORANGE,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-20 - 45, 0), new Vector2(90, 90));
    }

    /// <summary>
    /// 카테고리 토글 패널 생성 (AA503)
    /// </summary>
    private static void CreateCategoryToggle(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateImage("Panel_AA503_Category", parent, sprite, COLOR_PANEL_HEADER);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -1641);
        rt.sizeDelta = new Vector2(0, 90);

        SetGradient(panel.GetComponent<Image>(), COLOR_PANEL_BG, COLOR_PANEL_BG_END);

        ToggleGroup group = panel.AddComponent<ToggleGroup>();
        group.allowSwitchOff = false;

        // C501~C504 토글 (Daily, Weekly, Achievement, Repeat)
        float[] xPositions = { 17, 278, 542, 804 };
        float[] widths = { 261, 262, 262, 261 };
        string[] ids = { "C501", "C502", "C503", "C504" };
        string[] labels = { "Daily", "Weekly", "Achievement", "Repeat" };

        for (int i = 0; i < 4; i++)
        {
            CreateToggle($"Tgl_Category_{ids[i]}", panel.transform, sprite, group, xPositions[i], widths[i], labels[i]);
        }
    }

    /// <summary>
    /// 토글 생성 헬퍼
    /// </summary>
    private static void CreateToggle(string name, Transform parent, Sprite sprite, ToggleGroup group, float xPos, float width, string label)
    {
        GameObject toggleObj = CreateImage(name, parent, sprite, COLOR_BTN_ORANGE);
        RectTransform rt = toggleObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = new Vector2(xPos, 0);
        rt.sizeDelta = new Vector2(width, 90);

        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.group = group;
        toggle.targetGraphic = toggleObj.GetComponent<Image>();

        ColorBlock cb = toggle.colors;
        cb.normalColor = COLOR_BTN_ORANGE;
        cb.selectedColor = COLOR_BTN_ORANGE_ACTIVE;
        toggle.colors = cb;

        CreateText("Label", toggleObj.transform, label, 24);
    }


    // ========== 헬퍼 메서드 ==========

    /// <summary>
    /// RectTransform GameObject 생성
    /// </summary>
    private static GameObject CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    /// <summary>
    /// Image GameObject 생성
    /// </summary>
    private static GameObject CreateImage(string name, Transform parent, Sprite sprite, Color color)
    {
        GameObject go = CreateRect(name, parent);
        Image img = go.AddComponent<Image>();
        if(sprite != null) img.sprite = sprite;
        img.color = color;
        img.type = Image.Type.Sliced;
        return go;
    }

    /// <summary>
    /// Text GameObject 생성
    /// </summary>
    private static void CreateText(string name, Transform parent, string content, int fontSize = 24)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.fontSize = fontSize;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = fontSize + 10;

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Button GameObject 생성
    /// </summary>
    private static GameObject CreateButton(string name, Transform parent, Sprite sprite, Color color, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = CreateImage(name, parent, sprite, color);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Button btn = btnObj.AddComponent<Button>();
        CreateText("Text", btnObj.transform, name.Replace("Btn_Menu_", "").Replace("Btn_Action_", "").Replace("Btn_Close_", "").Replace("QOK", "").Replace("_ClaimBtn", ""), 20);

        Shadow shadow = btnObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.2f);
        shadow.effectDistance = new Vector2(0, -4);

        return btnObj;
    }

    /// <summary>
    /// 그라디언트 텍스처 생성 및 적용
    /// </summary>
    private static void SetGradient(Image img, Color top, Color bottom)
    {
        Texture2D tex = new Texture2D(1, 16);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < 16; y++)
        {
            tex.SetPixel(0, y, Color.Lerp(bottom, top, (float)y/15f));
        }
        tex.Apply();
        img.sprite = Sprite.Create(tex, new Rect(0,0,1,16), new Vector2(0.5f, 0.5f));
        img.type = Image.Type.Sliced;
    }

    /// <summary>
    /// 둥근 모서리 스프라이트 생성
    /// </summary>
    private static Sprite GetRoundedSprite()
    {
        int size = 64;
        int radius = 12;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                colors[y * size + x] = Color.white;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }
}
