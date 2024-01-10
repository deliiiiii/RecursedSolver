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
    //public bool canReach;
    public bool canMove;

    public int id_in_library;
    public int id_total;

    //public List<Vector2Int> checkPoints;
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
    public SerializableDictionary<string, int> movementsDict;
    /*
    移动到checkPoint
    //用石头垫脚
    */
    public void FakeAwake()
    {
        movementsDict = new();
        id_in_library = -1;
    }
    public bool ExecuteMove(string funcName,int idPara)
    {
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
        if (Solver.instance.item_load[Solver.depth].id_in_library != -1)
            return false;
        movementsDict.Remove(nameof(LoadItem)+id.ToString());
        movementsDict.Add(nameof(UnloadItem), -1);
        Solver.instance.item_load[Solver.depth] = Clone.DeepCopy1(items_exsiting.Find(it => it.id_total == id));
        if (Solver.instance.item_load[Solver.depth].isBox)
            movementsDict.Remove(nameof(EnterBox) + id.ToString());
        Debug.Log("LoadItem :" + id);
        return true;
    }
    bool UnloadItem()
    {
        ItemInfo load = Solver.instance.item_load[Solver.depth];
        if (load.id_in_library == -1)
            return false;
        int id = load.id_total;
        if (load.isBox)
            movementsDict.Add(nameof(EnterBox) + id.ToString(),id);
        movementsDict.Remove(nameof(UnloadItem));
        movementsDict.Add(nameof(LoadItem) + id.ToString(), id);
        Solver.instance.item_load[Solver.depth] = new() { id_in_library = -1 };
        Debug.Log("UnloadItem :" + id);
        return true;
    }
    bool EnterBox(int id)
    {
        ItemInfo load = Solver.instance.item_load[Solver.depth];
        ItemInfo newBox = items_exsiting.Find(it=>it.id_total == id);
        Solver.instance.box_parent[Solver.depth] = Clone.DeepCopy1(this);
        Solver.instance.box_curIn[Solver.depth] = newBox;
        movementsDict.Remove(nameof(EnterBox)+id.ToString());
        newBox.movementsDict.Add(nameof(ExitBox),-1);
        newBox.RefreshItems();
        if (load.id_in_library != -1)
        {
            Debug.Log("a");
            newBox.AddExistingItem(load);
            Debug.Log("b");
            RemoveExistingItem(load);
            Debug.Log("c");
        }
        Debug.Log("EnterBox :" + newBox.id_total);
        return true;
    }
    bool ExitBox()
    {
        ItemInfo load = Solver.instance.item_load[Solver.depth];
        if (load.id_in_library != -1)
        {
            Solver.instance.box_parent[Solver.depth].AddExistingItem(load);
            RemoveExistingItem(load);
        }
        Solver.instance.box_curIn[Solver.depth] = Clone.DeepCopy1(Solver.instance.box_parent[Solver.depth]);
        //TODO -1 movementsDict.Value.Remove(nameof(ExitBox));
        //TODO -1 refresh existing item
        Solver.instance.box_parent[Solver.depth] = new() { id_in_library = -1 };
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
        if (Solver.instance.item_load[Solver.depth] == item)
        {
            items_exsiting.Add(item);
            movementsDict.Add(nameof(UnloadItem) + item.id_total, item.id_total);
            return;
        }
        ItemInfo newItem = Clone.DeepCopy1(item);
        items_exsiting.Add(newItem);
        
        newItem.id_total = Solver.id_usedTotal;
        Solver.id_usedTotal++;
        if (!newItem.canMove)
            return;
        movementsDict.Add(nameof(LoadItem) + newItem.id_total, newItem.id_total);
        if (!newItem.isBox)
            return;
        movementsDict.Add(nameof(EnterBox) + newItem.id_total, newItem.id_total);
        //TODO newItem.RefreshItems();
    }
    public void RemoveExistingItem(ItemInfo item)
    {
        if (Solver.instance.item_load[Solver.depth] == item)
        {
            movementsDict.Remove(nameof(UnloadItem));
        }
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
        int c = 0;
        while(items_exsiting.Count != 0)
        {
            c++;
            Debug.Log("c = " + c); 
            RemoveExistingItem(items_exsiting[0]);
            if (c > 10)
                break;
        }
           
        foreach (var it in items_initial)
            AddExistingItem(it);
    }
    public void AddConversion(Conversion conversion)
    {
        movementsDict.Add(nameof(ConvertItem)+ conversions.Count.ToString(), conversions.Count);
        conversions.Add(conversion);
    }

    //public void AddTrigger()
    //{

    //}
    //int GetIndexInList<T>(List<T> list,T elem)
    //{
    //    for (int i = 0; i < list.Count; i++)
    //        if (list[i].Equals(elem))
    //            return i;
    //    return -1;
    //}
}
