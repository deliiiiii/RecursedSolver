using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Box;
using static UnityEditor.Progress;
public class Box : Item
{
    //public int id_in_map;
    //public bool canExit;
    public int t;
    public Box t_localBox;
    public Item t_item_load;
    public Box P_Box=null;//Parent

    public List<Vector2Int> checkPoints;
    public Transform P_item_initial;
    public List<Item> items_initial = new();
    public Transform P_item_existing;
    public List<Item> items_exsiting = new();

    public Transform P_conversion;
    public GameObject prefab_conversion;
    public List<Conversion> conversions = new();
    public class Conversion
    {
        public List<Item> item_costs;
        public List<Item> item_rewards;
        public bool canRegainCost;
    }

    //#region triggerFuncs
    //public delegate void TriggerFuncs(object condition, object result);
    //public List<TriggerFuncs> triggerFuncs;
    ///*
    //移动到checkPoint -> 获取Item
    //*/
    //#endregion

    #region movementFuncs
    public Transform P_Movement;
    public GameObject prefab_movement;
    [Serializable]
    public delegate bool MovementFuncs();
    public List<MovementFuncs> movementFuncs = new();
    /*
    移动到checkPoint
    搬起Item(movable)
    放下Item
    进入箱子
    退出箱子
    //用钥匙开锁
    //用石头垫脚
    */
    bool MoveTo(Vector2Int target)
    {
        return true;
    }
    bool LoadItem(Item item)
    {
        if (Solver.instance.item_load != null)
            return false;
        Solver.instance.item_load = item;

        movementFuncs.Add(UnloadItem);
        prefab_movement.name = nameof(UnloadItem) + "()";
        Instantiate(prefab_movement, P_Movement);

        Debug.Log("LoadItem :" + item.name);
        return true;
        
    }
    bool UnloadItem()
    {
        if (Solver.instance.item_load == null)
            return false;
        Debug.Log("RemoveFunc Unload = "  + movementFuncs.Remove(UnloadItem));
        RemoveChild_byName(P_Movement, nameof(UnloadItem) + "()");
        Debug.Log("UnloadItem :" + Solver.instance.item_load.name);
        Solver.instance.item_load = null;
        return true;
    }
    bool EnterBox(Box box)
    {
        
        Item load = Solver.instance.item_load;
        if (load == box)
            return false;
        Debug.Log("EnterBox :" + box.name);
        return true;
        if (load)
        {
            box.AddExistingItem(load);
            RemoveExistingItem(load);
        }
        box.P_Box = this;
        Solver.instance.box_curIn = box;
        box.movementFuncs.Add(ExitBox);
        prefab_movement.name = nameof(ExitBox) + "()";
        Instantiate(prefab_movement, box.P_Movement);

        return true;
    }
    bool ExitBox()
    {
        Debug.Log("ExitBox :" + name);
        Item load = Solver.instance.item_load;
        if (load)
        {
            P_Box.AddExistingItem(load);
            RemoveExistingItem(load);
        }
        Solver.instance.box_curIn = P_Box;
        movementFuncs.Remove(ExitBox);
        //TODO refresh existing item
        P_Box = null;
        return true;
    }
    bool StepOnStone()
    {
        return true;
    }
    bool ConvertItem(int index)
    {
        if (CanSatisfyConversion(index) == null)
            return false;
        Debug.Log("Convert id:" + index);
        if (conversions[index].item_rewards[0].name.Contains("Target"))
            Debug.Log("WIN!!");
        return true;
    }
    List<Item> CanSatisfyConversion(int index)
    {
        List<Item> ret = new();
        foreach(var sub in conversions[index].item_costs)
        {
            Item t = items_exsiting.Find(it => 
                                    it.id_in_library == sub.id_in_library
                                    &&
                                    it.canReach
                                    &&
                                    !ret.Contains(it)
                                );
            if (t == null)
                return null;
            ret.Add(t);
        }
        return ret;
    }
    #endregion
    public void AddInitItem_byName(string itemName)//初始添加
    {
        Item item = ItemManager.instance.GetItemByName(itemName);
        if (item == null)
            return;
        items_initial.Add(Instantiate(item,P_item_initial));
        
    }
    public void AddExistingItem(Item item)
    {
        //item.id_in_box = items_exsiting.Count - 1;
        items_exsiting.Add(Instantiate(item, P_item_existing));
        Item newItem = items_exsiting[^1];
        if (!newItem.canMove)
            return;
        movementFuncs.Add(delegate () { return LoadItem(newItem); });
        prefab_movement.name = nameof(LoadItem) + "(" + newItem.name+")";
        Instantiate(prefab_movement, P_Movement);
        Box newBox = newItem.GetComponent<Box>();
        if (newBox)
        {
            movementFuncs.Add(delegate () { return EnterBox(newBox); });
            //(Box.MovementFuncs)movementFuncs[0]
            prefab_movement.name = nameof(EnterBox) + "( " + newBox.name+ ")";
            Instantiate(prefab_movement, P_Movement);

            newBox.RefreshItems();
        }
            
    }
    public void RemoveExistingItem(Item item)
    {
        Destroy(P_item_existing.GetChild(GetIndexInList(items_exsiting, item)).gameObject);
        movementFuncs.Remove(delegate () { return LoadItem(item); });
        RemoveChild_byName(P_Movement, nameof(LoadItem) + "(" + item.name + ")");
    }
    public void RefreshItems()
    {
        foreach (var sub in items_exsiting)
            RemoveExistingItem(sub);
        items_exsiting.Clear();
        foreach (var sub in items_initial)
            AddExistingItem(sub);
    }
    public void AddConversion(Conversion conversion)
    {
        ClearChild(P_conversion);
        GameObject g = Instantiate(prefab_conversion, P_conversion);
        g.name = "Conversion_" + (conversions.Count).ToString();
        foreach(var sub in conversion.item_costs)
            Instantiate(sub, P_conversion.GetChild(conversions.Count).GetChild(0));
        foreach (var sub in conversion.item_rewards)
            Instantiate(sub, P_conversion.GetChild(conversions.Count).GetChild(1));
        conversions.Add(conversion);
        movementFuncs.Add(delegate () { return ConvertItem(conversions.Count-1);});
        prefab_movement.name = nameof(ConvertItem) + "_" + (conversions.Count - 1).ToString();
        Instantiate(prefab_movement, P_Movement);
    }
    //public void AddTrigger()
    //{

    //}
    void ClearChild(Transform p)
    {
        for (int i = 0; i < p.childCount; i++)
            Destroy(p.GetChild(i).gameObject);
    }
    void RemoveChild_byName(Transform p,string name)
    {
        for (int i = 0; i < p.childCount; i++)
            if(p.GetChild(i).name.Contains(name))
            {
                Destroy(p.GetChild(i).gameObject);
                return;
            }    
    }

    int GetIndexInList<T>(List<T> list,T elem)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i].Equals(elem))
                return i;
        return -1;
    }
}
