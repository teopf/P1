using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryScrollView : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject itemPrefab; // We'll assume the generator created slots we can clone or use existing
    public Transform contentRoot;
    
    private List<GameObject> activeItems = new List<GameObject>();
    private List<GameObject> pooledItems = new List<GameObject>();

    private void Awake()
    {
        if (contentRoot == null) contentRoot = GetComponent<ScrollRect>()?.content;
        
        // Initialize pool from existing items created by Generator
        if (contentRoot != null)
        {
            // Move existing children to pool
            int childCount = contentRoot.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                var child = contentRoot.GetChild(i).gameObject;
                child.SetActive(false);
                pooledItems.Add(child);
                if (itemPrefab == null) itemPrefab = child; // Use first one as template
            }
        }
    }

    public void RefreshData(string category)
    {
        // Return existing items to pool
        foreach (var item in activeItems)
        {
            item.SetActive(false);
            pooledItems.Add(item);
        }
        activeItems.Clear();

        // Generate Mock Data
        int itemCount = 0;
        switch (category)
        {
            case "C201": itemCount = 15; break; // Equipment
            case "C202": itemCount = 45; break; // Consumables
            case "C203": itemCount = 5; break;  // Quest
            default: itemCount = 25; break;     // All
        }

        // Spawn Items
        for (int i = 0; i < itemCount; i++)
        {
            GameObject item = GetItemFromPool();
            SetupItem(item, i, category);
            activeItems.Add(item);
        }
    }

    private GameObject GetItemFromPool()
    {
        if (pooledItems.Count > 0)
        {
            var item = pooledItems[0];
            pooledItems.RemoveAt(0);
            item.SetActive(true);
            return item;
        }
        else
        {
            // Instantiate new if pool empty
            if (itemPrefab != null)
            {
                var item = Instantiate(itemPrefab, contentRoot);
                item.SetActive(true);
                return item;
            }
        }
        return null;
    }

    private void SetupItem(GameObject item, int index, string category)
    {
        item.name = $"Slot_Item_{category}_{index}";
        // Find Text to update
        var text = item.transform.Find("Img_InfoBar/Txt_Info")?.GetComponent<Text>();
        if (text != null)
        {
            text.text = $"{category}_Item_{index + 1}";
        }
        
        // Random Color variation for visual check
        var topImg = item.transform.Find("Img_ItemDisplay")?.GetComponent<Image>();
        if(topImg) topImg.color = Color.HSVToRGB(Random.value, 0.5f, 0.8f);
    }
}
