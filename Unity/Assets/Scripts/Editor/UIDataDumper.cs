using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class UIDataDumper : EditorWindow
{
    [MenuItem("Tools/UI/Dump UI Data")]
    public static void Dump()
    {
        StringBuilder sb = new StringBuilder();
        
        // Target 1: Chat Overlay
        DumpRecursively(sb, "Canvas_ChatOverlay");

        // Target 2: A2 Inventory
        DumpRecursively(sb, "Canvas_A2_Inventory");

        // Target 3: HUD (Check specific buttons)
        DumpRecursively(sb, "HUD_Canvas"); // Or whatever the root is named in HUDGenerator usually
        // Also check "SubMenu_Canvas" just in case
        DumpRecursively(sb, "SubMenu_Canvas");

        string path = "Assets/UI_Dump.txt";
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"UI Data Dumped to {path}");
        AssetDatabase.Refresh();
    }

    private static void DumpRecursively(StringBuilder sb, string rootName)
    {
        // Improved Find for Inactive Objects
        GameObject root = FindObjectEvenIfInactive(rootName);
        if (root == null) 
        {
            sb.AppendLine($"MISSING: {rootName}");
            return;
        }

        sb.AppendLine($"ROOT: {rootName}");
        DumpTransform(sb, root.transform, "");
        sb.AppendLine("--------------------------------------------------");
    }

    private static GameObject FindObjectEvenIfInactive(string name)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        foreach (var t in objs)
        {
            if (t.hideFlags == HideFlags.None && t.name == name && t.gameObject.scene.IsValid())
            {
                return t.gameObject;
            }
        }
        return null;
    }

    private static void DumpTransform(StringBuilder sb, Transform t, string prefix)
    {
        RectTransform rt = t.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Format: PATH | AnchorMin | AnchorMax | Pivot | AnchoredPosition | SizeDelta
            string path = string.IsNullOrEmpty(prefix) ? t.name : $"{prefix}/{t.name}";
            string line = $"{path} | MIN:{rt.anchorMin} | MAX:{rt.anchorMax} | PIVOT:{rt.pivot} | POS:{rt.anchoredPosition} | SIZE:{rt.sizeDelta}";
            sb.AppendLine(line);
        }

        foreach (Transform child in t)
        {
            DumpTransform(sb, child, string.IsNullOrEmpty(prefix) ? t.name : $"{prefix}/{t.name}");
        }
    }
}
