using UnityEngine;
using UnityEditor;
using System.Text;

public class UIPositionDumper : EditorWindow
{
    [MenuItem("Tools/UI/Dump a3 Growth Menu Positions")]
    public static void DumpPositions()
    {
        // Find including inactive objects
        GameObject canvasObj = null;
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "Canvas_a3_Growth" && go.scene.IsValid())
            {
                canvasObj = go;
                break;
            }
        }
        
        if (canvasObj == null)
        {
            Debug.LogError("Canvas_a3_Growth not found in scene! Make sure you've generated it first with Tools > Generate a3 Growth Menu");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== Canvas_a3_Growth Position Dump ===\n");
        
        DumpRecursive(canvasObj.transform, sb, 0);
        
        Debug.Log(sb.ToString());
        
        // Also copy to clipboard
        GUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log("Position data copied to clipboard!");
    }

    private static void DumpRecursive(Transform t, StringBuilder sb, int indent)
    {
        string prefix = new string(' ', indent * 2);
        RectTransform rt = t.GetComponent<RectTransform>();
        
        if (rt != null)
        {
            sb.AppendLine($"{prefix}[{t.name}]");
            sb.AppendLine($"{prefix}  anchorMin: {rt.anchorMin}");
            sb.AppendLine($"{prefix}  anchorMax: {rt.anchorMax}");
            sb.AppendLine($"{prefix}  pivot: {rt.pivot}");
            sb.AppendLine($"{prefix}  anchoredPosition: {rt.anchoredPosition}");
            sb.AppendLine($"{prefix}  sizeDelta: {rt.sizeDelta}");
            sb.AppendLine($"{prefix}  offsetMin: {rt.offsetMin}");
            sb.AppendLine($"{prefix}  offsetMax: {rt.offsetMax}");
            sb.AppendLine();
        }

        foreach (Transform child in t)
        {
            DumpRecursive(child, sb, indent + 1);
        }
    }
}
