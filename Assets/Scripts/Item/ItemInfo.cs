using System;
using System.Collections.Generic;
using UnityEngine;

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

    public int id_in_library;
    public int id_total;

    //public int id_in_map;
    //public bool canExit;
    //public Box t_curBox;
    //public Item t_item_load;

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
    //[Serializable]
    //public delegate bool MovementFuncs();
    //public List<MovementFuncs> movementFuncs = new();
    //public List <string> name_movementFuncs;
    //public List <int> paraId_movementFuncs;
    public SerializableDictionary<string, int> movementsDict;
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
    }
    public bool ExecuteMove(string funcName,int idPara)
    {
        //Debug.Log("ExecuteMove " + funcName + " " + idPara);
        if(funcName.Contains(nameof(LoadItem)))
        {
            return LoadItem(idPara);
        }
        if(funcName.Contains(nameof(UnloadItem)))
        {
            return UnloadItem();
        }
        if (funcName.Contains(nameof(EnterBox)))
        {
            return EnterBox(idPara);
        }
        if (funcName.Contains(nameof(ExitBox)))
        {
            return ExitBox();
        }
        if (funcName.Contains(nameof(ConvertItem)))
        {
            return ConvertItem(idPara);
        }
        return false;
    }
    bool MoveTo(Vector2Int target)
    {
        return true;
    }
    bool LoadItem(int id)
    {
        ItemInfo load = Solver.instance.item_load;
        if (load.id_in_library != -1)
            return false;
        movementsDict.Remove(nameof(LoadItem)+id.ToString());
        movementsDict.Add(nameof(UnloadItem), -1);
        Solver.instance.item_load = Clone.DeepCopy1(items_exsiting.Find(it => it.id_total == id));
        if (Solver.instance.item_load.isBox)
            movementsDict.Remove(nameof(EnterBox) + id.ToString());
        Debug.Log("LoadItem :" + id_total);
        return true;
    }
    bool UnloadItem()
    {
        ItemInfo load = Solver.instance.item_load;
        if (load.id_in_library == -1)
            return false;
        int id = load.id_total;
        if (load.isBox)
            movementsDict.Add(nameof(EnterBox) + id.ToString(),id);
        movementsDict.Remove(nameof(UnloadItem));
        movementsDict.Add(nameof(LoadItem) + id.ToString(), id);
        Solver.instance.item_load = new() { id_in_library = -1 };
        Debug.Log("UnloadItem :" + id);
        return true;
    }
    bool EnterBox(int id)
    {
        ItemInfo load = Solver.instance.item_load;
        ItemInfo newBox = items_exsiting.Find(it=>it.id_total == id);
        if (load.id_in_library != -1)
        {
            newBox.AddExistingItem(load);
            RemoveExistingItem(load);
        }
        Solver.instance.box_parent = this;
        Solver.instance.box_curIn = newBox;
        movementsDict.Remove(nameof(EnterBox)+id.ToString());
        newBox.movementsDict.Add(nameof(ExitBox),-1);

        Solver.instance.RefreshBox(newBox);
        Debug.Log("EnterBox :" + newBox.id_total);
        return true;
    }
    bool ExitBox()
    {
        ItemInfo load = Solver.instance.item_load;
        if (load.id_in_library != -1)
        {
            Solver.instance.box_parent.AddExistingItem(load);
            RemoveExistingItem(load);
        }
        Solver.instance.box_curIn = Solver.instance.box_parent;
        //TODO -1 movementsDict.Value.Remove(nameof(ExitBox));
        //TODO -1 refresh existing item
        Solver.instance.box_parent = null;
        Debug.Log("ExitBox");
        return true;
    }
    bool StepOnStone()
    {
        return true;
    }
    bool ConvertItem(int index)
    {
        if (!CanSatisfyConversion(index))
            return false;
        movementsDict.Remove(nameof(ConvertItem)+index);
        Debug.Log("Convert id:" + index);
        if (conversions[index].item_rewards[0].name.Contains("Target"))
            Debug.Log("WIN!!");
        return true;
    }
    bool CanSatisfyConversion(int index)
    {
        List<ItemInfo> ret = new();
        foreach(var cost in conversions[index].item_costs)
        {
            ItemInfo t = items_exsiting.Find(it => 
                                            it.name == cost.name
                                            //&&
                                            //it.canReach
                                            &&
                                            !ret.Contains(it)
                                        );
            if (t == null)
                return false;
            ret.Add(t);
        }
        return true;
    }
    #endregion
    public void AddInitItem(string f_name)//初始添加
    {
        ItemInfo item = new()
        {
            name = f_name,
        };
        switch(item.name)
        {
            case "Box":
                item.id_in_library = 0;
                item.isBox = true;
                item.canMove = true;
                break;
            case "Key":
                item.id_in_library = 1;
                item.canMove = true;
                break;
            case "Stone":
                item.id_in_library = 2;
                item.canMove = true;
                break;
            case "Target":
                item.id_in_library = 3;
                item.canMove = true;
                break;
            default:
                break;
        }
        items_initial.Add(item);
    }
    public void AddExistingItem(ItemInfo item)
    {
        item.id_total = Solver.id_usedTotal;
        Solver.id_usedTotal++;
        ItemInfo newItem = Clone.DeepCopy1(item);
        items_exsiting.Add(newItem);
        if (!newItem.canMove)
            return;
        movementsDict.Add(nameof(LoadItem) + item.id_total,item.id_total);
        if (!newItem.isBox)
            return;
        movementsDict.Add(nameof(EnterBox) + item.id_total, item.id_total);
        //TODO newItem.RefreshItems();
    }
    public void RemoveExistingItem(ItemInfo item)
    {
        int id = item.id_total;
        if (!item.canMove)
            return;
        movementsDict.Remove(nameof(LoadItem) + id);
        if (!item.isBox)
            return;
        movementsDict.Remove(nameof(EnterBox) + id);
        items_exsiting.Remove(items_exsiting.Find(it => it.id_total == id));
    }
    public void RefreshItems()
    {
        while(items_exsiting.Count != 0)
            RemoveExistingItem(items_exsiting[0]);
        foreach (var it in items_initial)
            AddExistingItem(it);
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
        
        //Debug.Log("Add : ConvertItem" + (conversions.Count - 1).ToString());
        movementsDict.Add(nameof(ConvertItem)+ conversions.Count.ToString(), conversions.Count);
        conversions.Add(conversion);
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

    //static bool hasGot = false;
    //public void RefreshMovementsFuncName()
    //{
    //    if (hasGot)
    //        return;
    //    hasGot = true;
    //    Debug.Log(" RefreshMovementsFuncName() count = " + movementsDict.Keys.Count);
    //    name_movementFuncs.Clear();
    //    foreach (var funcName in movementsDict.Keys)
    //        name_movementFuncs.Add(funcName);
    //    hasGot = false;
    //}
    //int GetIndexInList<T>(List<T> list,T elem)
    //{
    //    for (int i = 0; i < list.Count; i++)
    //        if (list[i].Equals(elem))
    //            return i;
    //    return -1;
    //}
}
