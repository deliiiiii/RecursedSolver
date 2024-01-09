using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

[Serializable]
public class ItemInfo
{
    public ItemInfo()
    {
        FakeAwake();
    }
    
    public string name;

    public bool isBox;
    public bool canReach;
    public bool canMove;

    //public int id_in_map;
    //public bool canExit;
    //public Box t_curBox;
    //public Item t_item_load;
    public ItemInfo P_Box=null;//Parent

    public List<Vector2Int> checkPoints;
    [SerializeReference]
    public List<ItemInfo> items_initial = new();
    [SerializeReference]
    public List<ItemInfo> items_exsiting = new();
    public List<Conversion> conversions = new();
    [Serializable]
    public class Conversion
    {
        [SerializeReference]
        public List<ItemInfo> item_costs;
        [SerializeReference]
        public List<ItemInfo> item_rewards;
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
    [Serializable]
    public delegate bool MovementFuncs();
    //public List<MovementFuncs> movementFuncs = new();
    //[JsonIgnore]
    public Dictionary<string, MovementFuncs> movementsDict;
    public Dictionary<string, int> dict2 = new()
    {
        { "1",111 },
        { "2",222 },
    };
    //public ObservableValue<Dictionary<string, MovementFuncs>, ItemInfo> movementsDict;
    public List <string> name_movementFuncs;
    /*
    移动到checkPoint
    搬起Item(movable)
    放下Item
    进入箱子
    退出箱子
    //用钥匙开锁
    //用石头垫脚
    */
    public void FakeAwake()
    {
        movementsDict = new();
        //movementsDict = new(movementsDict, this);
        name_movementFuncs = new();
    }
    bool MoveTo(Vector2Int target)
    {
        return true;
    }
    bool LoadItem(ItemInfo item)
    {
        if (Solver.instance.item_load != null)
            return false;
        Solver.instance.item_load = item;
        movementsDict.Add("UnloadItem", UnloadItem);
        name_movementFuncs.Add("UnloadItem");
        //movementFuncs.Add(UnloadItem);
        //prefab_movement.name = nameof(UnloadItem) + "()";
        //Instantiate(prefab_movement, P_Movement);

        Debug.Log("LoadItem :" + item.name);
        return true;
        
    }
    bool UnloadItem()
    {
        if (Solver.instance.item_load == null)
            return false;
        //Debug.Log("RemoveFunc Unload = "  + movementFuncs.Remove(UnloadItem));
        //RemoveChild_byName(P_Movement, nameof(UnloadItem) + "()");
        movementsDict.Remove("UnloadItem");
        name_movementFuncs.Remove("UnloadItem");
        Debug.Log("UnloadItem :" + Solver.instance.name);
        Solver.instance.item_load = null;
        return true;
    }
    bool EnterBox(ItemInfo box)
    {
        ItemInfo load = Solver.instance.item_load;
        if (load == box)
            return false;
        Debug.Log("EnterBox :" + box.name);
        if (load != null)
        {
            box.AddExistingItem(load);
            RemoveExistingItem(load);
        }
        box.P_Box = this;
        Solver.instance.box_curIn = box;
        box.movementsDict.Add("ExitBox", ExitBox);
        box.name_movementFuncs.Add("ExitBox");
        //prefab_movement.name = nameof(ExitBox) + "()";
        //Instantiate(prefab_movement, box.P_Movement);

        return true;
    }
    bool ExitBox()
    {
        Debug.Log("ExitBox :" + name);
        ItemInfo load = Solver.instance.item_load;
        if (load != null)
        {
            P_Box.AddExistingItem(load);
            RemoveExistingItem(load);
        }
        Solver.instance.box_curIn = P_Box;
        movementsDict.Remove("ExitBox");
        name_movementFuncs.Remove("ExitBox");
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
    List<ItemInfo> CanSatisfyConversion(int index)
    {
        List<ItemInfo> ret = new();
        foreach(var sub in conversions[index].item_costs)
        {
            ItemInfo t = items_exsiting.Find(it => 
                                            //it.id_in_library == sub.id_in_library
                                            //&&
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
    public void AddInitItem(string f_name)//初始添加
    {
        ItemInfo item = new()
        {
            name = f_name,
        };
        if (item.name == "Box")
        {
            item.isBox = true;
            item.canMove = true;
        }
        items_initial.Add(item);
    }
    public void AddExistingItem(ItemInfo item)
    {
        //item.id_in_box = items_exsiting.Count - 1;
        items_exsiting.Add(Clone.DeepCopy3(item));
        ItemInfo newItem = items_exsiting[^1];
        if (!newItem.canMove)
            return;
        movementsDict.Add("LoadItem" + item.name,
                          delegate () { return LoadItem(newItem); }
                          );
        name_movementFuncs.Add("LoadItem" + item.name);


        if (!newItem.isBox)
            return;
        movementsDict.Add("EnterBox" + newItem.name,
                          delegate () { return EnterBox(newItem); }
                          );
        name_movementFuncs.Add("EnterBox" + newItem.name);
        //TODO newItem.RefreshItems();

    }
    public void RemoveExistingItem(ItemInfo item)
    {
        movementsDict.Remove("LoadItem" + item.name);
        name_movementFuncs.Remove("LoadItem" + item.name);
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
        //ClearChild(P_conversion);
        //GameObject g = Instantiate(prefab_conversion, P_conversion);
        //g.name = "Conversion_" + (conversions.Count).ToString();
        //foreach(var sub in conversion.item_costs)
        //    Instantiate(sub, P_conversion.GetChild(conversions.Count).GetChild(0));
        //foreach (var sub in conversion.item_rewards)
        //    Instantiate(sub, P_conversion.GetChild(conversions.Count).GetChild(1));
        conversions.Add(conversion);
        //Debug.Log("Add : ConvertItem" + (conversions.Count - 1).ToString());
        movementsDict.Add("ConvertItem"+ (conversions.Count - 1).ToString(),
            delegate () { return ConvertItem(conversions.Count-1);});
        name_movementFuncs.Add("ConvertItem" + (conversions.Count - 1).ToString());
        //prefab_movement.name = nameof(ConvertItem) + "_" + (conversions.Count - 1).ToString();
        //Instantiate(prefab_movement, P_Movement);
    }
    //public void AddTrigger()
    //{

    //}
    //void ClearChild(Transform p)
    //{
    //    for (int i = 0; i < p.childCount; i++)
    //        Destroy(p.GetChild(i).gameObject);
    //}
    //void RemoveChild_byName(Transform p,string name)
    //{
    //    for (int i = 0; i < p.childCount; i++)
    //        if(p.GetChild(i).name.Contains(name))
    //        {
    //            DestroyImmediate(p.GetChild(i).gameObject);
    //            //Debug.Log("Removed :" +  name);
    //            return;
    //        }    
    //}

    static bool hasGot = false;
    public void RefreshMovementsFuncName()
    {
        if (hasGot)
            return;
        hasGot = true;
        Debug.Log(" RefreshMovementsFuncName() count = " + movementsDict.Keys.Count);
        name_movementFuncs.Clear();
        foreach (var funcName in movementsDict.Keys)
            name_movementFuncs.Add(funcName);
        hasGot = false;
    }
    int GetIndexInList<T>(List<T> list,T elem)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i].Equals(elem))
                return i;
        return -1;
    }
}
