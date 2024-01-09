using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    
    public static ItemManager instance { get; private set; }
    public List<ItemInfo> prefab_items = new();
    private void Awake()
    {
        instance = this;

        prefab_items.Clear();
        for (int i = 0; i < transform.childCount; i++)
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                prefab_items.Add(transform.GetChild(i).GetComponent<ItemInfo>());
                //prefab_items[i].id_in_library = i;
            }
    }
    public ItemInfo GetItemByName(string name)
    {
        for (int i = 0; i < prefab_items.Count; i++)
            if (prefab_items[i].name.Contains(name))
                return prefab_items[i];
        return null;
    }
}
